﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace MiniSollaris
{
    /// <summary>
    /// Temporary class for benchmarking different algorithms.
    /// </summary>
    class SpeedTest
    {
        private readonly SolarSystem system;
        private readonly int count;
        readonly Stopwatch stopwatch;

        public SpeedTest(SolarSystem sys, int c)
        {
            system = sys;
            count = c;
            stopwatch = new Stopwatch();
        }

        public void Test(bool runSerial,
                         bool runSerialRK,
                         bool runParallel,
                         bool runParallelRK,
                         bool runThreaded,
                         bool runThreadedRK,
                         bool runThreadedWithLocks,
                         bool runThreadedPO,
                         bool runThreadedSlim,
                         string[] additionalInfo)
        {
            using (FileStream fs = new FileStream("testLog.txt", FileMode.Append))
            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.WriteLine($"Time of test: {DateTime.Now}");
                foreach (string str in additionalInfo) writer.WriteLine(str);
                long serial = 0, serialRK = 0, parallel = 0, parallelRK = 0, threaded = 0, threadedWithLocks =0, threadedRK = 0, threadedPerObj = 0, threadedSlim = 0;
                if (runSerial)
                {
                    serial = RunSerial();
                    Debug.WriteLine("Serial test result: " + serial);
                }
                if (runSerialRK)
                {
                    serialRK = RunSerialRK();
                    Debug.WriteLine("SerialRK test result: " + serialRK);
                }
                if (runParallel)
                {
                    parallel = RunParallel();
                    Debug.WriteLine("Parallel test result:   " + parallel);
                }
                if (runParallelRK)
                {
                    parallelRK = RunParallelRK();
                    Debug.WriteLine("ParallelRK test result:   " + parallelRK);
                }
                if (runThreaded)
                {
                    threaded = RunThreaded();
                    Debug.WriteLine("Threaded test result:   " + threaded);
                }
                if (runThreadedRK)
                {
                    threadedRK = RunThreadedRK();
                    Debug.WriteLine("ThreadedRK test result:   " + threadedRK);
                }
                if (runThreadedWithLocks)
                {
                    threadedWithLocks = RunThreadedWithLocks();
                    Debug.WriteLine("Threaded test result:   " + threaded);
                }
                if (runThreadedPO)
                {
                    threadedPerObj = RunThreadedPerObject();
                    Debug.WriteLine("Threaded test result:   " + threadedPerObj);
                }
                if (runThreadedSlim)
                {
                    threadedSlim = RunThreadedSlim();
                    Debug.WriteLine("Threaded test result:   " + threadedSlim);
                }
                if (runParallel)
                {
                    Debug.WriteLine("Parallel speed up:      " + (double)serial / (double)parallel);
                    writer.WriteLine("Parallel speed up:      " + (double)serial / (double)parallel);
                }
                if (runParallelRK)
                {
                    Debug.WriteLine("ParallelRK speed up:      " + (double)serialRK / (double)parallelRK);
                    writer.WriteLine("ParallelRK speed up:      " + (double)serialRK / (double)parallelRK);
                }
                if (runThreaded)
                {
                    Debug.WriteLine("Threaded speed up:      " + (double)serial / (double)threaded);
                    writer.WriteLine("Threaded speed up:      " + (double)serial / (double)threaded);
                }
                if (runThreadedRK)
                {
                    Debug.WriteLine("ThreadedRK speed up:      " + (double)serialRK / (double)threadedRK);
                    writer.WriteLine("ThreadedRK speed up:      " + (double)serialRK / (double)threadedRK);
                }
                if (runThreadedWithLocks)
                {
                    Debug.WriteLine("Threaded with locks speed up:      " + (double)serial / (double)threadedWithLocks);
                    writer.WriteLine("Threaded with locks speed up:      " + (double)serial / (double)threadedWithLocks);
                }
                if (runThreadedPO)
                {
                    Debug.WriteLine("ThreadedPO speed up:      " + (double)serial / (double)threadedPerObj);
                    writer.WriteLine("ThreadedPO speed up:      " + (double)serial / (double)threadedPerObj);
                }
                if (runThreadedSlim)
                {
                    Debug.WriteLine("ThreadedSlim speed up:      " + (double)serial / (double)threadedSlim);
                    writer.WriteLine("ThreadedSlim speed up:      " + (double)serial / (double)threadedSlim);
                }
                Console.WriteLine();
                if (runSerial)
                {
                    Debug.WriteLine("Serial test result: " + serial);
                    writer.WriteLine("Serial test result: " + serial);
                }
                if (runSerialRK)
                {
                    Debug.WriteLine("SerialRK test result: " + serialRK);
                    writer.WriteLine("SerialRK test result: " + serialRK);
                }
                if (runParallel)
                {
                    Debug.WriteLine("Parallel test result:   " + parallel);
                    writer.WriteLine("Parallel test result:   " + parallel);
                }
                if (runParallelRK)
                {
                    Debug.WriteLine("ParallelRK test result:   " + parallelRK);
                    writer.WriteLine("ParallelRK test result:   " + parallelRK);
                }
                if (runThreaded)
                {
                    Debug.WriteLine("Threaded test result:   " + threaded);
                    writer.WriteLine("Threaded test result:   " + threaded);
                }
                if (runThreadedRK)
                {
                    Debug.WriteLine("ThreadedRK test result:   " + threadedRK);
                    writer.WriteLine("ThreadedRK test result:   " + threadedRK);
                }
                if (runThreadedWithLocks)
                {
                    Debug.WriteLine("Threaded test result:   " + threadedWithLocks);
                    writer.WriteLine("Threaded with locks test result:   " + threadedWithLocks);
                }
                if (runThreadedPO)
                {
                    Debug.WriteLine("ThreadedPO test result:   " + threadedPerObj);
                    writer.WriteLine("ThreadedPO test result:   " + threadedPerObj);
                }
                if (runThreadedSlim)
                {
                    Debug.WriteLine("ThreadedSlim test result:   " + threadedSlim);
                    writer.WriteLine("ThreadedSlim test result:   " + threadedSlim);
                }
                writer.WriteLine();
            }
            MessageBox.Show("Tests done!");
            Environment.Exit(0);
        }

        public long RunThreaded()
        {
            stopwatch.Reset();
            stopwatch.Start();
            Thread[] threads = system.StartThreadsPerCoreCounted(count);
            foreach (Thread thread in threads) thread.Join();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }
        public long RunThreadedRK()
        {
            stopwatch.Reset();
            stopwatch.Start();
            Thread[] threads = system.StartThreadsPerCoreRK(count);
            foreach (Thread thread in threads) thread.Join();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }
        public long RunThreadedWithLocks()
        {
            stopwatch.Reset();
            stopwatch.Start();
            Thread[] threads = system.StartThreadsPerCoreCountedWithLocker(count);
            foreach (Thread thread in threads) thread.Join();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        public long RunThreadedSlim()
        {
            stopwatch.Reset();
            stopwatch.Start();
            Thread[] threads = system.StartThreadsSlim(count);
            foreach (Thread thread in threads) thread.Join();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        public long RunThreadedPerObject()
        {
            stopwatch.Reset();
            stopwatch.Start();
            Thread[] threads = system.StartThreadsPerThreadCounted(count);
            foreach (Thread thread in threads) thread.Join();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        public long RunSerial()
        {
            stopwatch.Reset();
            stopwatch.Start();
            for (int i = 0; i < count; i++)
            {
                system.CalculateStep();
            }
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }
        public long RunSerialRK()
        {
            stopwatch.Reset();
            stopwatch.Start();
            for (int i = 0; i < count; i++)
            {
                system.CalculateStepRK();
            }
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        public long RunParallel()
        {
            stopwatch.Reset();
            stopwatch.Start();
            for (int i = 0; i < count; i++)
            {
                system.CalculateStepParallel();
            }
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }
        public long RunParallelRK()
        {
            stopwatch.Reset();
            stopwatch.Start();
            for (int i = 0; i < count; i++)
            {
                system.CalculateStepParallelRK();
            }
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }
    }
}
