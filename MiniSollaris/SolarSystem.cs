using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSollaris
{
    class SolarSystem
    {
        public int TimeStep { get; set; }
        private CelestialObject[] objects;
        private double[,] distances;
        private double[,] accelerationsMatrix;
        private double[] accelerations;
        private int objcectsCount;

        public SolarSystem(CelestialObject[] obj, int timeStep)
        {
            TimeStep = timeStep;
            objects = obj;
            objcectsCount = objects.Length;
            distances = new double[objcectsCount, objcectsCount];
            accelerationsMatrix = new double[objcectsCount, objcectsCount];
            accelerations = new double[objcectsCount];
        }

        public void CalculateStep()
        {
            ClearMatrices();
            CalculateDistanceMatrix();
            CalculateAccelerationMatric();
            CalculateAccelerations();
            for(int i = 0; i < objcectsCount; i++)
            {
                objects[i].CalculateStep(accelerations[i], TimeStep);
            }
        }

        private double CalculateDistance(CelestialObject obj1, CelestialObject obj2)
        {
            return Math.Sqrt((obj1.Position[0] - obj2.Position[0]) * (obj1.Position[0] - obj2.Position[0]) + (obj1.Position[1] - obj2.Position[1]) * (obj1.Position[1] - obj2.Position[1]));
        }

        private void ClearMatrices()
        {
            distances = new double[objcectsCount, objcectsCount];
            accelerationsMatrix = new double[objcectsCount, objcectsCount];
            accelerations = new double[objcectsCount];
        }

        private void CalculateDistanceMatrix()
        {
            for (int i = 0; i < objcectsCount; i++)
            {
                for (int j = 0; j < objcectsCount; j++)
                {
                    distances[i, j] = CalculateDistance(objects[i], objects[j]); //for i = j it will be just 0, don't want to do checking as it would need to check n^2 times to eliminate n cases
                }
            }
        }

        private void CalculateAccelerationMatric()
        {
            for (int i = 0; i < objcectsCount; i++)
            {
                for (int j = 0; j < objcectsCount; j++)
                {
                    accelerationsMatrix[i, j] = objects[j].SGravPar / distances[i, j]; //for i = j it will be just 0, don't want to do checking as it would need to check n^2 times to eliminate n cases
                }
            }
        }

        private void CalculateAccelerations()
        {
            for (int i = 0; i < objcectsCount; i++)
            {
                for (int j = 0; j < objcectsCount; j++)
                {
                    accelerations[i] += accelerationsMatrix[i,j];
                }
            }
        }
    }
}
