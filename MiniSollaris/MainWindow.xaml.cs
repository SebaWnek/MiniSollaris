using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace MiniSollaris
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double windowHeight;
        double windowWidth;
        long horizontalRange = 300000000000; //2AU
        long[] viewCenter = { 0, 0 };
        int timeStep = 1; //seconds
        int skipSteps = 1; //steps to skip for plot/simulation
        public MainWindow()
        {
            InitializeComponent();
            windowHeight = background.ActualHeight;
            windowWidth = background.ActualWidth;

        }
    }
}
