using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public long RunConcurrent()
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
