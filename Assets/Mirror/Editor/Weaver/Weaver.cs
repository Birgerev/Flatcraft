using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Mono.CecilX;
using UnityEngine;

namespace Mirror.Weaver
{
    // This data is flushed each time - if we are run multiple times in the same process/domain
    internal class WeaverLists
    {
        // amount of SyncVars per class. dict<className, amount>
        public Dictionary<string, int> numSyncVars = new Dictionary<string, int>();

        // getter functions that replace [SyncVar] member variable references. dict<field, replacement>
        public Dictionary<FieldDefinition, MethodDefinition> replacementGetterProperties =
            new Dictionary<FieldDefinition, MethodDefinition>();

        // setter functions that replace [SyncVar] member variable references. dict<field, replacement>
        public Dictionary<FieldDefinition, MethodDefinition> replacementSetterProperties =
            new Dictionary<FieldDefinition, MethodDefinition>();

        public int GetSyncVarStart(string className)
        {
            return numSyncVars.ContainsKey(className)
                ? numSyncVars[className]
                : 0;
        }

        public void SetNumSyncVars(string className, int num)
        {
            numSyncVars[className] = num;
        }
    }

    internal static class Weaver
    {
        // generated code class
        public const string GeneratedCodeNamespace = "Mirror";
        public const string GeneratedCodeClassName = "GeneratedNetworkCode";
        public static TypeDefinition GeneratedCodeClass;
        public static bool GenerateLogErrors;

        // private properties
        private static readonly bool DebugLogEnabled = true;
        public static string InvokeRpcPrefix => "InvokeUserCode_";

        public static WeaverLists WeaveLists { get; private set; }
        public static AssemblyDefinition CurrentAssembly { get; private set; }
        public static bool WeavingFailed { get; private set; }

        public static void DLog(TypeDefinition td, string fmt, params object[] args)
        {
            if (!DebugLogEnabled)
                return;

            Console.WriteLine("[" + td.Name + "] " + string.Format(fmt, args));
        }

        // display weaver error
        // and mark process as failed
        public static void Error(string message)
        {
            Log.Error(message);
            WeavingFailed = true;
        }

        public static void Error(string message, MemberReference mr)
        {
            Log.Error($"{message} (at {mr})");
            WeavingFailed = true;
        }

        public static void Warning(string message, MemberReference mr)
        {
            Log.Warning($"{message} (at {mr})");
        }


        private static void CheckMonoBehaviour(TypeDefinition td)
        {
            if (td.IsDerivedFrom<MonoBehaviour>())
                MonoBehaviourProcessor.Process(td);
        }

        private static bool WeaveNetworkBehavior(TypeDefinition td)
        {
            if (!td.IsClass)
                return false;

            if (!td.IsDerivedFrom<NetworkBehaviour>())
            {
                CheckMonoBehaviour(td);
                return false;
            }

            // process this and base classes from parent to child order

            List<TypeDefinition> behaviourClasses = new List<TypeDefinition>();

            TypeDefinition parent = td;
            while (parent != null)
            {
                if (parent.Is<NetworkBehaviour>())
                    break;

                try
                {
                    behaviourClasses.Insert(0, parent);
                    parent = parent.BaseType.Resolve();
                }
                catch (AssemblyResolutionException)
                {
                    // this can happen for plugins.
                    //Console.WriteLine("AssemblyResolutionException: "+ ex.ToString());
                    break;
                }
            }

            bool modified = false;
            foreach (TypeDefinition behaviour in behaviourClasses)
                modified |= new NetworkBehaviourProcessor(behaviour).Process();
            return modified;
        }

        private static bool WeaveModule(ModuleDefinition moduleDefinition)
        {
            try
            {
                bool modified = false;

                Stopwatch watch = Stopwatch.StartNew();

                watch.Start();
                foreach (TypeDefinition td in moduleDefinition.Types)
                    if (td.IsClass && td.BaseType.CanBeResolved())
                    {
                        modified |= WeaveNetworkBehavior(td);
                        modified |= ServerClientAttributeProcessor.Process(td);
                    }

                watch.Stop();
                Console.WriteLine("Weave behaviours and messages took" + watch.ElapsedMilliseconds + " milliseconds");

                return modified;
            }
            catch (Exception ex)
            {
                Error(ex.ToString());
                throw new Exception(ex.Message, ex);
            }
        }

        private static void CreateGeneratedCodeClass()
        {
            // create "Mirror.GeneratedNetworkCode" class
            GeneratedCodeClass = new TypeDefinition(GeneratedCodeNamespace, GeneratedCodeClassName,
                TypeAttributes.BeforeFieldInit | TypeAttributes.Class | TypeAttributes.AnsiClass |
                TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.Abstract | TypeAttributes.Sealed,
                WeaverTypes.Import<object>());
        }

        private static bool ContainsGeneratedCodeClass(ModuleDefinition module)
        {
            return module.GetTypes().Any(td => td.Namespace == GeneratedCodeNamespace &&
                                               td.Name == GeneratedCodeClassName);
        }

        private static bool Weave(string assName, IEnumerable<string> dependencies)
        {
            using (DefaultAssemblyResolver asmResolver = new DefaultAssemblyResolver())
            using (CurrentAssembly = AssemblyDefinition.ReadAssembly(assName
                , new ReaderParameters {ReadWrite = true, ReadSymbols = true, AssemblyResolver = asmResolver}))
            {
                asmResolver.AddSearchDirectory(Path.GetDirectoryName(assName));
                asmResolver.AddSearchDirectory(Helpers.UnityEngineDllDirectoryName());
                if (dependencies != null)
                    foreach (string path in dependencies)
                        asmResolver.AddSearchDirectory(path);

                // fix "No writer found for ..." error
                // https://github.com/vis2k/Mirror/issues/2579
                // -> when restarting Unity, weaver would try to weave a DLL
                //    again
                // -> resulting in two GeneratedNetworkCode classes (see ILSpy)
                // -> the second one wouldn't have all the writer types setup
                if (ContainsGeneratedCodeClass(CurrentAssembly.MainModule))
                    //Log.Warning($"Weaver: skipping {CurrentAssembly.Name} because already weaved");
                    return true;

                WeaverTypes.SetupTargetTypes(CurrentAssembly);

                CreateGeneratedCodeClass();

                // WeaverList depends on WeaverTypes setup because it uses Import
                WeaveLists = new WeaverLists();

                Stopwatch rwstopwatch = Stopwatch.StartNew();
                // Need to track modified from ReaderWriterProcessor too because it could find custom read/write functions or create functions for NetworkMessages
                bool modified = ReaderWriterProcessor.Process(CurrentAssembly);
                rwstopwatch.Stop();
                Console.WriteLine($"Find all reader and writers took {rwstopwatch.ElapsedMilliseconds} milliseconds");

                ModuleDefinition moduleDefinition = CurrentAssembly.MainModule;
                Console.WriteLine($"Script Module: {moduleDefinition.Name}");

                modified |= WeaveModule(moduleDefinition);

                if (WeavingFailed)
                    return false;

                if (modified)
                {
                    PropertySiteProcessor.Process(moduleDefinition);

                    // add class that holds read/write functions
                    moduleDefinition.Types.Add(GeneratedCodeClass);

                    ReaderWriterProcessor.InitializeReaderAndWriters(CurrentAssembly);

                    // write to outputDir if specified, otherwise perform in-place write
                    WriterParameters writeParams = new WriterParameters {WriteSymbols = true};
                    CurrentAssembly.Write(writeParams);
                }
            }

            return true;
        }

        public static bool WeaveAssembly(string assembly, IEnumerable<string> dependencies)
        {
            WeavingFailed = false;

            try
            {
                return Weave(assembly, dependencies);
            }
            catch (Exception e)
            {
                Log.Error("Exception :" + e);
                return false;
            }
        }
    }
}