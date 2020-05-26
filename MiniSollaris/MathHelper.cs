using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace MiniSollaris
{
    /// <summary>
    /// Helper class for calculations
    /// </summary>
    public static class MathHelper
    {
        static double G { get; set; } = 6.6743015E-11;

        /// <summary>
        /// Changes polar position coordinates to cartesian coordinates
        /// </summary>
        /// <param name="distance">Polar distance</param>
        /// <param name="angle">Polar angle</param>
        /// <returns>Cartesian coordinates</returns>
        public static long[] PositionPolarToCartesian(long distance, double angle)
        {
            long[] result = new long[2];
            result[0] = (long)(distance * Math.Cos(angle));
            result[1] = (long)(distance * Math.Sin(angle));
            return result;
        }
        /// <summary>
        /// Calculates velocity vector assuming 90 deg + deviation angle relative to radius direction of movement.
        /// </summary>
        /// <param name="velocity">Scalar value of velocity</param>
        /// <param name="angle">Polar angle of object</param>
        /// <param name="angleDeviation">Angle between direction prepenicular to radius and direction of movement</param>
        /// <returns></returns>
        public static double[] VelocityPolarToCartesian(double velocity, double angle, double angleDeviation)
        {
            double[] result = new double[2];

            result[0] = velocity * Math.Cos(angle + Math.PI / 2 + angleDeviation);
            result[1] = velocity * Math.Sin(angle + Math.PI / 2 + angleDeviation);

            return result;
        }
        /// <summary>
        /// Calculates orbital velocity when orbiting much hevier body.
        /// </summary>
        /// <param name="mass">Orbited body mass</param>
        /// <param name="distance">Distance</param>
        /// <returns></returns>
        public static double OrbitalVelocityFromMass(double mass, long distance)
        {
            return Math.Sqrt(G * mass / distance);
        }
        /// <summary>
        /// Calculates approximate orbital velocity when orbiting body of comparable mass.
        /// </summary>
        /// <param name="mass1">Orbitning body mass</param>
        /// <param name="mass2">Orbited body mass</param>
        /// <param name="distance">Distance</param>
        /// <returns></returns>
        public static double OrbitalVelocityFromMass(double mass1, double mass2, long distance)
        {
            return Math.Sqrt(G * mass1 / distance) + Math.Sqrt(G * mass2 / distance);
        }
    }
}
