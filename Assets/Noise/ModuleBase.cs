using System;
using System.Xml.Serialization;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace LibNoise
{
    #region Enumerations

    /// <summary>
    ///     Defines a collection of quality modes.
    /// </summary>
    public enum QualityMode
    {
        Low,
        Medium,
        High
    }

    #endregion

    /// <summary>
    ///     Base class for noise modules.
    /// </summary>
    public abstract class ModuleBase : IDisposable
    {
        #region Fields

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of Helpers.
        /// </summary>
        /// <param name="count">The number of source modules.</param>
        protected ModuleBase(int count)
        {
            if (count > 0) Modules = new ModuleBase[count];
        }

        #endregion

        #region Indexers

        /// <summary>
        ///     Gets or sets a source module by index.
        /// </summary>
        /// <param name="index">The index of the source module to aquire.</param>
        /// <returns>The requested source module.</returns>
        public virtual ModuleBase this[int index]
        {
            get
            {
                Debug.Assert(Modules != null);
                Debug.Assert(Modules.Length > 0);
                if (index < 0 || index >= Modules.Length)
                    throw new ArgumentOutOfRangeException("Index out of valid module range");
                if (Modules[index] == null) throw new ArgumentNullException("Desired element is null");
                return Modules[index];
            }
            set
            {
                Debug.Assert(Modules.Length > 0);
                if (index < 0 || index >= Modules.Length)
                    throw new ArgumentOutOfRangeException("Index out of valid module range");
                if (value == null) throw new ArgumentNullException("Value should not be null");
                Modules[index] = value;
            }
        }

        #endregion

        #region Properties

        protected ModuleBase[] Modules { get; private set; }

        /// <summary>
        ///     Gets the number of source modules required by this noise module.
        /// </summary>
        public int SourceModuleCount => Modules == null ? 0 : Modules.Length;

        #endregion

        #region Methods

        /// <summary>
        ///     Returns the output value for the given input coordinates.
        /// </summary>
        /// <param name="x">The input coordinate on the x-axis.</param>
        /// <param name="y">The input coordinate on the y-axis.</param>
        /// <param name="z">The input coordinate on the z-axis.</param>
        /// <returns>The resulting output value.</returns>
        public abstract double GetValue(double x, double y, double z);

        /// <summary>
        ///     Returns the output value for the given input coordinates.
        /// </summary>
        /// <param name="coordinate">The input coordinate.</param>
        /// <returns>The resulting output value.</returns>
        public double GetValue(Vector3 coordinate)
        {
            return GetValue(coordinate.x, coordinate.y, coordinate.z);
        }

        /// <summary>
        ///     Returns the output value for the given input coordinates.
        /// </summary>
        /// <param name="coordinate">The input coordinate.</param>
        /// <returns>The resulting output value.</returns>
        public double GetValue(ref Vector3 coordinate)
        {
            return GetValue(coordinate.x, coordinate.y, coordinate.z);
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        ///     Gets a value whether the object is disposed.
        /// </summary>
        [field: XmlIgnore]
        [field: NonSerialized]
        public bool IsDisposed { get; private set; }

        /// <summary>
        ///     Immediately releases the unmanaged resources used by this object.
        /// </summary>
        public void Dispose()
        {
            if (!IsDisposed) IsDisposed = Disposing();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Immediately releases the unmanaged resources used by this object.
        /// </summary>
        /// <returns>True if the object is completely disposed.</returns>
        protected virtual bool Disposing()
        {
            if (Modules != null)
            {
                for (var i = 0; i < Modules.Length; i++)
                {
                    Modules[i].Dispose();
                    Modules[i] = null;
                }

                Modules = null;
            }

            return true;
        }

        #endregion
    }
}