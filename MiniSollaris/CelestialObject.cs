using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSollaris
{
    class CelestialObject
    {
        public double G { get; set; } = 6.6743015E-11; //Gravitational constant
        public string Name { get; set; }
        public double Mass { get; set; } //kg
        public double SGravPar { get; set; } // Standard Gravitational Parameter = M * G
        public bool IsCalculatable { get; set; }
        public int Diameter { get; set; } //m
        public long[] Position { get; set; } = { 0, 0 }; //m, m in X and Y directions
        public int[] Velocity { get; set; } = { 0, 0 };  //m/s, m/s in X and Y directions

        public CelestialObject()
        {
            SGravPar = G * Mass;
        }

        public CelestialObject(string name, double mass, bool isCalculatable, int diameter, long[] position, int[] velocity) : this()
        {
            Name = name;
            Mass = mass;
            IsCalculatable = isCalculatable;
            Diameter = diameter;
            Position = position;
            Velocity = velocity;
        }

        public void CalculateStep(double acceleration, int timeStep)
        {
            throw new NotImplementedException();
        }
    }
}
