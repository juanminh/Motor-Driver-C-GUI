using System.Collections.ObjectModel;
using MotorController.Common;
using System.Windows.Input;
using MotorController.Models;
using System.Windows.Controls;
using System;
using System.Threading;
using MotorController.Models.DriverBlock;
using System.Diagnostics;
using MotorController.ViewModels;
using MotorController.Views;
using System.ComponentModel;
using System.Windows;
using MotorController.Helpers;

namespace MotorController.ViewModels
{
    enum eTab
    {
        CONTROL = 1,
        FEED_BACKS = 2,
        PID = 3,
        FILTER = 4,
        DEVICE = 5,
        I_O = 6,
        CALIBRATION = 7,
        BODE = 8,
        MAINTENANCE = 9,
        DEBUG = 10
    };
    internal class ParametarsWindowViewModel : Window//,  ViewModelBase
    {
        private CalibrationViewModel _calibrationViewModel;
        private MotionViewModel _motionViewModel;
        private MaintenanceViewModel _maintenanceViewModel;
        private FeedBackViewModel _feedBackViewModel;
        private DebugViewModel _debugViewModel;
        private IOViewModel _ioViewModel;
        private BodeViewModel _bodeViewModel;
        private FilterViewModel _filterViewModel;
        private PIDViewModel _pidViewModel;

        private static readonly object Synlock = new object();
        public static readonly object PlotBodeListLock = new object();             //Singletone variable

        private static ParametarsWindowViewModel _instance;
        public static ParametarsWindowViewModel GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new ParametarsWindowViewModel();
                    return _instance;
                }
            }
        }
        public ParametarsWindowViewModel()
        {
            _calibrationViewModel = CalibrationViewModel.GetInstance;
            _motionViewModel = MotionViewModel.GetInstance;
            _maintenanceViewModel = MaintenanceViewModel.GetInstance;
            _filterViewModel = FilterViewModel.GetInstance;
            _feedBackViewModel = FeedBackViewModel.GetInstance;
            _debugViewModel = DebugViewModel.GetInstance;
            _ioViewModel = IOViewModel.GetInstance;
            _bodeViewModel = BodeViewModel.GetInstance;
            _pidViewModel = PIDViewModel.GetInstance;
        }
        ~ParametarsWindowViewModel() { }
        public ObservableCollection<object> ControlList
        {
            get
            {
                return Commands.GetInstance.GenericCommandsGroup["Control"];
            }
        }

        public ObservableCollection<object> MotorlList
        {
            get
            {
                return Commands.GetInstance.GenericCommandsGroup["Motor"];
            }
        }
        private ObservableCollection<object> _motorLimitlList;
        public ObservableCollection<object> MotorLimitlList
        {
            get
            {
                return Commands.GetInstance.GenericCommandsGroup["Motion Limit"];
            }
        }
        private ObservableCollection<object> _currentLimitList;
        public ObservableCollection<object> CurrentLimitList
        {
            get
            {
                return Commands.GetInstance.GenericCommandsGroup["CurrentLimit List"];
            }
        }
        public ObservableCollection<object> DigitalFeedbackFeedBackList
        {
            get
            {
                return Commands.GetInstance.GenericCommandsGroup["Digital"];
            }
        }
        public ObservableCollection<object> AnalogFeedbackFeedBackList
        {
            get
            {
                return Commands.GetInstance.GenericCommandsGroup["Analog"];
            }
        }
        private ObservableCollection<object> _baudrateList;
        public ObservableCollection<object> DeviceSerialList
        {
            get
            {
                return Commands.GetInstance.GenericCommandsGroup["DeviceSerial"];
            }
        }
        public ObservableCollection<object> BaudrateList
        {
            get
            {
                return Commands.GetInstance.GenericCommandsGroup["BaudrateList"];
            }
        }

        public CalibrationViewModel CalibrationViewModel
        {
            get { return _calibrationViewModel; }
        }

        public MotionViewModel MotionViewModel
        {
            get { return _motionViewModel; }
        }
        public IOViewModel IOViewModel
        {
            get { return _ioViewModel; }
        }

        public MaintenanceViewModel MaintenanceViewModel
        {
            get { return _maintenanceViewModel; }
        }

        public FeedBackViewModel FeedBackViewModel
        {
            get { return _feedBackViewModel; }
        }
        public PIDViewModel PIDViewModel
        {
            get { return _pidViewModel; }
        }
        public FilterViewModel FilterViewModel
        {
            get { return _filterViewModel; }
        }
        public DebugViewModel DebugViewModel
        {
            get { return _debugViewModel; }
        }
        public BodeViewModel BodeViewModel
        {
            get { return _bodeViewModel; }
        }
       
        public virtual ICommand ParametersWindowTabSelection
        {
            get
            {
                return new RelayCommand(RebuildGenericCommandsList);
            }
        }
        public virtual ICommand ParametersWindowLoaded
        {
            get
            {
                return new RelayCommand(ParametersWindowLoaded_Func);
            }
        }
        public /*virtual*/ ICommand ParametersWindowClosed
        {
            get
            {
                return new RelayCommand(ParametersWindowClosed_Func);
            }
        }
        private void RebuildGenericCommandsList(object sender)
        {
            var _tabItem = sender as TabItem;
            
            Debug.WriteLine("RebuildGenericCommandsList");
            int _ind = (int)Enum.Parse(typeof(eTab), ((TabItem)((TabControl)sender).SelectedItem).Header.ToString().ToUpper().Replace(" ", "_").Replace("/", "_"));

            if(TabControlIndex == _old_tab)
                return;

            LeftPanelViewModel.GetInstance.cancelRefresh = new CancellationToken(true);
            RefreshManager.GetInstance.BuildGenericCommandsList_Func();
        }
        private void ParametersWindowLoaded_Func()
        {
            Debug.WriteLine("ParametersWindowLoaded_Func");

            TabControlIndex = 1;
            LeftPanelViewModel.GetInstance.cancelRefresh = new CancellationToken(true);
            RefreshManager.GetInstance.BuildGenericCommandsList_Func();
        }
        private void ParametersWindowClosed_Func()
        {
            TabControlIndex = -1;
            Debug.WriteLine("ParametersWindowClosed_Func");

            LeftPanelViewModel.GetInstance._param_window.Visibility = Visibility.Hidden;

            LeftPanelViewModel.GetInstance.cancelRefresh = new CancellationToken(true);
            RefreshManager.GetInstance.BuildGenericCommandsList_Func();
        }
        public static int TabControlIndex = -1;
        //public int TabControlIndex
        //{
        //    get { return _tabControlIndex; }
        //    set { _tabControlIndex = value; OnPropertyChanged(); }
        //}
        private TabItem _tabControlItem;
        private int _old_tab = -1;
        public TabItem TabControlItem
        {
            get { return _tabControlItem; }
            set { _tabControlItem = value;
                _old_tab = TabControlIndex;
                TabControlIndex = value.TabIndex;
                //OnPropertyChanged(); 
            }
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
