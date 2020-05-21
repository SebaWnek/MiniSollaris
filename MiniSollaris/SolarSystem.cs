using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MiniSollaris
{
    class SolarSystem
    {
        public double TimeStep { get; set; }
        public static double G { get; set; } = 6.6743015E-11; //Gravitational constant
        internal CelestialObject[] Objects { get => objects; set => objects = value; }

        private CelestialObject[] objects;
        private double[,] gMms; //precalculated combinations of G and masses of objects
        private double[,,] forces; //forces betweeen each planet
        private double[,] forcesSum;

        public SolarSystem(CelestialObject[] obj, double timeStep)
        {
            TimeStep = timeStep;
            Objects = obj;
            CalculateContinuousStepsGMms();
            forces = new double[objects.Length, objects.Length, 2];
            forcesSum = new double[objects.Length, 2];
        }

        public SolarSystem(List<CelestialObject> obj, double timeStep)
        {
            TimeStep = timeStep;
            Objects = obj.ToArray();
            CalculateContinuousStepsGMms();
            forces = new double[objects.Length, objects.Length, 2];
            forcesSum = new double[objects.Length, 2];
        }

        public SolarSystem(string path, double timeStep)
        {
            TimeStep = timeStep;
            Objects = Deserialize(path);
            CalculateContinuousStepsGMms();
            forces = new double[objects.Length, objects.Length, 2];
            forcesSum = new double[objects.Length, 2];
        }

        #region Threaded slim

        private void CalculateContinuousStepsGMms()
        {
            int count = objects.Length;
            gMms = new double[count, count];
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    gMms[i, j] = G * objects[i].Mass * objects[j].Mass;
                }
            }
        }

        private List<CelestialObject>[] DivideObjectsPerThreadsSlim(int count)
        {
            int objCount = objects.Length;
            List<CelestialObject>[] objPerThread = new List<CelestialObject>[count];
            for (int i = 0; i < count; i++) objPerThread[i] = new List<CelestialObject>();
            int thread = 0;
            int step = 1;
            for (int i = 0; i < objCount; i++)
            {
                objPerThread[thread].Add(objects[i]);
                if (thread + step >= 0 && thread + step < count) thread += step;
                else step = -step;
            }
            return objPerThread;
        }

        private void AssignCalculatableObjects()
        {
            for (int i = 0; i < objects.Length; i++)
            {
                objects[i].Number = i;
                objects[i].CalculatableObjects = objects.Skip(i + 1).Take(objects.Length - (i + 1)).ToArray();
            }
        }

        private void CalculateForces(CelestialObject obj)
        {
            int num = obj.Number;
            double dX, dY, r2, F;
            for (int i = num + 1; i < objects.Length; i++)
            {
                dX = objects[i].Position[0] - obj.Position[0];
                dY = objects[i].Position[1] - obj.Position[1];
                r2 = dX * dX + dY * dY;
                F = gMms[obj.Number, i] / (Math.Sqrt(r2) * r2);
                forces[num, i, 0] = F * dX;
                forces[num, i, 1] = F * dY;
                forces[i, num, 0] = -forces[num, i, 0];
                forces[i, num, 1] = -forces[num, i, 1];
            }
        }

        private void UpdatePositions(CelestialObject obj)
        {
            int num = obj.Number;
            double[] force = new double[2];
            for (int i = 0; i < objects.Length; i++)
            {
                force[0] += forces[num, i, 0];
                force[1] += forces[num, i, 1];
            }
            obj.Position[0] += (long)(TimeStep * (obj.Velocity[0] + TimeStep * force[0] / obj.Mass));
            obj.Position[1] += (long)(TimeStep * (obj.Velocity[0] + TimeStep * force[1] / obj.Mass));
        }

        private void CalculateContinuousStepsSlim(IEnumerable<CelestialObject> obj, Barrier barrier, CancellationToken token)
        {
            int objCount = obj.Count();
            CelestialObject[] threadObjects = obj.ToArray();
            while (true)
            {
                if (token.IsCancellationRequested) break;
                for (int i = 0; i < objCount; i++)
                {
                    CalculateForces(threadObjects[i]);
                }
                barrier.SignalAndWait();
                for (int i = 0; i < objCount; i++)
                {
                    UpdatePositions(threadObjects[i]);
                }
                barrier.SignalAndWait();
            }
        }

        public void StartThreadsSlim(CancellationToken token)
        {
            int procCount = Environment.ProcessorCount;
            int objCount = objects.Length;
            int count = procCount <= objCount ? procCount : objCount;
            Barrier barrier = new Barrier(count);
            List<CelestialObject>[] objPerThread = DivideObjectsPerThreads(count);
            AssignCalculatableObjects();
            Thread[] threads = GenerateThreads(objPerThread, barrier, token);
            foreach (Thread thread in threads) thread.Start();
        }

        public Thread[] StartThreadsSlim(int steps)
        {
            int procCount = Environment.ProcessorCount;
            int objCount = objects.Length;
            int count = procCount <= objCount ? procCount : objCount;
            Barrier barrier = new Barrier(count);
            List<CelestialObject>[] objPerThread = DivideObjectsPerThreads(count);
            AssignCalculatableObjects();
            Thread[] threads = GenerateThreads(objPerThread, barrier, steps);
            foreach (Thread thread in threads) thread.Start();
            return threads;
        }
        #endregion

        #region Serial

        public void CalculateStep()
        {
            foreach (CelestialObject obj in objects)
            {
                obj.CalculateNewPosition(objects, TimeStep);
            }
        }

        #endregion

        #region Parallel

        public void CalculateStepParallel()
        {
            Parallel.ForEach(Objects, (obj) => { (obj as CelestialObject).CalculateNewPosition(Objects, TimeStep); });
        }

        #endregion

        #region Threaded

        public void StartThreadsPerCore(CancellationToken token)
        {
            int procCount = Environment.ProcessorCount;
            int objCount = objects.Length;
            int count = procCount <= objCount ? procCount : objCount;
            Barrier barrier = new Barrier(count);
            List<CelestialObject>[] objPerThread = DivideObjectsPerThreads(count);
            Thread[] threads = GenerateThreads(objPerThread, barrier, token);
            foreach (Thread thread in threads) thread.Start();
        }
        public Thread[] StartThreadsPerCoreCounted(int steps)
        {
            int procCount = Environment.ProcessorCount;
            int objCount = objects.Length;
            int count = procCount <= objCount ? procCount : objCount;
            Barrier barrier = new Barrier(count);
            List<CelestialObject>[] objPerThread = DivideObjectsPerThreads(count);
            Thread[] threads = GenerateThreads(objPerThread, barrier, steps);
            foreach (Thread thread in threads) thread.Start();
            return threads;
        }
        public void StartThreadsPerCoreStepped(CancellationToken token, int stepsPerCycle, object locker)
        {
            int procCount = Environment.ProcessorCount;
            int objCount = objects.Length;
            int count = procCount <= objCount ? procCount : objCount;
            Barrier barrier = new Barrier(count);
            List<CelestialObject>[] objPerThread = DivideObjectsPerThreads(count);
            Thread[] threads = GenerateThreadsStepped(objPerThread, barrier, token, stepsPerCycle, locker);
            foreach (Thread thread in threads) thread.Start();
        }
        private Thread[] GenerateThreadsStepped(List<CelestialObject>[] objPerThread, Barrier barrier, CancellationToken token, int stepsPerCycle, object locker)
        {
            Thread[] threads = new Thread[objPerThread.Length];
            for (int i = 0; i < objPerThread.Length; i++)
            {
                int current = i;
                threads[i] = new Thread(() => { CalculateContinuousStepsStepped(objPerThread[current], barrier, token, stepsPerCycle, locker); });
                threads[i].Name = "Thread " + i;
            }
            return threads;
        }

        private void CalculateContinuousStepsStepped(IEnumerable<CelestialObject> obj, Barrier barrier, CancellationToken token, int stepsPerCycle, object locker)
        {
            int objCount = obj.Count();
            CelestialObject[] threadObjects = obj.ToArray();
            while (true)
            {
                if (token.IsCancellationRequested) break;
                for (int j = 0; j < stepsPerCycle; j++)
                {
                    for (int i = 0; i < objCount; i++)
                    {
                        threadObjects[i].CalculateNewVelocity(objects, TimeStep);
                    }
                    barrier.SignalAndWait();
                    for (int i = 0; i < objCount; i++)
                    {
                        threadObjects[i].CalculateNewPosition(TimeStep);
                    }
                    barrier.SignalAndWait(); 
                }
                lock (locker) Monitor.Wait(locker);
            }
        }

        #endregion

        #region Threaded per object

        public Thread[] StartThreadsPerThreadCounted(int steps)
        {
            int count = objects.Length;
            Barrier barrier = new Barrier(count);
            Thread[] threads = GenerateThreadsPerObjectCounted(barrier, steps);
            foreach (Thread thread in threads) thread.Start();
            return threads;
        }
        private Thread[] GenerateThreadsPerObjectCounted(Barrier barrier, int steps)
        {
            int count = objects.Count();
            Thread[] threads = new Thread[count];
            for (int i = 0; i < count; i++)
            {
                int current = i;
                CelestialObject currentObject = objects[i];
                threads[i] = new Thread(() => { CalculateContinuousStepsCounted(currentObject, barrier, steps); });
                threads[i].Name = "Thread " + i;
            }
            return threads;
        }

        private void CalculateContinuousStepsCounted(CelestialObject currentObject, Barrier barrier, int steps)
        {
            for (int j = 0; j < steps; j++)
            {
                currentObject.CalculateNewPosition(objects, TimeStep);
                barrier.SignalAndWait();
            }
        }

        #endregion


        #region Common
        private Thread[] GenerateThreads(List<CelestialObject>[] objPerThread, Barrier barrier, CancellationToken token)
        {
            Thread[] threads = new Thread[objPerThread.Length];
            for (int i = 0; i < objPerThread.Length; i++)
            {
                int current = i;
                threads[i] = new Thread(() => { CalculateContinuousSteps(objPerThread[current], barrier, token); });
                threads[i].Name = "Thread " + i;
            }
            return threads;
        }

        private Thread[] GenerateThreads(List<CelestialObject>[] objPerThread, Barrier barrier, int steps)
        {
            Thread[] threads = new Thread[objPerThread.Length];
            for (int i = 0; i < objPerThread.Length; i++)
            {
                int current = i;
                threads[i] = new Thread(() => { CalculateContinuousSteps(objPerThread[current], barrier, steps); });
                threads[i].Name = "Thread " + i;
            }
            return threads;
        }

        private List<CelestialObject>[] DivideObjectsPerThreads(int count)
        {
            int objCount = objects.Length;
            List<CelestialObject>[] objPerThread = new List<CelestialObject>[count];
            for (int i = 0; i < count; i++) objPerThread[i] = new List<CelestialObject>();
            for (int i = 0; i < objCount; i++)
            {
                int threadNumber = i % count;
                objPerThread[threadNumber].Add(objects[i]);
            }
            return objPerThread;
        }
        private void CalculateContinuousSteps(IEnumerable<CelestialObject> obj, Barrier barrier, CancellationToken token)
        {
            int objCount = obj.Count();
            CelestialObject[] threadObjects = obj.ToArray();
            while (true)
            {
                if (token.IsCancellationRequested) break;
                for (int i = 0; i < objCount; i++)
                {
                    threadObjects[i].CalculateNewVelocity(objects, TimeStep);
                }
                barrier.SignalAndWait();
                for (int i = 0; i < objCount; i++)
                {
                    threadObjects[i].CalculateNewPosition(TimeStep);
                }
                barrier.SignalAndWait();
            }
        }

        private void CalculateContinuousSteps(IEnumerable<CelestialObject> obj, Barrier barrier, int steps)
        {
            int objCount = obj.Count();
            CelestialObject[] threadObjects = obj.ToArray();
            for (int j = 0; j < steps; j++)
            {
                for (int i = 0; i < objCount; i++)
                {
                    threadObjects[i].CalculateNewVelocity(objects, TimeStep);
                }
                barrier.SignalAndWait();
                for (int i = 0; i < objCount; i++)
                {
                    threadObjects[i].CalculateNewPosition(TimeStep);
                }
                barrier.SignalAndWait();
            }
        }
        #endregion


        #region Helper methods
        public void Serialize(string path)
        {
            string jsonString = JsonSerializer.Serialize(Objects, new JsonSerializerOptions() { WriteIndented = true });
            File.WriteAllText(path, jsonString);
        }

        public static CelestialObject[] Deserialize(string path)
        {
            CelestialObject[] result;
            string jsonString = File.ReadAllText(path);
            result = JsonSerializer.Deserialize<CelestialObject[]>(jsonString);
            return result;
        }

        public CelestialObject SelectObject(string name)
        {
            CelestialObject result = objects.First(obj => obj.Name == name); //not First or default so we get exception if not found
            return result;
        }
        #endregion
    }
}
