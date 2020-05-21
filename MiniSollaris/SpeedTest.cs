using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MiniSollaris
{
    class SpeedTest
    {
        private SolarSystem system;
        private int count;
        Stopwatch stopwatch;

        public SpeedTest(SolarSystem sys, int c)
        {
            system = sys;
            count = c;
            stopwatch = new Stopwatch();
        }

        public void Test(Window window, bool runSerial, bool runParallel, bool runThreaded, bool runThreadedPO, bool runThreadedSlim)
        {
            long serial =0, parallel=0, threaded=0, threadedPerObj=0, threadedSlim=0;
            window.WindowState = WindowState.Minimized;
            if (runSerial)
            {
                serial = RunSerial();
                Debug.WriteLine("Serial test result: " + serial); 
            }
            if (runParallel)
            {
                parallel = RunParallel();
                Debug.WriteLine("Parallel test result:   " + parallel); 
            }
            if (runThreaded)
            {
                threaded = RunThreaded();
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
            }
            if (runThreaded)
            {
                Debug.WriteLine("Threaded speed up:      " + (double)serial / (double)threaded); 
            }
            if (runThreadedPO)
            {
                Debug.WriteLine("ThreadedPO speed up:      " + (double)serial / (double)threadedPerObj); 
            }
            if (runThreadedSlim)
            {
                Debug.WriteLine("ThreadedSlim speed up:      " + (double)serial / (double)threadedSlim); 
            }
            Console.WriteLine();
            if (runSerial)
            {
                Debug.WriteLine("Serial test result: " + serial); 
            }
            if (runParallel)
            {
                Debug.WriteLine("Parallel test result:   " + parallel); 
            }
            if (runThreaded)
            {
                Debug.WriteLine("Threaded test result:   " + threaded); 
            }
            if (runThreadedPO)
            {
                Debug.WriteLine("ThreadedPO test result:   " + threadedPerObj); 
            }
            if (runThreadedSlim)
            {
                Debug.WriteLine("ThreadedSlim test result:   " + threadedSlim); 
            }
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
    }
}
