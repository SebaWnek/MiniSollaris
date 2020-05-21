using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MiniSollaris
{
    static class DataAccess
    {
        static double centralMass = 1E39;
        static public SolarSystem InitializeFromJSON(string path, double timeStep)
        {
            return new SolarSystem(path, timeStep);
        }
        static public SolarSystem InitializeHardcodedSystem(double timeStep, int randomCount)
        {
            List<CelestialObject> tmpObjects = new List<CelestialObject>();
            //                                  Name        Mass        Calc? Radius                 Position X         Position Y                         Vel X    Vel Y       Size Color
            //tmpObjects.Add(new CelestialObject("Sun", 1.9884E30, true, 695700000, new long[] { 0, 0 }, new double[] { 0, 0 }, 20, Brushes.Yellow));
            //tmpObjects.Add(new CelestialObject("Mercury",   3.3011E23,  true, 4880000,  new long[] { 57909050000,       0 },                new double[] { 0,       47362 },    4,   Brushes.Brown));
            //tmpObjects.Add(new CelestialObject("Venus",     4.867E24,   true, 6051800,  new long[] { (long)-1.0821E11,  0 },                new double[] { 0,       -35020 },   6,   Brushes.Pink));
            //tmpObjects.Add(new CelestialObject("Earth",     5.97237E24, true, 6371000,  new long[] { 0,                 149598023000 },     new double[] { -29780,  0 },        7,   Brushes.Blue));
            //tmpObjects.Add(new CelestialObject("Mars",      6.4171E23,  true, 3389500,  new long[] { 0,                 (long)-2.2792E11 }, new double[] { 24070,   0 },        5,   Brushes.Red));

            //tmpObjects.Add(new CelestialObject("Moon",      7.342E22,   true, 1737400,  new long[] { 384399999,         149598023000 },     new double[] { -29780,  -1022 },    3,   Brushes.DarkGray));

            //tmpObjects.Add(new CelestialObject("Phobos",    1.072E16,   true, 21000,    new long[] { 9375000,           (long)-2.2792E11 }, new double[] { 24070,   -2138 },    3,   Brushes.Gray));
            //tmpObjects.Add(new CelestialObject("Deimos",    1.476E15,   true, 12000,    new long[] { -23458000,         (long)-2.2792E11 }, new double[] { 24070,   1351 },     3,   Brushes.Orange));

            //tmpObjects.Add(new CelestialObject("Comet", 1, false, 1, 300000000000, 4, 1.2, 15000, 2, Brushes.Aquamarine));

            //tmpObjects.Add(new CelestialObject("Second Sun",  5E37, true, 400000000,  new long[] { 0,                 149598023000 },     new double[] { -29780,  0 },        30,   Brushes.Orange));

            tmpObjects.Add(new CelestialObject2("Sun", centralMass, true, 695700000, new long[] { 0, 0 }, new double[] { 0, 0 }, 20, Brushes.Yellow));
            tmpObjects.Add(new CelestialObject2("Sun2", 1E38, true, (long)1E7, (long)2E11, 2, 0, MathHelper.OrbitalVelocityFromMass(1E37, centralMass, (long)2E11) / 1.4, 15, Brushes.Orange));
            tmpObjects.Add(new CelestialObject2("Sun2", 1E38, true, (long)1E7, (long)1E11, 3.9, 0, MathHelper.OrbitalVelocityFromMass(1E37, centralMass, (long)2E11), 15, Brushes.Red));
            tmpObjects.Add(new CelestialObject2("Sun2", 1E38, true, (long)1E7, (long)1.5E11, 0.5, 0, MathHelper.OrbitalVelocityFromMass(1E37, centralMass, (long)2E11) / 1.3, 15, Brushes.Blue));
            tmpObjects.Add(new CelestialObject2("Sun2", 1E38, true, (long)1E7, (long)3E11, 5, 0, MathHelper.OrbitalVelocityFromMass(1E37, centralMass, (long)2E11) / 1.6, 15, Brushes.PaleTurquoise));
            tmpObjects.Add(new CelestialObject2("Sun2", 1E38, true, (long)1E7, (long)0.5E11, 5.9, 0, MathHelper.OrbitalVelocityFromMass(1E37, centralMass, (long)2E11) / 1.1, 15, Brushes.PaleGreen));
            tmpObjects.Add(new CelestialObject2("Sun2", 1E38, true, (long)1E7, (long)0.75E11, 1.5, 0, MathHelper.OrbitalVelocityFromMass(1E37, centralMass, (long)2E11), 15, Brushes.Violet));
            tmpObjects.Add(new CelestialObject2("Sun2", 1E38, true, (long)1E7, (long)1.1E11, 3.5, 0, MathHelper.OrbitalVelocityFromMass(1E37, centralMass, (long)2E11) / 1.1, 15, Brushes.BlueViolet));
            tmpObjects.Add(new CelestialObject2("Sun2", 1E38, true, (long)1E7, (long)1.7E11, 4.3, 0, MathHelper.OrbitalVelocityFromMass(1E37, centralMass, (long)2E11) / 1.1, 15, Brushes.DarkOliveGreen));

            GenerateRandomObjectsPolar(tmpObjects, randomCount);

            return new SolarSystem(tmpObjects, timeStep);
        }

        private static void GenerateRandomObjectsPolar(List<CelestialObject> tmpObjects, int count)
        {
            Random rnd = new Random();
            double mass = 1E32;
            int DistanceMultiplier = 800;
            long[] position;
            double[] velocity;
            int minDistance = 3200000;
            int maxDistance = 470000000;
            long distance;
            double angle;
            double angleVariance = 0.05;
            double speed;
            double speedMultiplier = 1;
            double speedVariance = 1000;
            for (int i = 0; i < count; i++)
            {
                distance = (long)rnd.Next(minDistance, maxDistance) * DistanceMultiplier;
                distance = (long)(distance * Math.Pow(rnd.Next(5,10),0.0001));
                angle = rnd.NextDouble() * 2 * Math.PI;
                speed = speedMultiplier * (MathHelper.OrbitalVelocityFromMass(centralMass, mass, distance) + rnd.NextDouble() * speedVariance * (rnd.Next(0, 2) > 0 ? 1 : -1));
                position = MathHelper.PositionPolarToCartesian(distance, angle);
                velocity = MathHelper.VelocityPolarToCartesian(speed, angle, rnd.NextDouble() * angleVariance);
                tmpObjects.Add(new CelestialObject2("test", mass, true, 2000000000, position, velocity));
            }
        }
    }
}
