using System;
using System.Threading.Tasks;
using System.Windows;
using Abt.Controls.SciChart;
using MotorController.Models.DriverBlock;
using MotorController.Views;
using BaseViewModel = MotorController.Common.BaseViewModel;
using MotorController.Helpers;
using System.Diagnostics;
//using SharpDX.Design;

namespace MotorController.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public OscilloscopeViewModel OscilloscopeViewModel
        {
            get { return oscilloscopeViewModel; }
            set {; }
        }
        
        public ActionCommand MainWindowResized { get { return new ActionCommand(mainWindowResized); } }

        private double maxHeight = 240;
        void mainWindowResized()
        {
            maxHeight = (float)Application.Current.MainWindow.ActualHeight - 101;
        }

        //private WindowState _windowState = WindowState.Normal;

        private WindowState _windowState = WindowState.Maximized;

        public WindowState WindowState
        {
            get { return _windowState; }
            set {; }
        }
#region Actions
        public ActionCommand SetAutoConnectActionCommandCommand
        {
            get { return new ActionCommand(AutoConnectCommand); }
        }

#endregion

        //Data content binding between views of panels within main window
        //and their view models, write binding in XAMLs also

        private BottomPanelViewModel bottomPanelViewModel = BottomPanelViewModel.GetInstance;

        public BottomPanelViewModel BPcontent
        {
            get { return bottomPanelViewModel; }
            set { }
        }

        private LeftPanelViewModel leftPanelViewModel = LeftPanelViewModel.GetInstance;
        private OscilloscopeViewModel oscilloscopeViewModel = OscilloscopeViewModel.GetInstance;

        public LeftPanelViewModel LPcontent
        {
            get { return leftPanelViewModel; }
            set { }
        }

        public MainViewModel()
        {
            leftPanelViewModel.ConnectButtonContent = "Connect";
            leftPanelViewModel.ConnectTextBoxContent = "Not Connected";

            leftPanelViewModel.ComToolTipText = "Pls Choose CoM";
            Rs232Interface.GetInstance.Driver2Mainmodel += SincronizationPos;
        }

        ~MainViewModel()
        {
            
        }

        private void SincronizationPos(object sender, Rs232InterfaceEventArgs e)
        {
            leftPanelViewModel.ConnectButtonContent = e.ConnecteButtonLabel;
            leftPanelViewModel.ComToolTipText = "Allready Connected";
        }

        public void AutoConnectCommand()
        {
            Rs232Interface comRs232Interface = Rs232Interface.GetInstance;
            Task task = new Task(new Action(comRs232Interface.AutoConnect));
            task.Start();
        }
    }
}