using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace LibNoise.Operator
{
    /// <summary>
    ///     Provides a noise module that maps the output value from a source module onto a
    ///     terrace-forming curve. [OPERATOR]
    /// </summary>
    public class Terrace : ModuleBase
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
            Debug.Assert(Modules[0] != null);
            Debug.Assert(ControlPointCount >= 2);
            double smv = Modules[0].GetValue(x, y, z);
            int ip;
            for (ip = 0; ip < ControlPoints.Count; ip++)
                if (smv < ControlPoints[ip])
                    break;
            int i0 = Mathf.Clamp(ip - 1, 0, ControlPoints.Count - 1);
            int i1 = Mathf.Clamp(ip, 0, ControlPoints.Count - 1);
            if (i0 == i1)
                return ControlPoints[i1];
            double v0 = ControlPoints[i0];
            double v1 = ControlPoints[i1];
            double a = (smv - v0) / (v1 - v0);
            if (IsInverted)
            {
                a = 1.0 - a;
                double t = v0;
                v0 = v1;
                v1 = t;
            }

            a *= a;
            return Utils.InterpolateLinear(v0, v1, a);
        }

        #endregion

        #region Fields

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of Terrace.
        /// </summary>
        public Terrace()
            : base(1)
        {
        }

        /// <summary>
        ///     Initializes a new instance of Terrace.
        /// </summary>
        /// <param name="input">The input module.</param>
        public Terrace(ModuleBase input)
            : base(1)
        {
            Modules[0] = input;
        }

        /// <summary>
        ///     Initializes a new instance of Terrace.
        /// </summary>
        /// <param name="inverted">Indicates whether the terrace curve is inverted.</param>
        /// <param name="input">The input module.</param>
        public Terrace(bool inverted, ModuleBase input)
            : base(1)
        {
            Modules[0] = input;
            IsInverted = inverted;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the number of control points.
        /// </summary>
        public int ControlPointCount => ControlPoints.Count;

        /// <summary>
        ///     Gets the list of control points.
        /// </summary>
        public List<double> ControlPoints { get; } = new List<double>();

        /// <summary>
        ///     Gets or sets a value whether the terrace curve is inverted.
        /// </summary>
        public bool IsInverted { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Adds a control point to the curve.
        /// </summary>
        /// <param name="input">The curves input value.</param>
        public void Add(double input)
        {
            if (!ControlPoints.Contains(input))
                ControlPoints.Add(input);
            ControlPoints.Sort(delegate(double lhs, double rhs) { return lhs.CompareTo(rhs); });
        }

        /// <summary>
        ///     Clears the control points.
        /// </summary>
        public void Clear()
        {
            ControlPoints.Clear();
        }

        /// <summary>
        ///     Auto-generates a terrace curve.
        /// </summary>
        /// <param name="steps">The number of steps.</param>
        public void Generate(int steps)
        {
            if (steps < 2)
                throw new ArgumentException("Need at least two steps");
            Clear();
            double ts = 2.0 / (steps - 1.0);
            double cv = -1.0;
            for (int i = 0; i < steps; i++)
            {
                Add(cv);
                cv += ts;
            }
        }

        #endregion
    }
}