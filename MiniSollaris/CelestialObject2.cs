using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MiniSollaris
{
    class CelestialObject2 : CelestialObject
    {
        public CelestialObject2(string name, double mass, bool isCalculatable, long radius, long[] position, double[] velocity, int imageSize = 2, Brush color = null)
            : base(name, mass, isCalculatable, radius, position, velocity, imageSize, color)
        {

        }
        public CelestialObject2(string name, double mass, bool isCalculatable, long radius, long distance, double angle, double andleDeviation, double speed, int imageSize = 2, Brush color = null)
            : base(name, mass, isCalculatable, radius, distance, angle, andleDeviation, speed, imageSize, color)
        { }

        /// <summary>
        /// Calculates new acceleration.
        /// Method taking into account sizes of objects.  
        /// </summary>
        /// <remarks>
        /// For now it allows avoiding calculation problems when objects are too close to each other, in future might be used for collision detection.  
        /// </remarks>
        protected override void CalculateAcceleration()
        {
            acceleration = new double[] { 0, 0 };
            double dX, dY, r2, a;                                       //dX, dY - relative position vector components, r2 s- quare of distance, a - temporary value 

            foreach (CelestialObject obj in CalculatableObjects)
            {
                dX = obj.Position[0] - this.Position[0];
                dY = obj.Position[1] - this.Position[1];
                r2 = dX * dX + dY * dY;
                if (r2 <= ((double)Radius + (double)obj.Radius) * ((double)Radius + (double)obj.Radius))
                {
                    return;
                    //obj.Position = Position = new long[] { long.MaxValue, long.MaxValue };
                    //obj.IsCalculatable = IsCalculatable = false;
                }
                a = obj.StdGravPar / (Math.Sqrt(r2) * r2);
                acceleration[0] += a * dX;
                acceleration[1] += a * dY;
            }
        }
    }
}
