using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class World
{
    public static string cachedAppPath = "";

    public string name;
    public int seed;
    public float time;
    public int versionId;
    public WorldTemplate template;

    public World(string name, int seed)
    {
        this.name = name;
        this.seed = seed;
        this.versionId = VersionController.CurrentVersionId;
        this.template = WorldTemplate.Default;
    }

    public World()
    {
        this.versionId = VersionController.CurrentVersionId;
        this.template = WorldTemplate.Default;
    }

    public static string appPath
    {
        get
        {
            if (cachedAppPath != "")
                return cachedAppPath;

            cachedAppPath = Application.dataPath;
            return cachedAppPath;
        }
    }

    public static World LoadWorld(string name)
    {
        World world = new World(name, 0);
        try
        {
            Dictionary<string, string> worldData = new Dictionary<string, string>();
            string[] data = File.ReadAllLines(world.GetPath() + "\\level.dat");
            foreach (string dataLine in data)
                worldData.Add(dataLine.Split('=')[0], dataLine.Split('=')[1]);

            world.seed = int.Parse(worldData["seed"]);
            world.time = float.Parse(worldData["time"]);
            world.versionId = int.Parse(worldData["versionId"]);
            world.template = (WorldTemplate) Enum.Parse(typeof(WorldTemplate), worldData["template"]);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to load world properties, error: " + e.Message + e.StackTrace);
        }

        return world;
    }

    public static bool WorldExists(string name)
    {
        return File.Exists(new World(name, 0).GetPath() + "\\level.dat");
    }

    public static List<World> LoadWorlds()
    {
        List<World> worlds = new List<World>();

        foreach (string worldName in Directory.GetDirectories(GetSavesPath()))
            worlds.Add(LoadWorld(worldName.Split('\\')[worldName.Split('\\').Length - 1]));

        return worlds;
    }

    public void Delete()
    {
        Directory.Delete(GetPath(), true);
    }

    public void SaveData()
    {
        if (!Directory.Exists(GetPath()))
            Directory.CreateDirectory(GetPath());
        if (!Directory.Exists(GetPath() + "\\chunks"))
            Directory.CreateDirectory(GetPath() + "\\chunks");
        if (!Directory.Exists(GetPath() + "\\chunks\\Overworld"))
            Directory.CreateDirectory(GetPath() + "\\chunks\\Overworld");
        if (!Directory.Exists(GetPath() + "\\chunks"))
            Directory.CreateDirectory(GetPath() + "\\players");
        if (!Directory.Exists(GetPath() + "\\inventories"))
            Directory.CreateDirectory(GetPath() + "\\inventories");

        if (!File.Exists(GetPath() + "\\level.dat"))
            File.Create(GetPath() + "\\level.dat").Close();

        List<string> data = new List<string>();

        data.Add("seed=" + seed);
        data.Add("time=" + time);
        data.Add("versionId=" + versionId);
        data.Add("template=" + template);

        File.WriteAllLines(GetPath() + "\\level.dat", data);
    }

    public static string GetSavesPath()
    {
        string savesPath = appPath + "\\..\\Saves\\";

        if (!Directory.Exists(savesPath))
            Directory.CreateDirectory(savesPath);
        return savesPath;
    }

    public string GetPath()
    {
        return GetSavesPath() + name;
    }

    public float GetDiskSize()
    {
        return CalculateFolderSize(GetPath());
    }

    protected static float CalculateFolderSize(string folder)
    {
        float folderSize = 0.0f;
        //Checks if the path is valid or not
        if (!Directory.Exists(folder))
            return folderSize;

        foreach (string file in Directory.GetFiles(folder))
            if (File.Exists(file))
            {
                FileInfo finfo = new FileInfo(file);
                folderSize += finfo.Length;
            }

        foreach (string dir in Directory.GetDirectories(folder))
            folderSize += CalculateFolderSize(dir);
        return folderSize;
    }
}