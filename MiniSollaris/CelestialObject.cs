using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MiniSollaris
{
    class CelestialObject
    {
        public static double G { get; set; } = 6.6743015E-11; //Gravitational constant
        public string Name { get; set; }
        public double Mass { get; set; } //kg
        public double StdGravPar { get; set; } // Standard Gravitational Parameter = M * G
        public bool IsCalculatable { get; set; }
        public long Radius { get; set; } //m
        public long[] Position { get; set; } = { 0, 0 }; //m, m in X and Y directions
        public double[] Velocity { get; set; } = { 0, 0 };  //m/s, m/s in X and Y directions

        public Ellipse Picture { get; set; }

        protected double[] acceleration = { 0, 0 };
        public int Number { get; set; }
        public CelestialObject[] CalculatableObjects { get; set; }

        public CelestialObject()
        {
            StdGravPar = G * Mass;
        }

        public CelestialObject(string name, double mass, bool isCalculatable, long radius, long[] position, double[] velocity, int imageSize = 2, Brush color = null)
        {
            Name = name;
            Mass = mass;
            IsCalculatable = isCalculatable;
            Radius = radius;
            Position = position;
            Velocity = velocity;
            StdGravPar = G * Mass;
            GenerateShape(imageSize, color);
        }

        public CelestialObject(string name, double mass, bool isCalculatable, long radius, long distance, double angle, double andleDeviation, double speed, int imageSize = 2, Brush color = null)
        {
            Name = name;
            Mass = mass;
            IsCalculatable = isCalculatable;
            Radius = radius;
            Position = MathHelper.PositionPolarToCartesian(distance,angle);
            Velocity = MathHelper.VelocityPolarToCartesian(speed, angle, andleDeviation);
            StdGravPar = G * Mass;
            GenerateShape(imageSize, color);
        }

        private void GenerateShape(int imageSize, Brush color)
        {
            Picture = new Ellipse();
            Picture.Fill = color != null ? color : Brushes.White;
            Picture.Width = imageSize;
            Picture.Height = imageSize;
        }

        public void CalculateNewPosition(CelestialObject[] objects, double timeStep)
        {
            CalculateAcceleration(objects);
            CalculateNewVelocity(timeStep);
            CalculateNewPosition(timeStep);
        }

        public void CalculateNewVelocity(CelestialObject[] objects, double timeStep)
        {
            CalculateAcceleration(objects);
            CalculateNewVelocity(timeStep);
        }

        public void CalculateNewPosition(double timeStep)
        {
            Position[0] += (long)Math.Round(Velocity[0] * timeStep);
            Position[1] += (long)Math.Round(Velocity[1] * timeStep);
        }

        private void CalculateNewVelocity(double timeStep)
        {
            Velocity[0] += acceleration[0] * timeStep;
            Velocity[1] += acceleration[1] * timeStep;
        }

        protected virtual void CalculateAcceleration(CelestialObject[] objects)
        {
            acceleration = new double[] { 0, 0 };
            double dX, dY, r2, a;                                       //dX, dY - relative position vector components, r2 s- quare of distance, a - temporary value 

            foreach (CelestialObject obj in objects)
            {
                //must test if checking for each object won't be slower than just calculating acceleration from itself
                //probably with less objects it will be faster to do the check, with more - to just calculate without checking
                if (obj.IsCalculatable && obj != this)
                {
                    dX = obj.Position[0] - this.Position[0];
                    dY = obj.Position[1] - this.Position[1];
                    r2 = dX * dX + dY * dY;
                    a = obj.StdGravPar / (Math.Sqrt(r2) * r2);
                    acceleration[0] += a * dX;
                    acceleration[1] += a * dY;
                }
            }
        }
    }
}
