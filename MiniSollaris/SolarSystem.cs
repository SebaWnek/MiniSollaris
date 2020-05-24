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
        public int ObjectsCount { get => objects.Length; }

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
            AssignCalculatableObjects();
        }

        public SolarSystem(List<CelestialObject> obj, double timeStep)
        {
            TimeStep = timeStep;
            Objects = obj.ToArray();
            CalculateContinuousStepsGMms();
            forces = new double[objects.Length, objects.Length, 2];
            forcesSum = new double[objects.Length, 2];
            AssignCalculatableObjects();
        }

        public SolarSystem(string path, double timeStep)
        {
            TimeStep = timeStep;
            Objects = Deserialize(path);
            CalculateContinuousStepsGMms();
            forces = new double[objects.Length, objects.Length, 2];
            forcesSum = new double[objects.Length, 2];
            AssignCalculatableObjects();
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

        private void AssignCalculatableObjectsSlim()
        {
            for (int i = 0; i < objects.Length; i++)
            {
                objects[i].Number = i;
                objects[i].CalculatableObjects = objects.Skip(i + 1).Take(objects.Length - (i + 1)).Where(k => k.IsCalculatable == true).ToArray();
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
                //barrier.SignalAndWait();
            }
        }

        private void CalculateContinuousStepsSlim(IEnumerable<CelestialObject> obj, Barrier barrier, int steps)
        {
            int objCount = obj.Count();
            CelestialObject[] threadObjects = obj.ToArray();
            for(int j = 0; j < steps; j++)
            {
                for (int i = 0; i < objCount; i++)
                {
                    CalculateForces(threadObjects[i]);
                }
                barrier.SignalAndWait();
                for (int i = 0; i < objCount; i++)
                {
                    UpdatePositions(threadObjects[i]);
                }
                //barrier.SignalAndWait();
            }
        }

        public void StartThreadsSlim(CancellationToken token)
        {
            int procCount = Environment.ProcessorCount;
            int objCount = objects.Length;
            int count = procCount <= objCount ? procCount : objCount;
            Barrier barrier = new Barrier(count);
            List<CelestialObject>[] objPerThread = DivideObjectsPerThreadsSlim(count);
            AssignCalculatableObjectsSlim();
            Thread[] threads = GenerateThreadsSlim(objPerThread, token);
            foreach (Thread thread in threads) thread.Start();
        }
        private Thread[] GenerateThreadsSlim(List<CelestialObject>[] objPerThread, CancellationToken token)
        {
            Barrier barrier = new Barrier(objPerThread.Length);
            Thread[] threads = new Thread[objPerThread.Length];
            for (int i = 0; i < objPerThread.Length; i++)
            {
                int current = i;
                threads[i] = new Thread(() => { CalculateContinuousStepsSlim(objPerThread[current], barrier, token); });
                threads[i].Name = "Thread " + i;
            }
            return threads;
        }

        public Thread[] StartThreadsSlim(int steps)
        {
            int procCount = Environment.ProcessorCount;
            int objCount = objects.Length;
            int count = procCount <= objCount ? procCount : objCount;
            Barrier barrier = new Barrier(count);
            List<CelestialObject>[] objPerThread = DivideObjectsPerThreadsSlim(count);
            AssignCalculatableObjectsSlim();
            Thread[] threads = GenerateThreadsSlim(objPerThread, steps);
            foreach (Thread thread in threads) thread.Start();
            return threads;
        }
        private Thread[] GenerateThreadsSlim(List<CelestialObject>[] objPerThread, int steps)
        {
            Barrier barrier = new Barrier(objPerThread.Length);
            Thread[] threads = new Thread[objPerThread.Length];
            for (int i = 0; i < objPerThread.Length; i++)
            {
                int current = i;
                threads[i] = new Thread(() => { CalculateContinuousStepsSlim(objPerThread[current], barrier, steps); });
                threads[i].Name = "Thread " + i;
            }
            return threads;
        }
        #endregion

        #region Serial
        /// <summary>
        /// Calculates one step using only one thread.
        /// To be used with another method that will use it in a loop, or with timer. 
        /// </summary>
        public void CalculateStep()
        {
            foreach (CelestialObject obj in objects)
            {
                obj.CalculateNewPosition(TimeStep);
            }
        }

        #endregion

        #region Parallel
        /// <summary>
        /// Calculates one step using TPL.
        /// To be used with another method that will use it in a loop, or with timer. 
        /// </summary>
        public void CalculateStepParallel()
        {
            Parallel.ForEach(Objects, (obj) => { (obj as CelestialObject).CalculateNewPosition(TimeStep); });
        }

        #endregion

        #region Threaded with locks

        /// <summary>
        /// Starts continuous simulation, that will stop only on callation token.
        /// Provides object array to pause simulation during redrawing UI. 
        /// </summary>
        /// <param name="token">Cancellation token to stop simulation</param>
        /// <returns>Array of locker objects that can be used to synchronize with UI thread, to pause simulation during redrawing</returns>
        public object[] StartThreadsPerCoreWithLocker(CancellationToken token)
        {
            int procCount = Environment.ProcessorCount;
            int objCount = objects.Length;
            int count = procCount <= objCount ? procCount : objCount;
            List<CelestialObject>[] objPerThread = DivideObjectsPerThreads(count);
            (Thread[] threads, object[] lockers) = GenerateThreadsWithLocker(objPerThread, token);
            foreach (Thread thread in threads) thread.Start();
            return lockers;
        }

        /// <summary>
        /// Starts continuous simulation, that will stop after required number of steps calculated.
        /// Intended only for benchmark use, to simulate overhead of locking each step.
        /// </summary>
        /// <param name="steps">Number of calculation steps to be performed</param>
        /// <returns>Array of threads that can be joined for continuation after calculation</returns>
        public Thread[] StartThreadsPerCoreCountedWithLocker(int steps)
        {
            int procCount = Environment.ProcessorCount;
            int objCount = objects.Length;
            int count = procCount <= objCount ? procCount : objCount;
            List<CelestialObject>[] objPerThread = DivideObjectsPerThreads(count);
            Thread[] threads = GenerateThreadsWithLocker(objPerThread, steps);
            foreach (Thread thread in threads) thread.Start();
            return threads;
        }
        /// <summary>
        /// Generates threads equal in number to CPU cores count, that will stop only on callation token.
        /// Provides object array to pause simulation during redrawing UI. 
        /// </summary>
        /// <param name="objPerThread">Array of lists containing objects intended for each thread</param>
        /// <param name="token">Cancellation token to stop simulation</param>
        /// <returns>Tuple of arrays - array of threads to be started and array of locker objects that can be used to synchronize with UI thread, to pause simulation during redrawing </returns>
        private (Thread[], object[]) GenerateThreadsWithLocker(List<CelestialObject>[] objPerThread, CancellationToken token)
        {
            Barrier barrier = new Barrier(objPerThread.Length);
            Thread[] threads = new Thread[objPerThread.Length];
            object[] lockers = new object[objPerThread.Length];
            for (int i = 0; i < objPerThread.Length; i++)
            {
                lockers[i] = new object();
            }
            for (int i = 0; i < objPerThread.Length; i++)
            {
                int current = i;
                threads[i] = new Thread(() => { CalculateContinuousStepsWithLocker(objPerThread[current], barrier, token, lockers[current]); });
                threads[i].Name = "Thread " + i;
            }
            return (threads, lockers);
        }

        /// <summary>
        /// Generates threads equal in number to CPU cores count, that will stop after required number of steps calculated.
        /// Intended only for benchmark use, to simulate overhead of locking each step.
        /// </summary>
        /// <param name="objPerThread">Array of lists containing objects intended for each thread</param>
        /// <param name="token">Cancellation token to stop simulation</param>
        /// <returns>Array of threads that can be joined for continuation after calculation</returns>
        private Thread[] GenerateThreadsWithLocker(List<CelestialObject>[] objPerThread, int steps)
        {
            Barrier barrier = new Barrier(objPerThread.Length);
            Thread[] threads = new Thread[objPerThread.Length];
            object[] lockers = new object[objPerThread.Length];
            for (int i = 0; i < objPerThread.Length; i++)
            {
                lockers[i] = new object();
            }
            for (int i = 0; i < objPerThread.Length; i++)
            {
                int current = i;
                threads[i] = new Thread(() => { CalculateContinuousStepsWithLocker(objPerThread[current], barrier, steps, lockers[current]); });
                threads[i].Name = "Thread " + i;
            }
            return threads;
        }

        /// <summary>
        /// Calculaties selected objects predefined number of times.
        /// Intended only for benchmarking, with locks on each step to simulate locking in real-time version.
        /// </summary>
        /// <param name="obj">Objects assigned to this thread.</param>
        /// <param name="barrier">Barrier for synchronizing calculation steps between threads</param>
        /// <param name="steps">Number of calculation steps to be counted</param>
        /// <param name="locker">Locker object</param>
        private void CalculateContinuousStepsWithLocker(IEnumerable<CelestialObject> obj, Barrier barrier, int steps, object locker)
        {
            int objCount = obj.Count();
            CelestialObject[] threadObjects = obj.ToArray();
            for (int j = 0; j < steps; j++)
            {
                for (int i = 0; i < objCount; i++)
                {
                    threadObjects[i].CalculateNewVelocity(TimeStep);
                }
                barrier.SignalAndWait();
                lock (locker)
                {
                    for (int i = 0; i < objCount; i++)
                    {
                        threadObjects[i].UpdatePosition(TimeStep);
                    }
                }
                //barrier.SignalAndWait();
            }
        }

        /// <summary>
        /// Calculaties selected objects until cancelled.
        /// Gets blocked when UI thread locks locker object so there are no changes during UI updating.
        /// Running one per each thread with assigned part of objects.
        /// </summary>
        /// <param name="obj">Objects assigned to this thread.</param>
        /// <param name="barrier">Barrier for synchronizing calculation steps between threads</param>
        /// <param name="token">Cancellation token to stop simulation</param>
        /// <param name="locker"></param>
        private void CalculateContinuousStepsWithLocker(IEnumerable<CelestialObject> obj, Barrier barrier, CancellationToken token, object locker)
        {
            int objCount = obj.Count();
            CelestialObject[] threadObjects = obj.ToArray();
            while (true)
            {
                if (token.IsCancellationRequested) break;
                for (int i = 0; i < objCount; i++)
                {
                    threadObjects[i].CalculateNewVelocity(TimeStep);
                }
                barrier.SignalAndWait();
                lock (locker)
                {
                    for (int i = 0; i < objCount; i++)
                    {
                        threadObjects[i].UpdatePosition(TimeStep);
                    }
                }
                //barrier.SignalAndWait();

            }
        }

        #endregion

        #region Threaded

        /// <summary>
        /// Starts continuous simulation, that will stop only on callation token.
        /// </summary>
        /// <param name="token">Cancellation token to stop simulation</param>
        public void StartThreadsPerCore(CancellationToken token)
        {
            int procCount = Environment.ProcessorCount;
            int objCount = objects.Length;
            int count = procCount <= objCount ? procCount : objCount;
            Barrier barrier = new Barrier(count);
            List<CelestialObject>[] objPerThread = DivideObjectsPerThreads(count);
            Thread[] threads = GenerateThreads(objPerThread, token);
            foreach (Thread thread in threads) thread.Start();
        }
        /// <summary>
        /// Starts continuous simulation, that will stop after required number of steps calculated.
        /// </summary>
        /// <param name="steps">Number of calculation steps to be performed</param>
        /// <returns>Array of threads that can be joined for continuation after calculation</returns>
        public Thread[] StartThreadsPerCoreCounted(int steps)
        {
            int procCount = Environment.ProcessorCount;
            int objCount = objects.Length;
            int count = procCount <= objCount ? procCount : objCount;
            Barrier barrier = new Barrier(count);
            List<CelestialObject>[] objPerThread = DivideObjectsPerThreads(count);
            Thread[] threads = GenerateThreads(objPerThread, steps);
            foreach (Thread thread in threads) thread.Start();
            return threads;
        }

        /// <summary>
        /// Generates threads equal in number to CPU cores count, that will stop only after cancellation by cancellation token.
        /// </summary>
        /// <param name="objPerThread">Array of lists containing objects intended for each thread</param>
        /// <param name="token">Cancellation token to stop simulation</param>
        /// <returns>Array of threads to be started</returns>
        private Thread[] GenerateThreads(List<CelestialObject>[] objPerThread, CancellationToken token)
        {
            Barrier barrier = new Barrier(objPerThread.Length);
            Thread[] threads = new Thread[objPerThread.Length];
            for (int i = 0; i < objPerThread.Length; i++)
            {
                int current = i;
                threads[i] = new Thread(() => { CalculateContinuousSteps(objPerThread[current], barrier, token); });
                threads[i].Name = "Thread " + i;
            }
            return threads;
        }

        /// <summary>
        /// Generates threads equal in number to CPU cores count, that will stop after required number of steps calculated.
        /// </summary>
        /// <param name="objPerThread">Array of lists containing objects intended for each thread</param>
        /// <param name="token">Cancellation token to stop simulation</param>
        /// <returns>Array of threads that can be joined for continuation after calculation</returns>
        private Thread[] GenerateThreads(List<CelestialObject>[] objPerThread, int steps)
        {
            Barrier barrier = new Barrier(objPerThread.Length);
            Thread[] threads = new Thread[objPerThread.Length];
            for (int i = 0; i < objPerThread.Length; i++)
            {
                int current = i;
                threads[i] = new Thread(() => { CalculateContinuousSteps(objPerThread[current], barrier, steps); });
                threads[i].Name = "Thread " + i;
            }
            return threads;
        }

        /// <summary>
        /// Calculaties selected objects predefined number of times.
        /// Running one per each thread with assigned part of objects.
        /// </summary>
        /// <param name="obj">Objects assigned to this thread.</param>
        /// <param name="barrier">Barrier for synchronizing calculation steps between threads</param>
        /// <param name="steps">Number of calculation steps to be counted</param>
        private void CalculateContinuousSteps(IEnumerable<CelestialObject> obj, Barrier barrier, int steps)
        {
            int objCount = obj.Count();
            CelestialObject[] threadObjects = obj.ToArray();
            for (int j = 0; j < steps; j++)
            {
                for (int i = 0; i < objCount; i++)
                {
                    threadObjects[i].CalculateNewVelocity(TimeStep);
                }
                barrier.SignalAndWait();
                for (int i = 0; i < objCount; i++)
                {
                    threadObjects[i].UpdatePosition(TimeStep);
                }
                //barrier.SignalAndWait();
            }
        }

        /// <summary>
        /// Calculaties selected objects until cancelled.
        /// Running one per each thread with assigned part of objects.
        /// </summary>
        /// <param name="obj">Objects assigned to this thread.</param>
        /// <param name="barrier">Barrier for synchronizing calculation steps between threads</param>
        /// <param name="token">Cancellation token to stop simulation</param>
        private void CalculateContinuousSteps(IEnumerable<CelestialObject> obj, Barrier barrier, CancellationToken token)
        {
            int objCount = obj.Count();
            CelestialObject[] threadObjects = obj.ToArray();
            while (true)
            {
                if (token.IsCancellationRequested) break;
                for (int i = 0; i < objCount; i++)
                {
                    threadObjects[i].CalculateNewVelocity(TimeStep);
                }
                barrier.SignalAndWait();
                for (int i = 0; i < objCount; i++)
                {
                    threadObjects[i].UpdatePosition(TimeStep);
                }
                //barrier.SignalAndWait();

            }
        }

        #endregion

        #region Threaded with cycles
        
        /// <summary>
        /// Start continuous simulation that calculates predefined number of steps and waits for signal from UI.
        /// Intended for real-time use, to allow siowing down simulation - calculating only predefined number of steps (and in return constatnd time passage) between each UI redraw.
        /// </summary>
        /// <param name="token">Cancellation token to stop simulation</param>
        /// <param name="stepsPerCycle">Number of steps to be calculated in each UI cycle</param>
        /// <param name="locker">Locker object for Monitor.Wait to wait on</param>
        public void StartThreadsPerCoreInCycles(CancellationToken token, int stepsPerCycle, object locker)
        {
            int procCount = Environment.ProcessorCount;
            int objCount = objects.Length;
            int count = procCount <= objCount ? procCount : objCount;
            List<CelestialObject>[] objPerThread = DivideObjectsPerThreads(count);
            Thread[] threads = GenerateThreadsWithCycles(objPerThread, token, stepsPerCycle, locker);
            foreach (Thread thread in threads) thread.Start();
        }

        /// <summary>
        /// Generates threads equal in number to CPU cores count, that will stop only on cancellation token.
        /// After calculating selecter number of steps each thread will wait for signal from UI thread, to control steps and time passage between each UI redraw.
        /// </summary>
        /// <param name="objPerThread">Array of lists containing objects intended for each thread</param>
        /// <param name="token">Cancellation token to stop simulation</param>
        /// <param name="stepsPerCycle">Number of steps to be calculated in each UI cycle</param>
        /// <param name="locker">Locker object for Monitor.Wait to wait on</param>
        /// <returns>Array of threads to be started</returns>
        private Thread[] GenerateThreadsWithCycles(List<CelestialObject>[] objPerThread, CancellationToken token, int stepsPerCycle, object locker)
        {
            Barrier barrier = new Barrier(objPerThread.Length);
            Thread[] threads = new Thread[objPerThread.Length];
            for (int i = 0; i < objPerThread.Length; i++)
            {
                int current = i;
                threads[i] = new Thread(() => { CalculateContinuousStepsInCycles(objPerThread[current], barrier, token, stepsPerCycle, locker); });
                threads[i].Name = "Thread " + i;
            }
            return threads;
        }

        /// <summary>
        /// Calculaties selected objects until cancelled. After predefined number of steps waits for signal from UI thread to continue.
        /// Running one per each thread with assigned part of objects.
        /// </summary>
        /// <param name="obj">Objects assigned to this thread.</param>
        /// <param name="barrier">Barrier for synchronizing calculation steps between threads</param>
        /// <param name="token">Cancellation token to stop simulation</param>
        /// <param name="stepsPerCycle">Number of steps to be calculated in each UI cycle</param>
        /// <param name="locker">Locker object for Monitor.Wait to wait on</param>
        private void CalculateContinuousStepsInCycles(IEnumerable<CelestialObject> obj, Barrier barrier, CancellationToken token, int stepsPerCycle, object locker)
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
                        threadObjects[i].CalculateNewVelocity(TimeStep);
                    }
                    barrier.SignalAndWait();
                    for (int i = 0; i < objCount; i++)
                    {
                        threadObjects[i].UpdatePosition(TimeStep);
                    }
                    barrier.SignalAndWait();
                }
                lock (locker) Monitor.Wait(locker);
            }
        }

        #endregion

        #region Threaded per object
        /// <summary>
        /// Starts continuous simulation, that will stop after predefined number of steps, creating one thread per object.
        /// Extremely unefficient, not to be used!
        /// </summary>
        /// <param name="steps">Number of steps to calculate</param>
        public Thread[] StartThreadsPerThreadCounted(int steps)
        {
            int count = objects.Length;
            Thread[] threads = GenerateThreadsPerObjectCounted(steps);
            foreach (Thread thread in threads) thread.Start();
            return threads;
        }

        /// <summary>
        /// Generates one thread per object.
        /// </summary>
        /// <param name="steps">Number of steps to calculate</param>
        /// <returns>Array of threads to be started</returns>
        private Thread[] GenerateThreadsPerObjectCounted(int steps)
        {
            int count = objects.Count();
            Barrier barrier = new Barrier(count);
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

        /// <summary>
        /// Calcilates selected object.
        /// Running one per thread. 
        /// </summary>
        /// <param name="currentObject">Object to calculate</param>
        /// <param name="barrier">Barrier to synchronize calculation steps between threads</param>
        /// <param name="steps">Number of steps to calculate</param>
        private void CalculateContinuousStepsCounted(CelestialObject currentObject, Barrier barrier, int steps)
        {
            for (int j = 0; j < steps; j++)
            {
                currentObject.CalculateNewPosition(TimeStep);
                barrier.SignalAndWait();
            }
        }

        #endregion

        #region Common


        /// <summary>
        /// Divides celestial objects equally for each thread to calculate
        /// </summary>
        /// <param name="count">Number of threads</param>
        /// <returns>Array of lists of objects intended for each thread</returns>
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

        private void AssignCalculatableObjects()
        {
            for(int i = 0; i < objects.Length; i++)
            {
                IEnumerable<CelestialObject> co = objects.Where(j => j.IsCalculatable == true && objects[i] != j);
                objects[i].CalculatableObjects = co.ToArray();
            }
        }

        #endregion


        #region Helper methods
        /// <summary>
        /// Serializes Solar System to JSON for future use
        /// </summary>
        /// <param name="path">Path to file</param>
        public void Serialize(string path)
        {
            string jsonString = JsonSerializer.Serialize(Objects, new JsonSerializerOptions() { WriteIndented = true });
            File.WriteAllText(path, jsonString);
        }
        /// <summary>
        /// Deserializes Solar System from JSON file.
        /// Intended to be used only in class constructor!
        /// </summary>
        /// <param name="path">Path to JSON file</param>
        /// <returns>Array of Celestial Objects</returns>
        public static CelestialObject[] Deserialize(string path)
        {
            CelestialObject[] result;
            string jsonString = File.ReadAllText(path);
            result = JsonSerializer.Deserialize<CelestialObject[]>(jsonString);
            return result;
        }
        /// <summary>
        /// Select object by its name
        /// </summary>
        /// <param name="name">Name of celestial object</param>
        /// <returns>Reference to selected object</returns>
        public CelestialObject SelectObject(string name)
        {
            CelestialObject result = objects.First(obj => obj.Name == name); //not First or default so we get exception if not found
            return result;
        }
        #endregion
    }
}
