using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MiniSollaris
{
    public static class AccuracyBenchmark
    {
        /*
        Raw data 10 days:
        Deimos:
            2459001.500000000 = A.D. 2020-Jun-01 00:00:00.0000 TDB 
        9.292900233975990E+07 -1.876649526838233E+08 -6.242563193821013E+06
        2.137558517721452E+01  1.282116396869707E+01  2.741372184803437E-01
            2459011.500000000 = A.D. 2020-Jun-11 00:00:00.0000 TDB 
        1.119379696057207E+08 -1.755851039920129E+08 -6.460150480507791E+06
        2.025812448661597E+01  1.572134176390718E+01  3.363763557953297E-01

        Phobos
            2459001.500000000 = A.D. 2020-Jun-01 00:00:00.0000 TDB 
        9.293569947874898E+07 -1.876933471303736E+08 -6.248106829306796E+06
        2.363302859419011E+01  1.466218554288201E+01 -7.112486668584443E-01
            2459011.500000000 = A.D. 2020-Jun-11 00:00:00.0000 TDB 
        1.119264349469582E+08 -1.755967460415063E+08 -6.456266447306663E+06
        1.939938288308386E+01  1.479573886208244E+01  7.306059717606566E-01

        Mars
            2459001.500000000 = A.D. 2020-Jun-01 00:00:00.0000 TDB 
        9.292863773354495E+07 -1.876883272843097E+08 -6.244428598046154E+06
        2.260573283252675E+01  1.284657949784173E+01 -2.852031779968884E-01
             2459011.500000000 = A.D. 2020-Jun-11 00:00:00.0000 TDB 
        1.119275014834447E+08 -1.756058804088263E+08 -6.457177845185742E+06
        2.133045752570017E+01  1.510343984456725E+01 -2.066153138674549E-01

             Earth
             2459001.500000000 = A.D. 2020-Jun-01 00:00:00.0000 TDB 
        5.090344138819925E+07 -1.421208588831371E+08  1.502222254326940E+04
        2.761277880737265E+01 -9.953139875921677E+00  9.696940372618812E-04
             2459011.500000000 = A.D. 2020-Jun-11 00:00:00.0000 TDB 
        2.645654323212675E+07 -1.486649100082768E+08  1.639604612983763E+04
        2.885306375653031E+01 -5.164544966459705E+00  1.251643281117598E-03

             Moon
             2459001.500000000 = A.D. 2020-Jun-01 00:00:00.0000 TDB 
        5.126732045566156E+07 -1.421548733244768E+08  4.841062077829987E+04
        2.773708143919783E+01 -1.101400555919551E+01 -1.220668925950985E-02
             2459011.500000000 = A.D. 2020-Jun-11 00:00:00.0000 TDB 
        2.613504766857735E+07 -1.488918192835108E+08 -1.370618946789950E+04
        2.946456719138615E+01 -4.383784308607676E+00 -5.381993340891467E-02

             Sun
             2459001.500000000 = A.D. 2020-Jun-01 00:00:00.0000 TDB 
        7.551930378359107E+05  1.050698175400754E+06  8.678822746303806E+03
        1.403999536832919E-02 -6.200795838603056E-03  4.032009148837962E-04
             2459011.500000000 = A.D. 2020-Jun-11 00:00:00.0000 TDB 
        7.673014539301289E+05  1.045243089093115E+06  9.027043376729998E+03
        1.398646037928305E-02 -6.427267777182033E-03  4.027310610427684E-04
        */

        /*Raw Data 60 days:
         Mars:
        2459001.500000000 = A.D. 2020-Jun-01 00:00:00.0000 TDB 
   9.292863773354495E+07 -1.876883272843097E+08 -6.244428598046154E+06
   2.260573283252675E+01  1.284657949784173E+01 -2.852031779968884E-01
        2459061.500000000 = A.D. 2020-Jul-31 00:00:00.0000 TDB 
   1.847750267445320E+08 -8.965035795482343E+07 -6.442231995050564E+06
   1.154640587399491E+01  2.383453739395394E+01  2.164139742219540E-01

        Sun:
         2459001.500000000 = A.D. 2020-Jun-01 00:00:00.0000 TDB 
  -7.551930378359107E+05  1.050698175400754E+06  8.678822746303806E+03
  -1.403999536832919E-02 -6.200795838603056E-03  4.032009148837962E-04
        2459061.500000000 = A.D. 2020-Jul-31 00:00:00.0000 TDB 
  -8.268300005034901E+05  1.015057174066017E+06  1.074669324622460E+04
  -1.352077556688332E-02 -7.522151267579006E-03  3.916241961903644E-04

        Earth:
        2459001.500000000 = A.D. 2020-Jun-01 00:00:00.0000 TDB 
  -5.090344138819925E+07 -1.421208588831371E+08  1.502222254326940E+04
   2.761277880737265E+01 -9.953139875921677E+00  9.696940372618812E-04
        2459061.500000000 = A.D. 2020-Jul-31 00:00:00.0000 TDB 
   9.262046940434681E+07 -1.186818210741035E+08  1.602224643202871E+04
   2.296997983958195E+01  1.821353102199808E+01  6.130046160537361E-04

        Moon:
        2459001.500000000 = A.D. 2020-Jun-01 00:00:00.0000 TDB 
  -5.126732045566156E+07 -1.421548733244768E+08  4.841062077829987E+04
   2.773708143919783E+01 -1.101400555919551E+01 -1.220668925950985E-02
        2459061.500000000 = A.D. 2020-Jul-31 00:00:00.0000 TDB 
   9.257412100739263E+07 -1.190565516973682E+08  1.925661025305092E+04
   2.399494477402395E+01  1.805330152790824E+01 -9.263241606464767E-02

        Phobos:
        2459001.500000000 = A.D. 2020-Jun-01 00:00:00.0000 TDB 
   9.293569947874898E+07 -1.876933471303736E+08 -6.248106829306796E+06
   2.363302859419011E+01  1.466218554288201E+01 -7.112486668584443E-01
        2459061.500000000 = A.D. 2020-Jul-31 00:00:00.0000 TDB 
   1.847830095388835E+08 -8.964740556981444E+07 -6.446107263509717E+06
   1.094760509550355E+01  2.585612346196570E+01  6.002389141672584E-01

        Deimos:
        2459001.500000000 = A.D. 2020-Jun-01 00:00:00.0000 TDB 
   9.292900233975990E+07 -1.876649526838233E+08 -6.242563193821013E+06
   2.137558517721452E+01  1.282116396869707E+01  2.741372184803437E-01
        2459061.500000000 = A.D. 2020-Jul-31 00:00:00.0000 TDB 
   1.847782631018424E+08 -8.967333656312339E+07 -6.445696836286183E+06
   1.276240826012666E+01  2.408620766737257E+01 -3.156603462972161E-01
         */

        static Stopwatch stopwatch = new Stopwatch();
        static SolarSystem system;

        static long[] sun1 = { 755193037, 1050698175 };
        static long[] sun2 = { -826830000, 1015057174 };
        static long[] earth1 = { 50903441388, -142120858883 };
        static long[] earth2 = { 92620469404, -118681821074 };
        static long[] moon1 = { 51267320455, -142154873324 };
        static long[] moon2 = { 92574121007, -119056551697 };
        static long[] mars1 = { 92928637733, -187688327284 };
        static long[] mars2 = { 184775026744, -89650357954 };
        static long[] phobos1 = { 92935699478, -187693347130 };
        static long[] phobos2 = { 184783009538, -89647405569 };
        static long[] deimos1 = { 92929002339, -187664952683 };
        static long[] deimos2 = { 184778263101, -89673336563 };

        static long HorizonsSunEart = CalculateDistance(sun2, earth2);
        static long HorizonsEarthMoon = CalculateDistance(earth2, moon2);
        static long HorizonsMarsPhobos = CalculateDistance(mars2, phobos2);
        static long HorizonsMarsDeimos = CalculateDistance(mars2, deimos2);
        static long HorizonsEarthMars = CalculateDistance(earth2, mars2);

        static long eulerSunEarthSerial, eulerEarthMoonSerial, eulerMarsPhobosSerial, eulerMarsDeimosSerial, eulerEarthMarsSerial;
        static long eulerSunEarthParallel, eulerEarthMoonParallel, eulerMarsPhobosParallel, eulerMarsDeimosParallel, eulerEarthMarsParallel;
        static long eulerSunEarthThreaded, eulerEarthMoonThreaded, eulerMarsPhobosThreaded, eulerMarsDeimosThreaded, eulerEarthMarsThreaded;
        static long RK4SunEarth, RK4EarthMoon, RK4MarsPhobos, RK4MarsDeimos, RK4EarthMars;
        static long serialTime, parallelTime, threadedTime, RK4Time;

        static double ses, ems, mps, mds, emas;
        static double sep, emp, mpp, mdp, emap;
        static double set, emt, mpt, mts, emat;
        static double serk, emrk, mprk, mdrk, emark;

        static public void RunTest(SolarSystem sys, int eulerSteps, int RK4steps, double eulerTimeStep, double RK4TimeStep, bool runSerial, bool runParallel, bool runThreaded, bool runRK4)
        {
            system = sys;
            using (FileStream fs = new FileStream("accuracyTestLog.txt", FileMode.Append))
            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.WriteLine($"Time of test: {DateTime.Now}");
                if (runSerial)
                {
                    (eulerSunEarthSerial, eulerEarthMoonSerial, eulerMarsPhobosSerial, eulerMarsDeimosSerial, eulerEarthMarsSerial, serialTime) = RunSerial(eulerSteps, eulerTimeStep);
                }
                if (runParallel)
                {
                    (eulerSunEarthParallel, eulerEarthMoonParallel, eulerMarsPhobosParallel, eulerMarsDeimosParallel, eulerEarthMarsParallel, parallelTime) = RunParallel(eulerSteps, eulerTimeStep);
                }
                if (runThreaded)
                {
                    (eulerSunEarthThreaded, eulerEarthMoonThreaded, eulerMarsPhobosThreaded, eulerMarsDeimosThreaded, eulerEarthMarsThreaded, threadedTime) = RunThreaded(eulerSteps, eulerTimeStep);
                }
                if (runRK4)
                {
                    (RK4SunEarth, RK4EarthMoon, RK4MarsPhobos, RK4MarsDeimos, RK4EarthMars, RK4Time) = RunRK4(RK4steps, RK4TimeStep);
                }

                ses = 100 *   (1 - (double)eulerSunEarthSerial / HorizonsSunEart);
                ems = 100 *   (1 - (double)eulerEarthMoonSerial / HorizonsEarthMoon);
                mps = 100 *   (1 - (double)eulerMarsPhobosSerial / HorizonsMarsPhobos);
                mds = 100 *   (1 - (double)eulerMarsDeimosSerial / HorizonsMarsDeimos);
                emas = 100 *  (1 - (double)eulerEarthMarsSerial / HorizonsEarthMars);
                sep = 100 *   (1 - (double)eulerSunEarthParallel / HorizonsSunEart);
                emp = 100 *   (1 - (double)eulerEarthMoonParallel / HorizonsEarthMoon);
                mpp = 100 *   (1 - (double)eulerMarsPhobosParallel / HorizonsMarsPhobos);
                mdp = 100 *   (1 - (double)eulerMarsDeimosParallel / HorizonsMarsDeimos);
                emap = 100 *  (1 - (double)eulerEarthMarsParallel / HorizonsEarthMars);
                set = 100 *   (1 - (double)eulerSunEarthThreaded / HorizonsSunEart);
                emt = 100 *   (1 - (double)eulerEarthMoonThreaded / HorizonsEarthMoon);
                mpt = 100 *   (1 - (double)eulerMarsPhobosThreaded / HorizonsMarsPhobos);
                mts = 100 *   (1 - (double)eulerMarsDeimosThreaded / HorizonsMarsDeimos);
                emat = 100 *  (1 - (double)eulerEarthMarsThreaded / HorizonsEarthMars);
                serk = 100 *  (1 - (double)RK4SunEarth / HorizonsSunEart);
                emrk = 100 *  (1 - (double)RK4EarthMoon / HorizonsEarthMoon);
                mprk = 100 *  (1 - (double)RK4MarsPhobos / HorizonsMarsPhobos);
                mdrk = 100 *  (1 - (double)RK4MarsDeimos / HorizonsMarsDeimos);
                emark = 100 * (1 - (double)RK4EarthMars / HorizonsEarthMars);

                writer.WriteLine($"Euler steps: {eulerSteps}, time step: {eulerTimeStep}");
                writer.WriteLine($"Runge Kutta RK4 steps: {RK4steps}, time step: {RK4TimeStep}");
                writer.WriteLine();
                writer.WriteLine("Body pair, Horizons distance, serial distance, parallel dostance, threaded distance, RK4 distance:");
                writer.WriteLine($"Sun to Earth:   {HorizonsSunEart}, {eulerSunEarthSerial}, {eulerSunEarthParallel}, {eulerSunEarthThreaded}, {RK4SunEarth}");
                writer.WriteLine($"Earth to Moon:  {HorizonsEarthMoon}, {eulerEarthMoonSerial}, {eulerEarthMoonParallel}, {eulerEarthMoonThreaded}, {RK4EarthMoon}");
                writer.WriteLine($"Mars to Phobos: {HorizonsMarsPhobos}, {eulerMarsPhobosSerial}, {eulerMarsPhobosParallel}, {eulerMarsPhobosThreaded}, {RK4MarsPhobos}");
                writer.WriteLine($"Mars to Deimos: {HorizonsMarsDeimos}, {eulerMarsDeimosSerial}, {eulerMarsDeimosParallel}, {eulerMarsDeimosThreaded}, {RK4MarsDeimos}");
                writer.WriteLine($"Earth to Mars:  {HorizonsEarthMars}, {eulerEarthMarsSerial}, {eulerEarthMarsParallel}, {eulerEarthMarsThreaded}, {RK4EarthMars}");
                writer.WriteLine();
                writer.WriteLine("Body pair, serial error, parallel error, threaded error, RK4 error:");
                writer.WriteLine($"Sun to Earth:   {ses}%, {sep}%, {set}%, {serk}%");
                writer.WriteLine($"Earth to Moon:  {ems}%, {emp}%, {emt}%, {emrk}%");
                writer.WriteLine($"Mars to Phobos: {mps}%, {mpp}%, {mpt}%, {mprk}%");
                writer.WriteLine($"Mars to Deimos: {mds}%, {mdp}%, {mts}%, {mdrk}%");
                writer.WriteLine($"Earth to Mars:  {emas}%, {emap}%, {emat}%, {emark}%");
                writer.WriteLine();
                writer.WriteLine($"Times: Srial: {serialTime}, Parallel: {parallelTime}, Threaded: {threadedTime}, RK4: {RK4Time}");
                writer.WriteLine();
                writer.WriteLine("---------------------------------------------------------------------------------------------------------");
                writer.WriteLine();
            }
            MessageBox.Show("Tests done!");
            Environment.Exit(0);
        }

        private static (long sunEarth, long earthMoon, long marsPhobos, long marsDeimos, long earthMars, long serialTime) RunSerial(int eulerSteps, double eulerTimeStep)
        {
            system.Reset("system.json", eulerTimeStep);
            stopwatch.Reset();
            stopwatch.Start();
            for (int i = 0; i < eulerSteps; i++)
            {
                system.CalculateStep();
            }
            stopwatch.Stop();
            long[] sun = system.SelectObject("Sun").Position;
            long[] earth = system.SelectObject("Earth").Position;
            long[] moon = system.SelectObject("Moon").Position;
            long[] mars = system.SelectObject("Mars").Position;
            long[] phobos = system.SelectObject("Phobos").Position;
            long[] deimos = system.SelectObject("Deimos").Position;
            long sunEarth = CalculateDistance(sun, earth);
            long earthMoon = CalculateDistance(earth, moon);
            long marsPhobos = CalculateDistance(mars, phobos);
            long marsDeimos = CalculateDistance(mars, deimos);
            long earthMars = CalculateDistance(earth, mars);
            long serialTime = stopwatch.ElapsedMilliseconds;
            return (sunEarth, earthMoon, marsPhobos, marsDeimos, earthMars, serialTime);
        }

        private static (long sunEarth, long earthMoon, long marsPhobos, long marsDeimos, long earthMars, long parallelTime) RunParallel(int eulerSteps, double eulerTimeStep)
        {
            system.Reset("system.json", eulerTimeStep);
            stopwatch.Reset();
            stopwatch.Start();
            for (int i = 0; i < eulerSteps; i++)
            {
                system.CalculateStepParallel();
            }
            stopwatch.Stop();
            long[] sun = system.SelectObject("Sun").Position;
            long[] earth = system.SelectObject("Earth").Position;
            long[] moon = system.SelectObject("Moon").Position;
            long[] mars = system.SelectObject("Mars").Position;
            long[] phobos = system.SelectObject("Phobos").Position;
            long[] deimos = system.SelectObject("Deimos").Position;
            long sunEarth = CalculateDistance(sun, earth);
            long earthMoon = CalculateDistance(earth, moon);
            long marsPhobos = CalculateDistance(mars, phobos);
            long marsDeimos = CalculateDistance(mars, deimos);
            long earthMars = CalculateDistance(earth, mars);
            long parallelTime = stopwatch.ElapsedMilliseconds;
            return (sunEarth, earthMoon, marsPhobos, marsDeimos, earthMars, parallelTime);
        }

        private static (long sunEarth, long earthMoon, long marsPhobos, long marsDeimos, long earthMars, long threadedTime) RunThreaded(int eulerSteps, double eulerTimeStep)
        {
            system.Reset("system.json", eulerTimeStep);
            stopwatch.Reset();
            stopwatch.Start();
            Thread[] threads = system.StartThreadsPerCoreCounted(eulerSteps);
            foreach (Thread thread in threads) thread.Join();
            stopwatch.Stop();
            long[] sun = system.SelectObject("Sun").Position;
            long[] earth = system.SelectObject("Earth").Position;
            long[] moon = system.SelectObject("Moon").Position;
            long[] mars = system.SelectObject("Mars").Position;
            long[] phobos = system.SelectObject("Phobos").Position;
            long[] deimos = system.SelectObject("Deimos").Position;
            long sunEarth = CalculateDistance(sun, earth);
            long earthMoon = CalculateDistance(earth, moon);
            long marsPhobos = CalculateDistance(mars, phobos);
            long marsDeimos = CalculateDistance(mars, deimos);
            long earthMars = CalculateDistance(earth, mars);
            long threadedTime = stopwatch.ElapsedMilliseconds;
            return (sunEarth, earthMoon, marsPhobos, marsDeimos, earthMars, threadedTime);
        }

        private static (long sunEarth, long earthMoon, long marsPhobos, long marsDeimos, long earthMars, long RK4Time) RunRK4(int rK4steps, double rK4TimeStep)
        {
            system.Reset("system.json", rK4TimeStep);
            stopwatch.Reset();
            stopwatch.Start();
            Thread[] threads = system.StartThreadsPerCoreRK(rK4steps);
            foreach (Thread thread in threads) thread.Join();
            stopwatch.Stop();
            long[] sun = system.SelectObject("Sun").Position;
            long[] earth = system.SelectObject("Earth").Position;
            long[] moon = system.SelectObject("Moon").Position;
            long[] mars = system.SelectObject("Mars").Position;
            long[] phobos = system.SelectObject("Phobos").Position;
            long[] deimos = system.SelectObject("Deimos").Position;
            long sunEarth = CalculateDistance(sun, earth);
            long earthMoon = CalculateDistance(earth, moon);
            long marsPhobos = CalculateDistance(mars, phobos);
            long marsDeimos = CalculateDistance(mars, deimos);
            long earthMars = CalculateDistance(earth, mars);
            long RK4Time = stopwatch.ElapsedMilliseconds;
            return (sunEarth, earthMoon, marsPhobos, marsDeimos, earthMars, RK4Time);
        }

        static long CalculateDistance(long[] obj1, long[] obj2)
        {
            double dx = obj2[0] - obj1[0];
            double dy = obj2[1] - obj1[1];
            return (long)Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
