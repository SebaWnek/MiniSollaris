using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MiniSollaris
{
    /// <summary>
    /// Temporary class to view results of simulation.
    /// </summary>
    /// <remarks>
    /// This is just basic code to be able to visualise results. It's not intended for final use, so it contains a lot of far-from-perfect code. 
    /// As this is just mostly back-end project in the future there will be more advanced MVVM front end with back-end logic developed here, or maybe even Unity project if I finally learn it. 
    /// For now please, don't consider this class as finished, or comment on some extremely basic solutsions like redrawing after some number of steps instead of using timer etc.
    /// </remarks>
    public partial class MainWindow : Window
    {
        double windowHeight;
        double windowWidth;
        readonly long horizontalRange = 1000000000000; //2AU
        CelestialObject selectedObject = null;
        long[] viewCenter = { 0, 0 };
        readonly double timeStep = 0.2; //seconds
        readonly int skipSteps = 1; //steps to skip for plot/simulation
        CancellationTokenSource tokenSource;
        DispatcherTimer timer;
        object locker;
        object[] lockers;

        WindowCalculatorHelper windowCalculatorHelper;
        SolarSystem system;

        /// <summary>
        /// Main method
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Simulation is started here. In case of benchmarking uncomment <code>Test()</code> and then after running it will perform benchmarks and close.
        /// </summary>
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            windowHeight = background.ActualHeight;
            windowWidth = background.ActualWidth;
            windowCalculatorHelper = new WindowCalculatorHelper(windowHeight, windowWidth, horizontalRange, new long[] { 0, 0 });

            //system = DataAccess.InitializeHardcodedSystem(timeStep, 10);
            system = DataAccess.InitializeFromJSON("system", timeStep);
            AddGraphics();
            RedrawAll();
            //system.Serialize("system");
            //SelectNewCenter("Mars");
            //SelectNewRange(150000000);

            while (true)
            {
                await Task.Delay(5000);
                SelectNewCenter("Mars");
                SelectNewRange(150000000);
                await Task.Delay(5000);
                SelectNewCenter("Earth");
                SelectNewRange(1500000000);
                await Task.Delay(5000);
                SelectNewCenter(new long[] { 0, 0 });
                SelectNewRange(1000000000000);
            }


            //Test();
        }

        /// <summary>
        /// For benchmarking purposes.
        /// </summary>
        private void Test()
        {
            int testSteps = 10000;
            SpeedTest test = new SpeedTest(system, testSteps);
            string[] info = { $"Time Step: {timeStep}", $"Test Steps: {testSteps}", $"Objects Count: {system.ObjectsCount}" };
            test.Test(true, true, true, true, false, false, info);
        }

        #region Graphics

        /// <summary>
        /// Allows selecting new object to be center of view.
        /// </summary>
        /// <remarks>
        /// View will follow selected object keeping it in the middle.
        /// </remarks>
        /// <param name="name">Name of object to be followed by view</param>
        public void SelectNewCenter(string name)
        {
            selectedObject = system.SelectObject(name);
        }

        /// <summary>
        /// Allows selecting new static center of view. 
        /// </summary>
        /// <param name="position">World coordinates of new view center</param>
        public void SelectNewCenter(long[] position)
        {
            selectedObject = null;
            viewCenter = position;
            windowCalculatorHelper.ChangeCenter(viewCenter);
        }

        /// <summary>
        /// Allows changing visible range and in turn scale.
        /// </summary>
        /// <param name="range">New horizontal range of visible part of simulation</param>
        public void SelectNewRange(long range)
        {
            windowCalculatorHelper.ChangeScale(range);
        }

        /// <summary>
        /// Adds graphic objects to canvas background representing each object.
        /// </summary>
        /// <remarks>
        /// For now it adds just elipse object from each object. 
        /// For future each object should have better graphical representation, containing graphical object like elipse, or image and, on demmand, text objects with object data.
        /// For now yet, as this is just strictly basic visualisation class it should be enough.
        /// </remarks>
        private void AddGraphics()
        {
            foreach (CelestialObject obj in system.Objects)
            {
                background.Children.Add(obj.Picture);
                RedrawObject(obj);
            }
        }

        /// <summary>
        /// Redraws all objects.
        /// </summary>
        private void RedrawAll()
        {
            if (selectedObject != null)
            {
                viewCenter = selectedObject.Position;
                windowCalculatorHelper.ChangeCenter(viewCenter);
            }
            foreach (CelestialObject obj in system.Objects)
            {
                RedrawObject(obj);
            }
        }

        /// <summary>
        /// Redraws single object.
        /// </summary>
        /// <param name="obj">Object to be redrawn</param>
        private void RedrawObject(CelestialObject obj)
        {
            double[] position = windowCalculatorHelper.CalculateScreenPosition(obj.Position);
            Canvas.SetLeft(obj.Picture, position[0] - obj.Picture.ActualWidth / 2);
            Canvas.SetTop(obj.Picture, position[1] - obj.Picture.ActualHeight / 2);
        }
        #endregion

        #region Controls
        /// <summary>
        /// Starts simulation.
        /// </summary>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            tokenSource = new CancellationTokenSource();
            //await Animate(system.CalculateStep, tokenSource.Token);
            AnimateThreaded(tokenSource.Token);
        }

        /// <summary>
        /// Saves solar system data for potential future so simulation can be contiuned. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            tokenSource.Cancel();
            system.Serialize("system.txt");
        }

        /// <summary>
        /// Stops simulation.
        /// </summary>
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (tokenSource != null)
            {
                tokenSource.Cancel();
                tokenSource.Dispose(); 
            }
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// Event to be used with <code>*InCycles</code> methods.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If calculation time doesn't exceed time between each tick, simulation should stop and await on <code>Monitor.PulseAll(locker)</code> from UI thread.
        /// After receiving that signal calculations should resume for next cycle.</para>
        /// <para>
        /// Allows detailed control of simulation speed in real-time use.
        /// </para>
        /// <para>
        /// If calculating selected number of steps will exceed time between two ticks it will work like normal Timer_Tick signal will be ignored and calculations will continue until finished and await pulse in following tick.
        /// </para>
        /// </remarks>
        private void Timer_TickStepped(object sender, EventArgs e)
        {
            RedrawAll();
            lock (locker) Monitor.PulseAll(locker);
        }

        /// <summary>
        /// Event to be used with continuous methods. 
        /// </summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            OtherHelper.MultiLock(lockers, lockers.Length, () =>
            {
                RedrawAll();
            });
        }
        #endregion

        #region Animate

        /// <summary>
        /// Runs animation with serial or parallel algorithms. Can't be stopped.
        /// </summary>
        /// <remarks>
        /// This is extremely rudimentary - but works, and as in the end I'm not using neither serial nor parallel algorithm there wasn't a point in creating better method.
        /// </remarks>
        /// <param name="action">Either threaded or Parallel step calculating</param>
        /// <returns>Task to be awaited</returns>
        private async Task Animate(Action action)
        {
            for (int i = 1; i < skipSteps; i++)
            {
                //system.CalculateStep();
                action();
                if (i % skipSteps == 0)
                {
                    await Task.Delay(1);
                    RedrawAll();
                    i = 1;
                }
            }
        }

        /// <summary>
        /// Runs animation with serial or parallel algorithms. Can be stopper by cancellation token.
        /// </summary>
        /// <remarks>
        /// This is extremely rudimentary - but works, and as in the end I'm not using neither serial nor parallel algorithm there wasn't a point in creating better method.
        /// </remarks>
        /// <param name="action">Either threaded or Parallel step calculating</param>
        /// <param name="token"></param>
        /// <returns>Task to be awaited</returns>
        private async Task Animate(Action action, CancellationToken token)
        {
            for (int i = 0; i < int.MaxValue; i++)
            {
                system.CalculateStep();
                action();
                if (i % skipSteps == 0)
                {
                    await Task.Delay(1);
                    RedrawAll();
                }
                if (token.IsCancellationRequested) break;
            }
        }

        /// <summary>
        /// Redraws UI in cycles with simulation running at maximum possible speed.
        /// </summary>
        /// <remarks>
        /// <code>system.StartThreadsPerCoreWithLocker(token)</code> is hardcoded, as this method is optimalized for UI use. 
        /// Could use non-locked version, but will generate position artifacts with smaller view ranges. 
        /// </remarks>
        /// <param name="token">Cancellation token to stop simulation</param>
        private void AnimateThreaded(CancellationToken token)
        {
            timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 30)
            };
            timer.Tick += Timer_Tick;
            lockers = system.StartThreadsPerCoreWithLocker(token);
            timer.Start();
        }

        /// <summary>
        /// Runs UI in cycles with simulation speed controlled by number of simulation steps calculated between each redraw.
        /// </summary>
        /// <param name="token">Cancellation token to stop simulation</param>
        private void AnimateThreadedStepped(CancellationToken token)
        {
            locker = new object();
            timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 30)
            };
            timer.Tick += Timer_TickStepped;
            system.StartThreadsPerCoreInCycles(token, skipSteps, locker);
            timer.Start();
        }
        #endregion


    }
}
