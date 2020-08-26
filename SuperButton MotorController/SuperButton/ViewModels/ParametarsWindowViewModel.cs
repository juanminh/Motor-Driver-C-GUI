using System.Collections.ObjectModel;
using MotorController.CommandsDB;
using System.Windows.Input;
using MotorController.Models;
using System.Windows.Controls;
using System;
using System.Threading;
using MotorController.Models.DriverBlock;
using System.Diagnostics;
using MotorController.ViewModels;

namespace MotorController.ViewModels
{
    enum eTab // = tabIndex - 1
    {
        CONTROL = 0,
        FEED_BACKS = 1,
        PID = 2,
        FILTER = 3,
        DEVICE = 4,
        I_O = 5,
        CALIBRATION = 6,
        BODE = 7,
        MAINTENANCE = 8,
        DEBUG = 9
    };
    internal class ParametarsWindowViewModel : ViewModelBase
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
        //private LoadParamsViewModel _loadParamsViewModel;

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
            set
            {
                _instance = value;
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
            //_loadParamsViewModel = LoadParamsViewModel.GetInstance;
        }
        ~ParametarsWindowViewModel() { }
        public ObservableCollection<object> ControlList
        {
            get
            {
                return Commands.GetInstance.EnumCommandsListbySubGroup["Control"];
            }
        }

        public ObservableCollection<object> MotorlList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["Motor"];
            }

        }
        private ObservableCollection<object> _motorLimitlList;
        public ObservableCollection<object> MotorLimitlList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["Motion Limit"];
            }
            set
            {
                _motorLimitlList = value;
                OnPropertyChanged();
            }

        }
        private ObservableCollection<object> _currentLimitList;
        public ObservableCollection<object> CurrentLimitList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["CurrentLimit List"];
            }
            set
            {
                _currentLimitList = value;
                OnPropertyChanged();
            }

        }
        public ObservableCollection<object> DigitalFeedbackFeedBackList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["Digital"];
            }

        }
        public ObservableCollection<object> AnalogFeedbackFeedBackList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["Analog"];
            }

        }

     
        private ObservableCollection<object> _baudrateList;
        public ObservableCollection<object> DeviceSerialList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["DeviceSerial"];
            }

        }
        public ObservableCollection<object> BaudrateList
        {

            get
            {
                return Commands.GetInstance.EnumCommandsListbySubGroup["BaudrateList"];
            }
            set
            {
                _baudrateList = value;
                OnPropertyChanged();
            }

        }

        public virtual ICommand TestEnumChange
        {
            get
            {
                return new RelayCommand(EnumChange, CheckValue);
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
        //public LoadParamsViewModel LoadParamsViewModel
        //{
        //    get { return _loadParamsViewModel; }
        //}
        public DebugViewModel DebugViewModel
        {
            get { return _debugViewModel; }
        }
        public BodeViewModel BodeViewModel
        {
            get { return _bodeViewModel; }
        }
        private bool CheckValue()
        {
            return true;
        }

        private void EnumChange()
        {
        }
    }
}
