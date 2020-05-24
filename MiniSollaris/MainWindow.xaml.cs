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
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double windowHeight;
        double windowWidth;
        long horizontalRange = 1000000000000; //2AU
        CelestialObject selectedObject = null;
        long[] viewCenter = { 0, 0 };
        double timeStep = 1; //seconds
        int skipSteps = 1; //steps to skip for plot/simulation
        CancellationTokenSource tokenSource;
        DispatcherTimer timer;
        object locker;
        object[] lockers;

        WindowCalculatorHelper windowCalculatorHelper;
        SolarSystem system;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            windowHeight = background.ActualHeight;
            windowWidth = background.ActualWidth;
            windowCalculatorHelper = new WindowCalculatorHelper(windowHeight, windowWidth, horizontalRange, new long[] { 0, 0 });

            system = DataAccess.InitializeHardcodedSystem(timeStep, 2000);
            AddGraphics();
            RedrawAll();

            //SelectNewCenter("Mars");
            //SelectNewRange(150000000);


            Test();
        }


        private void Test()
        {
            int testSteps = 10000;
            SpeedTest test = new SpeedTest(system, testSteps);
            string[] info = { $"Time Step: {timeStep}", $"Test Steps: {testSteps}", $"Objects Count: {system.ObjectsCount}" };
            test.Test(Window.GetWindow(this), true, true, true, true, false, false, info);
        }

        #region Graphics
        public void SelectNewCenter(string name)
        {
            selectedObject = system.SelectObject(name);
        }

        public void SelectNewCenter(long[] position)
        {
            selectedObject = null;
            viewCenter = position;
            windowCalculatorHelper.ChangeCenter(viewCenter);
        }

        public void SelectNewRange(long range)
        {
            windowCalculatorHelper.ChangeScale(range);
        }

        private void AddGraphics()
        {
            foreach (CelestialObject obj in system.Objects)
            {
                background.Children.Add(obj.Picture);
                RedrawObject(obj);
            }
        }

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

        private void RedrawObject(CelestialObject obj)
        {
            double[] position = windowCalculatorHelper.CalculateScreenPosition(obj.Position);
            Canvas.SetLeft(obj.Picture, position[0] - obj.Picture.ActualWidth / 2);
            Canvas.SetTop(obj.Picture, position[1] - obj.Picture.ActualHeight / 2);
        }
        #endregion

        #region Controls
        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            tokenSource = new CancellationTokenSource();
            //await Animate(system.CalculateStep, tokenSource.Token);
            AnimateThreaded(tokenSource.Token);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            tokenSource.Cancel();
            system.Serialize("system.txt");
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            tokenSource.Cancel();
            tokenSource.Dispose();
        }
        private void Timer_TickStepped(object sender, EventArgs e)
        {
            RedrawAll();
            lock (locker) Monitor.PulseAll(locker);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            OtherHelper.MultiLock(lockers, lockers.Length, () =>
            {
                RedrawAll();
            });
        }
        #endregion

        #region Animate
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

        private void AnimateThreaded(CancellationToken token)
        {
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 30);
            timer.Tick += Timer_Tick;
            lockers = system.StartThreadsPerCoreWithLocker(token);
            timer.Start();
        }

        private void AnimateThreadedStepped(CancellationToken token)
        {
            locker = new object();
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 30);
            timer.Tick += Timer_TickStepped;
            system.StartThreadsPerCoreInCycles(token, skipSteps, locker);
            timer.Start();
        }
        #endregion

    }
}
