﻿using System;
using UnityEngine;

namespace LibNoise.Generator
{
    /// <summary>
    ///     Provides a noise module that outputs 3-dimensional ridged-multifractal noise. [GENERATOR]
    /// </summary>
    public class RidgedMultifractal : ModuleBase
    {
        #region Methods

        /// <summary>
        ///     Updates the weights of the ridged-multifractal noise.
        /// </summary>
        private void UpdateWeights()
        {
            double f = 1.0;
            for (int i = 0; i < Utils.OctavesMaximum; i++)
            {
                _weights[i] = Math.Pow(f, -1.0);
                f *= _lacunarity;
            }
        }

        #endregion

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
            x *= Frequency;
            y *= Frequency;
            z *= Frequency;
            double value = 0.0;
            double weight = 1.0;
            double offset = 1.0;
            double gain = 2.0;
            for (int i = 0; i < _octaveCount; i++)
            {
                double nx = Utils.MakeInt32Range(x);
                double ny = Utils.MakeInt32Range(y);
                double nz = Utils.MakeInt32Range(z);
                long seed = (Seed + i) & 0x7fffffff;
                double signal = Utils.GradientCoherentNoise3D(nx, ny, nz, seed, Quality);
                signal = Math.Abs(signal);
                signal = offset - signal;
                signal *= signal;
                signal *= weight;
                weight = signal * gain;
                weight = Mathf.Clamp01((float) weight);
                value += signal * _weights[i];
                x *= _lacunarity;
                y *= _lacunarity;
                z *= _lacunarity;
            }

            return value * 1.25 - 1.0;
        }

        #endregion

        #region Fields

        private double _lacunarity = 2.0;
        private int _octaveCount = 6;
        private readonly double[] _weights = new double[Utils.OctavesMaximum];

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of RidgedMultifractal.
        /// </summary>
        public RidgedMultifractal()
            : base(0)
        {
            UpdateWeights();
        }

        /// <summary>
        ///     Initializes a new instance of RidgedMultifractal.
        /// </summary>
        /// <param name="frequency">The frequency of the first octave.</param>
        /// <param name="lacunarity">The lacunarity of the ridged-multifractal noise.</param>
        /// <param name="octaves">The number of octaves of the ridged-multifractal noise.</param>
        /// <param name="seed">The seed of the ridged-multifractal noise.</param>
        /// <param name="quality">The quality of the ridged-multifractal noise.</param>
        public RidgedMultifractal(double frequency, double lacunarity, int octaves, int seed, QualityMode quality)
            : base(0)
        {
            Frequency = frequency;
            Lacunarity = lacunarity;
            OctaveCount = octaves;
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
        ///     Gets or sets the lacunarity of the ridged-multifractal noise.
        /// </summary>
        public double Lacunarity
        {
            get => _lacunarity;
            set
            {
                _lacunarity = value;
                UpdateWeights();
            }
        }

        /// <summary>
        ///     Gets or sets the quality of the ridged-multifractal noise.
        /// </summary>
        public QualityMode Quality { get; set; } = QualityMode.Medium;

        /// <summary>
        ///     Gets or sets the number of octaves of the ridged-multifractal noise.
        /// </summary>
        public int OctaveCount
        {
            get => _octaveCount;
            set => _octaveCount = Mathf.Clamp(value, 1, Utils.OctavesMaximum);
        }

        /// <summary>
        ///     Gets or sets the seed of the ridged-multifractal noise.
        /// </summary>
        public int Seed { get; set; }

        #endregion
    }
}