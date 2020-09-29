using System.Diagnostics;

namespace LibNoise.Operator
{
    /// <summary>
    ///     Provides a noise module that moves the coordinates of the input value before
    ///     returning the output value from a source module. [OPERATOR]
    /// </summary>
    public class Translate : ModuleBase
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
            return Modules[0].GetValue(x + X, y + Y, z + Z);
        }

        #endregion

        #region Fields

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of Translate.
        /// </summary>
        public Translate()
            : base(1)
        {
        }

        /// <summary>
        ///     Initializes a new instance of Translate.
        /// </summary>
        /// <param name="input">The input module.</param>
        public Translate(ModuleBase input)
            : base(1)
        {
            Modules[0] = input;
        }

        /// <summary>
        ///     Initializes a new instance of Translate.
        /// </summary>
        /// <param name="x">The translation on the x-axis.</param>
        /// <param name="y">The translation on the y-axis.</param>
        /// <param name="z">The translation on the z-axis.</param>
        /// <param name="input">The input module.</param>
        public Translate(double x, double y, double z, ModuleBase input)
            : base(1)
        {
            Modules[0] = input;
            X = x;
            Y = y;
            Z = z;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the translation on the x-axis.
        /// </summary>
        public double X { get; set; } = 1.0;

        /// <summary>
        ///     Gets or sets the translation on the y-axis.
        /// </summary>
        public double Y { get; set; } = 1.0;

        /// <summary>
        ///     Gets or sets the translation on the z-axis.
        /// </summary>
        public double Z { get; set; } = 1.0;

        #endregion
    }
}