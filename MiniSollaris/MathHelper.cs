using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace MiniSollaris
{
    public static class MathHelper
    {
        static double G { get; set; } = 6.6743015E-11;

        public static long[] PositionPolarToCartesian(long distance, double angle)
        {
            long[] result = new long[2];
            result[0] = (long)(distance * Math.Cos(angle));
            result[1] = (long)(distance * Math.Sin(angle));
            return result;
        }

        public static double[] VelocityPolarToCartesian(double velocity, double angle, double angleDeviation)
        {
            double[] result = new double[2];

            result[0] = velocity * Math.Cos(angle + Math.PI / 2 + angleDeviation);
            result[1] = velocity * Math.Sin(angle + Math.PI / 2 + angleDeviation);

            return result;
        }

        public static double OrbitalVelocityFromMass(double mass, long distance)
        {
            return Math.Sqrt(G * mass / distance);
        }

        public static double OrbitalVelocityFromMass(double mass1, double mass2, long distance)
        {
            return Math.Sqrt(G * mass1 / distance) + Math.Sqrt(G * mass2 / distance);
        }
    }
}
