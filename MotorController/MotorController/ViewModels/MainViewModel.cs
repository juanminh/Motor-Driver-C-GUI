using System;
using System.Threading.Tasks;
using System.Windows;
using Abt.Controls.SciChart;
using MotorController.Models.DriverBlock;
using MotorController.Views;
using BaseViewModel = MotorController.Common.BaseViewModel;
using MotorController.Helpers;
using System.Diagnostics;
using System.Windows.Input;
using MotorController.Models;
using System.Windows.Controls;
using MotorController.Common;
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

        private WindowState _windowState = Consts._build == Consts.eBuild.DEBUG ? WindowState.Normal : WindowState.Maximized;

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
        public ICommand MouseDownEvent
        {
            get
            {
                return new RelayCommand(MouseDownEventFunc);
            }
        }
        private void MouseDownEventFunc(object sender)
        {
            var _tb = sender as UIElement;
            _tb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

            Keyboard.ClearFocus();
            foreach(var list in Commands.GetInstance.GenericCommandsList)
            {
                try
                {
                    if(list.Value.GetType().Name == "DataViewModel")
                    {
                        ((DataViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(Convert.ToInt16(((DataViewModel)list.Value).CommandId), Convert.ToInt16(((DataViewModel)list.Value).CommandSubId))]).IsSelected = false;
                        ((DataViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(Convert.ToInt16(((DataViewModel)list.Value).CommandId), Convert.ToInt16(((DataViewModel)list.Value).CommandSubId))]).Background = DataViewModel._background;
                    }
                }
                catch(Exception)
                {

                }
            }
        }
    }
}