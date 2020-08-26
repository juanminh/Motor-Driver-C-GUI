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
using System.Windows.Shapes;
using MotorController.ViewModels;
using MotorController.Models.DriverBlock;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using System.Windows.Interop;

namespace MotorController.Views
{
    /// <summary>
    /// Interaction logic for ParametarsWindow.xaml
    /// </summary>
    public partial class ParametarsWindow : Window
    {
        public static int ParametersWindowTabSelected = -1;
        public static bool WindowsOpen = false;

        private static readonly object Synlock = new object();
        private static ParametarsWindow _instance;
        public static ParametarsWindow GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new ParametarsWindow();
                    var vaultHwnd = Process.GetCurrentProcess().MainWindowHandle; //Vault window handle
                    new WindowInteropHelper(_instance) { Owner = vaultHwnd };
                    return _instance;
                }
            }
        }

        public ParametarsWindow()
        {
            InitializeComponent();
            ParametarsWindow.WindowsOpen = true;
        }
        ~ParametarsWindow()
        {
            MaintenanceViewModel.GetInstance = null;
        }
        public void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WindowsOpen = false;
            ParametersWindowTabSelected = -1;
            _instance = null;
        }

        private void TabSelected(object sender, SelectionChangedEventArgs e)
        {
            ParametersWindowTabSelected = ((System.Windows.Controls.Primitives.Selector)sender).SelectedIndex;
        }
        private new void MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }

        private void FolderTB_ScrolltoEnd(object sender, EventArgs e)
        {
            ((TextBox)sender).ScrollToHorizontalOffset(((TextBox)sender).ExtentWidth);
        }
    }
}
