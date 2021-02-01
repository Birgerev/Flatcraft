using System;
using UnityEngine;

namespace LibNoise.Generator
{
    /// <summary>
    ///     Provides a noise module that outputs a three-dimensional billowy noise. [GENERATOR]
    /// </summary>
    public class Billow : ModuleBase
    {
        #region ModuleBase Members

        /// <summary>
        ///     Returns the output value for the given input coordinates.
        /// </summary>
        /// <param name="x">The input coordinate on the x-axis.</param>
        /// <param name="y">The input coordinate on the y-axis.</param>
        /// <param name="z">The input coordinate on the z-axis.</param>
        /// <returns>The resulting output value.</returns>
        public override double GetValue(double x, double y, double z)
        {
            var value = 0.0;
            var curp = 1.0;
            x *= Frequency;
            y *= Frequency;
            z *= Frequency;
            for (var i = 0; i < _octaveCount; i++)
            {
                var nx = Utils.MakeInt32Range(x);
                var ny = Utils.MakeInt32Range(y);
                var nz = Utils.MakeInt32Range(z);
                var seed = (Seed + i) & 0xffffffff;
                var signal = Utils.GradientCoherentNoise3D(nx, ny, nz, seed, Quality);
                signal = 2.0 * Math.Abs(signal) - 1.0;
                value += signal * curp;
                x *= Lacunarity;
                y *= Lacunarity;
                z *= Lacunarity;
                curp *= Persistence;
            }

            return value + 0.5;
        }

        #endregion

        #region Fields

        private int _octaveCount = 6;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of Billow.
        /// </summary>
        public Billow()
            : base(0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of Billow.
        /// </summary>
        /// <param name="frequency">The frequency of the first octave.</param>
        /// <param name="lacunarity">The lacunarity of the billowy noise.</param>
        /// <param name="persistence">The persistence of the billowy noise.</param>
        /// <param name="octaves">The number of octaves of the billowy noise.</param>
        /// <param name="seed">The seed of the billowy noise.</param>
        /// <param name="quality">The quality of the billowy noise.</param>
        public Billow(double frequency, double lacunarity, double persistence, int octaves, int seed,
            QualityMode quality)
            : base(0)
        {
            Frequency = frequency;
            Lacunarity = lacunarity;
            OctaveCount = octaves;
            Persistence = persistence;
            Seed = seed;
            Quality = quality;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the frequency of the first octave.
        /// </summary>
        public double Frequency { get; set; } = 1.0;

        /// <summary>
        ///     Gets or sets the lacunarity of the billowy noise.
        /// </summary>
        public double Lacunarity { get; set; } = 2.0;

        /// <summary>
        ///     Gets or sets the quality of the billowy noise.
        /// </summary>
        public QualityMode Quality { get; set; } = QualityMode.Medium;

        /// <summary>
        ///     Gets or sets the number of octaves of the billowy noise.
        /// </summary>
        public int OctaveCount
        {
            get => _octaveCount;
            set => _octaveCount = Mathf.Clamp(value, 1, Utils.OctavesMaximum);
        }

        /// <summary>
        ///     Gets or sets the persistence of the billowy noise.
        /// </summary>
        public double Persistence { get; set; } = 0.5;

        /// <summary>
        ///     Gets or sets the seed of the billowy noise.
        /// </summary>
        public int Seed { get; set; }

        #endregion
    }
}