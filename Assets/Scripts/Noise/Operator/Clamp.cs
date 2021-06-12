using System.Diagnostics;

namespace LibNoise.Operator
{
    /// <summary>
    ///     Provides a noise module that clamps the output value from a source module to a
    ///     range of values. [OPERATOR]
    /// </summary>
    public class Clamp : ModuleBase
    {
        #region Methods

        /// <summary>
        ///     Sets the bounds.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        public void SetBounds(double min, double max)
        {
            Debug.Assert(min < max);
            Minimum = min;
            Maximum = max;
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
            Debug.Assert(Modules[0] != null);
            if (Minimum > Maximum)
            {
                double t = Minimum;
                Minimum = Maximum;
                Maximum = t;
            }

            double v = Modules[0].GetValue(x, y, z);
            if (v < Minimum)
                return Minimum;
            if (v > Maximum)
                return Maximum;
            return v;
        }

        #endregion

        #region Fields

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of Clamp.
        /// </summary>
        public Clamp()
            : base(1)
        {
        }

        /// <summary>
        ///     Initializes a new instance of Clamp.
        /// </summary>
        /// <param name="input">The input module.</param>
        public Clamp(ModuleBase input)
            : base(1)
        {
            Modules[0] = input;
        }

        /// <summary>
        ///     Initializes a new instance of Clamp.
        /// </summary>
        /// <param name="input">The input module.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        public Clamp(double min, double max, ModuleBase input)
            : base(1)
        {
            Minimum = min;
            Maximum = max;
            Modules[0] = input;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the maximum to clamp to.
        /// </summary>
        public double Maximum { get; set; } = 1.0;

        /// <summary>
        ///     Gets or sets the minimum to clamp to.
        /// </summary>
        public double Minimum { get; set; } = -1.0;

        #endregion
    }
}