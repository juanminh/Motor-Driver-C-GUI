using MotorController.Models;
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
    }
}
