using MotorController.Common;
using MotorController.Models;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace MotorController.ViewModels
{
    public partial class BodeWindowViewModel : Window
    {
        private static readonly object Synlock = new object();

        private static BodeWindowViewModel _instance;
        public static BodeWindowViewModel GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new BodeWindowViewModel();
                    return _instance;
                }
            }
        }
        private BodeViewModel _bodeWindowDataContext;
        public BodeViewModel BodeWindowDataContext
        {
            get { return _bodeWindowDataContext; }
        }
        public BodeWindowViewModel()
        {
            _bodeWindowDataContext = BodeViewModel.GetInstance;
        }
        public virtual ICommand BodeWindowLoaded
        {
            get
            {
                return new RelayCommand(BodeWindowLoaded_Func);
            }
        }
        public ICommand BodeWindowClosed
        {
            get
            {
                return new RelayCommand(BodeWindowClosed_Func);
            }
        }
        private void BodeWindowLoaded_Func()
        {
            DebugViewModel.updateList = true;
            LeftPanelViewModel.GetInstance.cancelRefresh = new CancellationToken(true);
            RefreshManager.GetInstance.BuildGenericCommandsList_Func();
        }
        private void BodeWindowClosed_Func()
        {
            LeftPanelViewModel.GetInstance._bode_window.Visibility = Visibility.Hidden;
            LeftPanelViewModel.GetInstance.cancelRefresh = new CancellationToken(true);
            DebugViewModel.updateList = true;
            RefreshManager.GetInstance.BuildGenericCommandsList_Func();
        }
        public new ICommand MouseDownEvent
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
