using MotorController.Common;
using MotorController.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MotorController.Views
{
    /// <summary>
    /// Interaction logic for Wizard.xaml
    /// </summary>
    public partial class Wizard : Window
    {
        public static bool WindowsOpen = false;

        private static readonly object Synlock = new object();
        private static Wizard _instance;
        public static Wizard GetInstance
        {
            get
            {
                lock (Synlock)
                {
                    if (_instance != null)
                        return _instance;
                    _instance = new Wizard();
                    var vaultHwnd = Process.GetCurrentProcess().MainWindowHandle; //Vault window handle
                    new WindowInteropHelper(_instance) { Owner = vaultHwnd };
                    return _instance;
                }
            }
        }

        public Wizard()
        {
            InitializeComponent();
            this.DataContext = WizardWindowViewModel.GetInstance;// WizardWindowViewModel();
            Wizard.WindowsOpen = true;
        }

        public void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WizardWindowViewModel.GetInstance.saveWizardParams();
            WindowsOpen = false;
            _instance = null;
        }

        ~Wizard()
        {
        }
    }
}
