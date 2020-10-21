using Abt.Controls.SciChart;
using MotorController.Helpers;
using MotorController.Models.DriverBlock;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using System.Windows.Input;
using MotorController.Models;
using System.Windows;
using MotorController.Common;

namespace MotorController.ViewModels
{
    enum eMODE
    {
        [Description("Wizard configuration option: 0 - short mode; 1 - adavanced configuration mode")]
        Normal = 0,
        Advanced = 1
    };
    enum eMotorType
    {
        [Description("Motor type")]
        DC_Brushed = 0,
        Three_Phase_Brushless = 1
    };
    enum eClaFdb
    {
        [Description("Feedbacks type")]
        Cla_Fdb_None = 0x0000,
        Cla_Fdb_Hall = 0x0001,
        Cla_Fdb_Motor = 0x0002,
        Cla_Fdb_Ext = 0x0003,
        Cla_Fdb_End = 0x04
    };

    enum eEncSel
    {
        [Description("Encoders type")]
        Enc_Fdb_None = 0x000,
        Enc_Fdb_Inc1 = 0x0001,
        Enc_Fdb_Inc_Sin_Cos = 0x0002,
        Enc_Fdb_Abs_Sin_Cos = 0x0003,
        Enc_Fdb_Inc2 = 0x0004,
        Enc_Fdb_Ssi = 0x0005,
        Enc_Fdb_Resolver = 0x0006,
        Enc_Fdb_Com = 0x07,
        Enc_Fdb_End = 0x08
    };

    enum eComutationSource
    {
        [Description("Comutation source type")]
        Cmtn_Brushed = 0,
        Cmtn_Hall = 1,
        Cmtn_Hall_Inc_Enc = 2,
        Cmtn_Abs_Enc = 3,
        Cmtn_Forced_Rotate = 4,
    };
    enum eHall
    {
        [Description("Hall Status")]
        Disable = 0,
        Enable = 1
    };

    enum eDriveMode
    {
        Current_Control = 0,
        Speed_Control = 1,
        Position_Control = 2
    };
    enum eCommandSource
    {
        Digital = 0,
        Analog = 1
    };

    public partial class WizardWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if(handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
            if(propertyName != "ValidOperations")
                VerifyValidOperation();
        }
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
        public Dictionary<Tuple<int, int>, CalibrationWizardViewModel> CalibrationWizardList = new Dictionary<Tuple<int, int>, CalibrationWizardViewModel>();
        public Dictionary<string, ObservableCollection<object>> CalibrationWizardListbySubGroup = new Dictionary<string, ObservableCollection<object>>();
        public Dictionary<Tuple<int, int>, DataViewModel> OperationList = new Dictionary<Tuple<int, int>, DataViewModel>();
        public static Dictionary<Tuple<int, int>, DataViewModel> operation_echo = new Dictionary<Tuple<int, int>, DataViewModel>();

        //public bool _save_cmd_success = false;
        #region FIELDS
        private static readonly object Synlock = new object();
        private static WizardWindowViewModel _instance;
        #endregion FIELDS

        public static WizardWindowViewModel GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new WizardWindowViewModel();
                    return _instance;
                }
            }
            set
            {
                _instance = value;
            }
        }
        public WizardWindowViewModel()
        {
            if(_instance == null)
            {
                CalibrationWizardListbySubGroup.Add("CalibrationList", new ObservableCollection<object>());
                BuildCalibrationWizardList();
                readWizardParams();
            }
            else
            {
                readWizardParams();
            }
            //Start = new RelayCommand(StartButton);
        }
        ~WizardWindowViewModel() { }
        #region Motor_Parameter
        private int _motorType = 1;
        public int MotorType
        {
            get { return _motorType; }
            set
            {
                _motorType = value;
                BuildCalibrationWizardList();
                OnPropertyChanged("MotorType");
            }
        }
        private string _polePaire = "3";
        public string PolePair
        {
            get { return _polePaire; }
            set
            {
                _polePaire = value;
                OnPropertyChanged("PolePair");
            }
        }
        private string _continuousCurrent = "1";
        public string ContinuousCurrent
        {
            get { return _continuousCurrent; }
            set { _continuousCurrent = value; OnPropertyChanged("ContinuousCurrent"); }
        }
        private string _maxSpeed = "5000";
        public string MaxSpeed
        {
            get { return _maxSpeed; }
            set { _maxSpeed = value; OnPropertyChanged("MaxSpeed"); }
        }
        private int _hallEnDis = (int)eHall.Enable;
        public int HallEnDis
        {
            get { return _hallEnDis; }
            set
            {
                _hallEnDis = value;
                BuildCalibrationWizardList();
                OnPropertyChanged("HallEnDis");
            }
        }
        private int _encoderFeedback = (int)eEncSel.Enc_Fdb_Ssi;
        public int EncoderFeedback
        {
            get { return _encoderFeedback; }
            set
            {
                _encoderFeedback = value;
                switch(value)
                {
                    case (int)eEncSel.Enc_Fdb_None:
                        EncoderResolution = "1000";
                        break;
                    case (int)eEncSel.Enc_Fdb_Abs_Sin_Cos:
                        EncoderResolution = "4096";
                        break;
                    default:
                        EncoderResolution = _encoderResolution;
                        break;
                }
                if(value == 0)
                    EncoderFeedbackExist = false;
                else
                    EncoderFeedbackExist = true;
                BuildCalibrationWizardList();
                OnPropertyChanged("EncoderFeedback");
            }
        }
        private bool _encoderFeedbackExist = false;
        public bool EncoderFeedbackExist
        {
            get { return _encoderFeedbackExist; }
            set
            {
                _encoderFeedbackExist = value;
                OnPropertyChanged("EncoderFeedbackExist");
            }
        }
        private string _encoderResolution = "18";//"1000";
        public string EncoderResolution
        {
            get { return _encoderResolution; }
            set
            {
                if(String.IsNullOrEmpty(value) || String.IsNullOrWhiteSpace(value))
                    return;
                if(EncoderFeedback == (int)eEncSel.Enc_Fdb_Ssi && Convert.ToInt32(value) < 32)
                    _encoderResolution = value;
                else if(EncoderFeedback != (int)eEncSel.Enc_Fdb_Ssi)
                    _encoderResolution = value;
                else
                    return;
                EncoderResolutionCounts = (Convert.ToInt64(_encoderResolution) * 4).ToString();
                OnPropertyChanged("EncoderResolution");
            }
        }

        private string _encoderResolutionCounts = "18";//"1000";
        public string EncoderResolutionCounts
        {
            get { return (Convert.ToInt64(EncoderResolution) * 4).ToString(); }
            set
            {
                _encoderResolutionCounts = value;
                OnPropertyChanged("EncoderResolutionCounts");
            }
        }
        #endregion Motor_Parameter
        #region Calibration
        public int send_operation_count = 0;
        public static bool calibration_is_in_process = false;
        private int _validOperations = RoundBoolLed.FAILED;
        public int ValidOperations
        {
            get { return _validOperations; }
            set
            {
                _validOperations = value;
                OnPropertyChanged("ValidOperations");
            }
        }
        private bool _startEnable = true;
        public bool StartEnable
        {
            get { return _startEnable; }
            set
            {
                _startEnable = value;
                OnPropertyChanged("StartEnable");
            }
        }
        private ObservableCollection<object> _calibList;
        public ObservableCollection<object> CalibList
        {
            get
            {
                return GetInstance.CalibrationWizardListbySubGroup["CalibrationList"];
            }
            set
            {
                _calibList = value;
                OnPropertyChanged();
            }
        }
        private void BuildCalibrationWizardList()
        {
            if(_instance != null)
            {
                GetInstance.CalibrationWizardList.Clear();
                GetInstance.CalibrationWizardListbySubGroup["CalibrationList"].Clear();
            }
            else
            {
                CalibrationWizardList.Clear();
                CalibrationWizardListbySubGroup["CalibrationList"].Clear();
            }
            var names = new[]
            {
                "Updating Parameters", "PI Current Loop", "Hall Mapping", "Feedback Direction", "Electrical Angle", "PI Speed Loop", "PI Position Loop"
            };
            var SubID = new[]
            {
                "-1", "4", "6", "8", "14", "10", "12"
            };
            int[] CalibTimeout = new int[] { 10, 10, 70, 15, 60, 60, 60 };
            Dictionary<string, string> calibOperation = new Dictionary<string, string>();
            for(int i = 0; i < names.Length; i++)
                calibOperation.Add(names[i], SubID[i]);

            if(MotorType == 0 || HallEnDis == (int)eHall.Disable)
                calibOperation.Remove("Hall Mapping");
            if(!(EncoderFeedback == (int)eEncSel.Enc_Fdb_Ssi || EncoderFeedback == (int)eEncSel.Enc_Fdb_Abs_Sin_Cos))
                calibOperation.Remove("Electrical Angle");
            if(EncoderFeedback == (int)eEncSel.Enc_Fdb_None)
                calibOperation.Remove("Feedback Direction");

            CalibrationWizardViewModel calibElement;
            for(int i = 0; i < calibOperation.Count; i++)
            {
                calibElement = new CalibrationWizardViewModel
                {
                    AdvanceMode_Calibration = CalibrationAdvancedMode,
                    CalibrationEnabled = i == 0 ? false : true,
                    CalibrationPerform = CalibrationAdvancedMode ? i == 0 ? true : false : true,
                    CalibrationName = calibOperation.ElementAt(i).Key,
                    CalibStatus = 0,
                    CommandId = "6",
                    CommandSubId = calibOperation.ElementAt(i).Value,
                    CalibTimeout = CalibTimeout[i],
                    isWizard = true
                };
                if(_instance != null)
                {
                    GetInstance.CalibrationWizardList.Add(new Tuple<int, int>(6, Convert.ToInt32(calibElement.CommandSubId)), calibElement);
                    GetInstance.CalibrationWizardListbySubGroup["CalibrationList"].Add(calibElement);
                }
                else
                {
                    CalibrationWizardList.Add(new Tuple<int, int>(6, Convert.ToInt32(calibElement.CommandSubId)), calibElement);
                    CalibrationWizardListbySubGroup["CalibrationList"].Add(calibElement);
                }
            }
        }
        public ActionCommand Start { get { return new ActionCommand(StartButton); } }
        CancellationToken cancellationTokenCalib;
        private async void StartButton()
        {
            if(!LeftPanelViewModel._app_running)
                return;
            StartEnable = false;
            calibration_is_in_process = true;

            cancellationTokenCalib = new CancellationToken(false);

            Thread.Sleep(10);
            try
            {
                await Task.Run(() => StartCalib());
            }
            catch
            {
                AbortCalib();
            }
        }
        private async void StartButtonStop()
        {
            //if(!calibration_is_in_process || !StartEnable)
            //    return;

            await Task.Run(() =>
            {
                for(int i = 0; i < GetInstance.CalibrationWizardList.Count; i++)
                {
                    GetInstance.CalibrationWizardList.ElementAt(i).Value.CalibrationEnabled = true;
                    if(GetInstance.CalibrationWizardList.ElementAt(i).Value.CalibStatus == RoundBoolLed.RUNNING)
                        updateCalibrationStatus(new Tuple<int, int>(Convert.ToInt32(GetInstance.CalibrationWizardList.ElementAt(i).Value.CommandId), Convert.ToInt32(GetInstance.CalibrationWizardList.ElementAt(i).Value.CommandSubId)), RoundBoolLed.STOPPED.ToString());
                }

                StartEnable = true;
                calibration_is_in_process = false;
                Thread.Sleep(10);
                DebugViewModel.updateList = true;
                RefreshManager.GetInstance.BuildGenericCommandsList_Func();
            });
        }
        private void StartCalib()
        {
            if(((UC_ToggleSwitchViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(1, 0)]).IsChecked)
            {
                EventRiser.Instance.RiseEevent(string.Format($"Motor must be off."));
                cancellationTokenCalib = new CancellationToken(true);
                StartButtonStop();
                return;
            }
            #region InitVariables
            DataViewModel operation = new DataViewModel();
            Int32 commandId = 0, commandSubId = 0;
            GetInstance.Count = 0;
            #endregion InitVariables

            GetInstance.CalibrationWizardList[new Tuple<int, int>(6, -1)].CalibStatus = RoundBoolLed.RUNNING;
            Thread.Sleep(50);

            for(int i = 1; i < GetInstance.CalibrationWizardList.Count; i++)
                GetInstance.CalibrationWizardList.ElementAt(i).Value.CalibrationEnabled = false;

            GetInstance.OperationList.Clear();
            for(int i = 1; i < GetInstance.CalibrationWizardList.Count; i++)
            {
                GetInstance.CalibrationWizardList.ElementAt(i).Value.CalibStatus = RoundBoolLed.IDLE;

                operation = new DataViewModel { CommandName = GetInstance.CalibrationWizardList.ElementAt(i).Value.CalibrationName, CommandId = "6", CommandSubId = GetInstance.CalibrationWizardList.ElementAt(i).Value.CommandSubId, IsFloat = false, CommandValue = "1" };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                if(GetInstance.CalibrationWizardList.ElementAt(i).Value.CalibrationPerform)
                    GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
            }

            GetInstance.OperationList.Clear();

            #region BuildOperationList
            string id_fdbck_cmd_temp = "", id_ext_fdbck_cmd_temp = "", comutation_source = "", index_analog_command_value = "";
            long max_speed, min_speed;

            /*Commutation select*/
            if(MotorType == (int)eMotorType.DC_Brushed)
                comutation_source = ((int)eComutationSource.Cmtn_Brushed).ToString();
            else
            {
                switch(EncoderFeedback) // EncoderFeedback => Motor Feedback
                {
                    case (int)eEncSel.Enc_Fdb_Inc1:
                    case (int)eEncSel.Enc_Fdb_Inc_Sin_Cos:
                    case (int)eEncSel.Enc_Fdb_Inc2:
                        if(HallEnDis == (int)eHall.Enable) // if Hall Enable
                            comutation_source = ((int)eComutationSource.Cmtn_Hall_Inc_Enc).ToString();
                        else
                        {
                            return; // Cannot use BL motor without hall or absolute encoder
                        }
                        break;
                    case (int)eEncSel.Enc_Fdb_Abs_Sin_Cos:
                    case (int)eEncSel.Enc_Fdb_Ssi:
                        comutation_source = ((int)eComutationSource.Cmtn_Abs_Enc).ToString();
                        break;
                    case (int)eEncSel.Enc_Fdb_None:
                        if(HallEnDis == (int)eHall.Enable) // if Hall Enable
                            comutation_source = ((int)eComutationSource.Cmtn_Hall).ToString();
                        else
                        {
                            return; // Cannot use BL motor without hall or absolute encoder
                        }
                        break;
                    default:
                    case (int)eEncSel.Enc_Fdb_Resolver:
                    case (int)eEncSel.Enc_Fdb_Com:
                        return;// don't start when start clicked
                }
            }
            string EncoderBits = "0", EncoderResolutionTemp = "0";
            EncoderResolutionTemp = EncoderResolution;
            /*set resolution for selected encoder*/
            switch(EncoderFeedback) // EncoderFeedback => Motor Feedback
            {
                case (int)eEncSel.Enc_Fdb_Inc1:
                case (int)eEncSel.Enc_Fdb_Inc_Sin_Cos:
                    id_fdbck_cmd_temp = "71";
                    break;
                case (int)eEncSel.Enc_Fdb_Inc2:
                    id_fdbck_cmd_temp = "72";
                    break;
                case (int)eEncSel.Enc_Fdb_Ssi:
                    id_fdbck_cmd_temp = "73";
                    EncoderBits = EncoderResolution;
                    EncoderResolutionTemp = (Math.Pow(2, Convert.ToInt32(EncoderResolution))).ToString();
                    break;
                default:
                case (int)eEncSel.Enc_Fdb_None:
                case (int)eEncSel.Enc_Fdb_Abs_Sin_Cos:
                case (int)eEncSel.Enc_Fdb_Resolver:
                case (int)eEncSel.Enc_Fdb_Com:
                    id_fdbck_cmd_temp = "";// don't update
                    break;
            }

            if(Convert.ToInt32(MaxSpeed) > 0)
            {
                max_speed = (Convert.ToInt32(MaxSpeed) * Convert.ToInt32(EncoderResolutionTemp) / 60);
                min_speed = -max_speed;
            }
            else
                return;// don't start when start clicked

            if(LoadDefault) //change to LoadDefault
            {
                operation = new DataViewModel { CommandName = "Load Default", CommandId = "63", CommandSubId = "1", IsFloat = false, CommandValue = "1" };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
            }

            operation = new DataViewModel { CommandName = "Electrical Commutation Type", CommandId = "50", CommandSubId = "2", IsFloat = false, CommandValue = comutation_source };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            operation = new DataViewModel { CommandName = "Encoder Type", CommandId = "50", CommandSubId = "3", IsFloat = false, CommandValue = EncoderFeedback.ToString() };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            /*update "Pole pair" parameter*/
            if(MotorType == (int)eMotorType.Three_Phase_Brushless)
            {
                operation = new DataViewModel { CommandName = "Pole Pair", CommandId = "51", CommandSubId = "1", IsFloat = false, CommandValue = PolePair };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
            }

            /*Update "Max Speed" parameter*/
            operation = new DataViewModel { CommandName = "Max Speed", CommandId = "53", CommandSubId = "1", IsFloat = false, CommandValue = max_speed.ToString() };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            /*update "Min Speed" parameter*/
            operation = new DataViewModel { CommandName = "Min Speed", CommandId = "53", CommandSubId = "2", IsFloat = false, CommandValue = min_speed.ToString() };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            /*update "Hall Enable" parameter*/
            operation = new DataViewModel { CommandName = "Hall", CommandId = "70", CommandSubId = "1", IsFloat = false, CommandValue = HallEnDis.ToString() };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            /*update "Roll High", "Roll Low" parameter when Hall Enable*/
            if(HallEnDis == (int)eHall.Enable)
            {
                operation = new DataViewModel { CommandName = "Roll High", CommandId = "70", CommandSubId = "2", IsFloat = false, CommandValue = (Convert.ToInt32(EncoderResolutionTemp) * 1000 - 1).ToString() };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

                operation = new DataViewModel { CommandName = "Roll Low", CommandId = "70", CommandSubId = "3", IsFloat = false, CommandValue = "0" };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
            }


            /*update SSI "Packet len" parameter*/
            if(EncoderFeedback == (int)eEncSel.Enc_Fdb_Ssi) //Motor feedback
            {
                operation = new DataViewModel { CommandName = "PacketLenght", CommandId = "73", CommandSubId = "8", IsFloat = false, CommandValue = (Convert.ToInt32(EncoderBits)).ToString() };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            }

            /*update  "Motor Encoder Resolution", "Roll High", "Roll Low" parameter*/
            if(id_fdbck_cmd_temp != "")
            {
                if(!(EncoderFeedback == (int)eEncSel.Enc_Fdb_Ssi))
                {
                    operation = new DataViewModel { CommandName = "Resolution", CommandId = id_fdbck_cmd_temp, CommandSubId = "5", IsFloat = false, CommandValue = EncoderResolutionTemp };
                    Int32.TryParse(operation.CommandId, out commandId);
                    Int32.TryParse(operation.CommandSubId, out commandSubId);
                    GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
                }

                operation = new DataViewModel { CommandName = "Roll High", CommandId = id_fdbck_cmd_temp, CommandSubId = "2", IsFloat = false, CommandValue = (Convert.ToInt32(EncoderResolutionTemp) * 1000 - 1).ToString() };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

                operation = new DataViewModel { CommandName = "Roll Low", CommandId = id_fdbck_cmd_temp, CommandSubId = "3", IsFloat = false, CommandValue = "0" };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
            }

            /*update  "Continuous Current" parameter*/
            operation = new DataViewModel { CommandName = "Continuous Current", CommandId = "52", CommandSubId = "1", IsFloat = true, CommandValue = ContinuousCurrent };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            /*update  "Peak Current" parameter*/
            operation = new DataViewModel { CommandName = "Peak Current", CommandId = "52", CommandSubId = "2", IsFloat = true, CommandValue = ContinuousCurrent };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            /*update  "External Encoder Type" parameter*/
            operation = new DataViewModel { CommandName = "External Encoder Type", CommandId = "50", CommandSubId = "4", IsFloat = false, CommandValue = ExternalEncoder.ToString() };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            /*update  "External Encoder Resolution" parameter*/
            switch(ExternalEncoder)
            {
                case (int)eEncSel.Enc_Fdb_Inc1:
                    id_ext_fdbck_cmd_temp = "71";
                    break;
                case (int)eEncSel.Enc_Fdb_Inc_Sin_Cos:
                    id_ext_fdbck_cmd_temp = "71";
                    break;
                case (int)eEncSel.Enc_Fdb_Abs_Sin_Cos:
                    id_ext_fdbck_cmd_temp = "71";
                    break;
                case (int)eEncSel.Enc_Fdb_Inc2:
                    id_ext_fdbck_cmd_temp = "72";
                    break;
                case (int)eEncSel.Enc_Fdb_Ssi:
                    id_ext_fdbck_cmd_temp = "73";
                    EncoderBits = ExternalResolution;
                    EncoderResolutionTemp = (Math.Pow(2, Convert.ToInt32(ExternalResolution))).ToString();
                    break;
                default:
                    id_ext_fdbck_cmd_temp = "";
                    break;
            }
            if(id_ext_fdbck_cmd_temp != "")
            {
                //EncoderResolutionTemp = ExternalResolution;

                operation = new DataViewModel { CommandName = "External Encoder Resolution", CommandId = id_ext_fdbck_cmd_temp, CommandSubId = "5", IsFloat = false, CommandValue = EncoderResolutionTemp };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

                operation = new DataViewModel { CommandName = "Roll High", CommandId = id_ext_fdbck_cmd_temp, CommandSubId = "2", IsFloat = false, CommandValue = (Convert.ToInt32(EncoderResolutionTemp) - 1).ToString() };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

                operation = new DataViewModel { CommandName = "Roll Low", CommandId = id_ext_fdbck_cmd_temp, CommandSubId = "3", IsFloat = false, CommandValue = "0" };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

                /*update SSI "Packet len" parameter*/
                if(ExternalEncoder == (int)eEncSel.Enc_Fdb_Ssi) //Motor feedback
                {
                    operation = new DataViewModel { CommandName = "PacketLenght", CommandId = "73", CommandSubId = "8", IsFloat = false, CommandValue = (Convert.ToInt32(EncoderBits)).ToString() };
                    Int32.TryParse(operation.CommandId, out commandId);
                    Int32.TryParse(operation.CommandSubId, out commandSubId);
                    GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
                }

            }

            /*update  "PI Speed Loop Feedback" parameter*/
            if(id_fdbck_cmd_temp == "")
            {
                if(id_ext_fdbck_cmd_temp != "")
                    ExternalSpeedLoop = 2; //external encoder
                else if(MotorType == (int)eMotorType.Three_Phase_Brushless)
                    ExternalSpeedLoop = 0; //use hall
                else
                    ExternalSpeedLoop = -1; //  1. remove PI Speed/Position/direction calibrations, 2. set unit mode to current
            }

            operation = new DataViewModel { CommandName = "PI Speed Loop Feedback", CommandId = "50", CommandSubId = "6", IsFloat = false, CommandValue = (ExternalSpeedLoop + 1).ToString() };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            /*update  "PI Position Loop Feedback" parameter*/
            if(id_fdbck_cmd_temp == "")
            {
                if(id_ext_fdbck_cmd_temp != "")
                    ExternalPositionLoop = 2; //external encoder
                else if(MotorType == (int)eMotorType.Three_Phase_Brushless)
                    ExternalPositionLoop = 0; //use hall
                else
                    ExternalPositionLoop = -1; //  1. remove PI Speed/Position/direction calibrations, 2. set unit mode to current
            }

            operation = new DataViewModel { CommandName = "PI Position Loop Feedback", CommandId = "50", CommandSubId = "7", IsFloat = false, CommandValue = (ExternalPositionLoop + 1).ToString() };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            operation = new DataViewModel { CommandName = "Drive Mode", CommandId = "50", CommandSubId = "1", IsFloat = false, CommandValue = (ExternalDriveMode + 1).ToString() };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            operation = new DataViewModel { CommandName = "Command Source", CommandId = "50", CommandSubId = "5", IsFloat = false, CommandValue = (ExternalCommandSource + 1).ToString() };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            if(ExternalCommandSource == (int)eCommandSource.Analog)
            {
                switch(ExternalDriveMode)
                {
                    case (int)eDriveMode.Current_Control:
                        index_analog_command_value = "0";
                        break;
                    case (int)eDriveMode.Speed_Control:
                        index_analog_command_value = "1";
                        break;
                    case (int)eDriveMode.Position_Control:
                        index_analog_command_value = "2";
                        break;
                    default:
                        index_analog_command_value = "";
                        break;
                }
                operation = new DataViewModel { CommandName = "Analog Command Value", CommandId = "110", CommandSubId = index_analog_command_value, IsFloat = true, CommandValue = AnalogCommandValue };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
            }


            //operation = new DataViewModel { CommandName = "Save", CommandId = "63", CommandSubId = "0", IsFloat = false, CommandValue = "1" };
            //Int32.TryParse(operation.CommandId, out commandId);
            //Int32.TryParse(operation.CommandSubId, out commandSubId);
            //GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            //operation = new DataViewModel { CommandName = "Reset", CommandId = "63", CommandSubId = "9", IsFloat = false, CommandValue = "1" };
            //Int32.TryParse(operation.CommandId, out commandId);
            //Int32.TryParse(operation.CommandSubId, out commandSubId);
            //GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
            operation_echo.Clear();
            DataViewModel myValue;
            for(int i = 0; i < GetInstance.OperationList.Count; i++)
            {
                GetInstance.OperationList.TryGetValue(new Tuple<int, int>(Convert.ToInt16(GetInstance.OperationList.ElementAt(i).Value.CommandId), Convert.ToInt16(GetInstance.OperationList.ElementAt(i).Value.CommandSubId)), out myValue);
                operation_echo.Add(new Tuple<int, int>(Convert.ToInt16(GetInstance.OperationList.ElementAt(i).Value.CommandId), Convert.ToInt16(GetInstance.OperationList.ElementAt(i).Value.CommandSubId)), myValue);
            }

            operation = new DataViewModel { CommandName = "Synchronisation Command", CommandId = "64", CommandSubId = "0", IsFloat = false, CommandValue = "1" };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation); // Restart Plot
            #endregion BuildOperationList

            #region Save_parameters_to_ini_file
            saveWizardParams();
            #endregion Save_parameters_to_ini_file

            sendPreStartOperation();

            if(!cancellationTokenCalib.IsCancellationRequested)
            {
                int timeout = 0;
                while(GetInstance.OperationList.Count != GetInstance.send_operation_count + 1 &&
                    timeout < GetInstance.CalibrationWizardList.ElementAt(0).Value.CalibTimeout &&
                    !cancellationTokenCalib.IsCancellationRequested)
                {
                    Thread.Sleep(1000);
                    timeout++;
                }
                if(timeout >= GetInstance.CalibrationWizardList.ElementAt(0).Value.CalibTimeout)
                {
                    cancellationTokenCalib = new CancellationToken(true);

                    GetInstance.CalibrationWizardList[new Tuple<int, int>(6, -1)].CalibStatus = RoundBoolLed.FAILED;
                    StartButtonStop();

                    EventRiser.Instance.RiseEevent(string.Format($"Missing some commands echo"));
                    for(int i = 0; i < operation_echo.Count; i++)
                        EventRiser.Instance.RiseEevent("Missing " + operation_echo.ElementAt(i).Value.CommandName + ": " +
                            operation_echo.ElementAt(i).Value.CommandId + "[" +
                            operation_echo.ElementAt(i).Value.CommandSubId + "]");

                    return;
                }
                GetInstance.send_update_parameters = false;
                GetInstance.CalibrationWizardList[new Tuple<int, int>(6, -1)].CalibStatus = RoundBoolLed.PASSED;
            };
            Thread.Sleep(300);

            GetInstance.OperationList.Clear();

            for(int i = 1; i < GetInstance.CalibrationWizardList.Count; i++)
            {
                operation = new DataViewModel
                {
                    CommandName = GetInstance.CalibrationWizardList.ElementAt(i).Value.CalibrationName,
                    CommandId = "6",
                    CommandSubId = GetInstance.CalibrationWizardList.ElementAt(i).Value.CommandSubId,
                    IsFloat = false,
                    CommandValue = "1"
                };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                if(GetInstance.CalibrationWizardList.ElementAt(i).Value.CalibrationPerform)
                {
                    GetInstance.CalibrationWizardList.ElementAt(i).Value.CalibrationEnabled = false;
                    GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
                }
            }

            DebugViewModel.updateList = true;
            RefreshManager.GetInstance.BuildGenericCommandsList_Func();

            CalibrationStart();
            CalibrationGetStatus();
        }
        public ActionCommand Abort { get { return new ActionCommand(AbortCalib); } }
        public void AbortCalib()
        {
            cancellationTokenCalib = new CancellationToken(true);

            Rs232Interface.GetInstance.SendToParser(new PacketFields
            {
                Data2Send = 0,
                ID = 1,
                SubID = 0,
                IsSet = true,
                IsFloat = false
            });
            Thread.Sleep(10);

            GetInstance.Count = GetInstance.OperationList.Count;

            StartButtonStop();
        }
        private bool _save = false;
        public bool Save
        {
            get { return _save; }
            set
            {

                if(value && LeftPanelViewModel._app_running)
                {
                    _save = value;
                    Rs232Interface.GetInstance.SendToParser(new PacketFields
                    {
                        Data2Send = 1,
                        ID = 63,
                        SubID = Convert.ToInt16(0),
                        IsSet = true,
                        IsFloat = false
                    }
                    );
                    Task WaitSave = Task.Run((Action)GetInstance.Wait);
                }
                else if(!value)
                {
                    _save = value;
                }

                OnPropertyChanged();
            }
        }
        private bool _restore = false;
        public bool Restore
        {
            get { return _restore; }
            set
            {

                if(value && LeftPanelViewModel._app_running)
                {
                    _restore = value;
                    Rs232Interface.GetInstance.SendToParser(new PacketFields
                    {
                        Data2Send = 1,
                        ID = 63,
                        SubID = Convert.ToInt16(9),
                        IsSet = true,
                        IsFloat = false
                    }
                    );
                    Task WaitSave = Task.Run((Action)GetInstance.Wait);
                }
                else if(!value)
                {
                    _restore = value;
                }

                OnPropertyChanged();
            }
        }
        private void Wait()
        {
            Thread.Sleep(1000);
            Save = false;
            Restore = false;
        }
        public ObservableCollection<object> WizardOperation
        {

            get
            {
                return Commands.GetInstance.GenericCommandsGroup["WizardOperation"];
            }
        }
        #endregion Calibration
        #region AdvancedConfiguration
        private bool _calibrationAdvancedMode = false;
        public bool CalibrationAdvancedMode
        {
            get { return _calibrationAdvancedMode; }
            set
            {
                _calibrationAdvancedMode = value;
                BuildCalibrationWizardList();
                OnPropertyChanged("CalibrationAdvancedMode");
            }
        }
        private bool _advancedConfig = false;
        public bool AdvancedConfig
        {
            get { return _advancedConfig; }
            set
            {
                _advancedConfig = value;
                if(!value)
                    CalibrationAdvancedMode = false;
                OnPropertyChanged("AdvancedConfig");
            }
        }
        private bool _loadDefault = false;
        public bool LoadDefault
        {
            get { return _loadDefault; }
            set
            {
                _loadDefault = value;
                OnPropertyChanged("LoadDefault");
            }
        }
        private int _externalEncoder = (int)eEncSel.Enc_Fdb_None;
        public int ExternalEncoder
        {
            get { return _externalEncoder; }
            set
            {
                _externalEncoder = value;
                switch(value)
                {
                    case (int)eEncSel.Enc_Fdb_None:
                        ExternalResolution = "1000";
                        break;
                    case (int)eEncSel.Enc_Fdb_Abs_Sin_Cos:
                        ExternalResolution = "4096";
                        break;
                    default:
                        EncoderResolution = _encoderResolution;
                        break;
                }
                if(value == 0)
                    ExternalEncoderExist = false;
                else
                    ExternalEncoderExist = true;
                OnPropertyChanged("ExternalEncoder");
            }
        }
        private bool _externalEncoderExist = false;
        public bool ExternalEncoderExist
        {
            get { return _externalEncoderExist; }
            set
            {
                _externalEncoderExist = value;
                OnPropertyChanged("ExternalEncoderExist");
            }
        }
        private string _externalResolution = "";
        public string ExternalResolution
        {
            get { return _externalResolution; }
            set
            {
                if(String.IsNullOrWhiteSpace(value))
                    return;
                if(ExternalEncoder == (int)eEncSel.Enc_Fdb_Ssi && Convert.ToInt32(value) < 32)
                    _externalResolution = value;
                else if(ExternalEncoder != (int)eEncSel.Enc_Fdb_Ssi)
                    _externalResolution = value;
                else
                    return;
                OnPropertyChanged("ExternalResolution");
            }
        }
        private int _externalSpeedLoop = (int)eClaFdb.Cla_Fdb_Motor - 1;
        public int ExternalSpeedLoop
        {
            get { return _externalSpeedLoop; }
            set
            {
                _externalSpeedLoop = value;
                OnPropertyChanged("ExternalSpeedLoop");
            }
        }
        private int _externalPositionLoop = (int)eClaFdb.Cla_Fdb_Motor - 1;
        public int ExternalPositionLoop
        {
            get { return _externalPositionLoop; }
            set
            {
                _externalPositionLoop = value;
                OnPropertyChanged("ExternalPositionLoop");
            }
        }
        private int _externalDriveMode = 2 - 1; // Speed Control
        public int ExternalDriveMode
        {
            get { return _externalDriveMode; }
            set
            {
                _externalDriveMode = value;
                OnPropertyChanged("ExternalDriveMode");
            }
        }
        private int _externalCommandSource = 1 - 1; // Digital_Cmd
        public int ExternalCommandSource
        {
            get { return _externalCommandSource; }
            set
            {
                _externalCommandSource = value;
                OnPropertyChanged("ExternalCommandSource");
            }
        }
        private string _analogCommandValue = "";
        public string AnalogCommandValue
        {
            get { return _analogCommandValue; }
            set
            {
                _analogCommandValue = value;
                OnPropertyChanged("AnalogCommandValue");
            }
        }
        #endregion AdvancedConfiguration
        #region Tasks
        private bool _motorParameters = false;
        public void VerifyValidOperation()
        {
            //if(!_motorParameters)
            { // if Motor Parameters Group is not valid 
                switch(_motorType)
                {
                    case 0:
                        if(_continuousCurrent != "" && _continuousCurrent != "0")
                        {
                            if(_maxSpeed != "" && _maxSpeed != "0")
                            {
                                switch(_encoderFeedback)
                                {
                                    case 0:
                                        ValidOperations = RoundBoolLed.FAILED;
                                        _motorParameters = false;
                                        break;
                                    case 1:
                                    case 2:
                                    case 4:
                                        ValidOperations = RoundBoolLed.FAILED;
                                        _motorParameters = false;
                                        break;
                                    default:
                                        if(_encoderResolution != "" && _encoderResolution != "0")
                                        {
                                            ValidOperations = RoundBoolLed.PASSED;
                                            _motorParameters = true;
                                        }
                                        else
                                        {
                                            ValidOperations = RoundBoolLed.FAILED;
                                            _motorParameters = false;
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                ValidOperations = RoundBoolLed.FAILED;
                                _motorParameters = false;
                            }
                        }
                        else
                        {
                            ValidOperations = RoundBoolLed.FAILED;
                            _motorParameters = false;
                        }
                        break;
                    case 1:
                        if(_polePaire != "" && _polePaire != "0")
                        {
                            if(_continuousCurrent != "" && _continuousCurrent != "0")
                            {
                                if(_maxSpeed != "" && _maxSpeed != "0")
                                {
                                    switch(_hallEnDis)
                                    {
                                        case 0: // Hall Disable
                                            switch(_encoderFeedback)
                                            {
                                                case 0:
                                                    ValidOperations = RoundBoolLed.FAILED;
                                                    _motorParameters = false;
                                                    break;
                                                default:
                                                    if(_encoderResolution != "" && _encoderResolution != "0")
                                                    {
                                                        ValidOperations = RoundBoolLed.PASSED;
                                                        _motorParameters = true;
                                                    }
                                                    else
                                                    {
                                                        ValidOperations = RoundBoolLed.FAILED;
                                                        _motorParameters = false;
                                                    }
                                                    break;
                                            }
                                            break;
                                        case 1: // Hall Enable
                                            switch(_encoderFeedback)
                                            {
                                                case 0:
                                                    ValidOperations = RoundBoolLed.PASSED;
                                                    _motorParameters = true;
                                                    break;
                                                default:
                                                    if(_encoderResolution != "" && _encoderResolution != "0")
                                                    {
                                                        ValidOperations = RoundBoolLed.PASSED;
                                                        _motorParameters = true;
                                                    }
                                                    else
                                                    {
                                                        ValidOperations = RoundBoolLed.FAILED;
                                                        _motorParameters = false;
                                                    }
                                                    break;
                                            }
                                            break;
                                    }
                                }
                                else
                                {
                                    ValidOperations = RoundBoolLed.FAILED;
                                    _motorParameters = false;
                                }
                            }
                            else
                            {
                                ValidOperations = RoundBoolLed.FAILED;
                                _motorParameters = false;
                            }
                        }
                        else
                        {
                            ValidOperations = RoundBoolLed.FAILED;
                            _motorParameters = false;
                        }
                        break;
                }
            }
            // if Motor Parameters Group is valid and external encoder feedback selectionned, 
            // then verify External Group valid operation.
            if(_motorParameters)// 
            {
                if(_encoderFeedback != _externalEncoder &&
                    _externalEncoder != 0 &&
                    _externalResolution != "" && _externalResolution != "0")
                    ValidOperations = RoundBoolLed.PASSED;
                else
                    ValidOperations = RoundBoolLed.FAILED;

                if(_motorType == 1 && _hallEnDis == 0 && (_externalSpeedLoop == 0 || _externalPositionLoop == 0))
                    ValidOperations = RoundBoolLed.FAILED;
                else
                    ValidOperations = RoundBoolLed.PASSED;
                if(_encoderFeedback == 0 && (_externalSpeedLoop == 1 || _externalPositionLoop == 1))
                    ValidOperations = RoundBoolLed.FAILED;
                else
                    ValidOperations = RoundBoolLed.PASSED;
                if(_externalEncoder == 0 && (_externalSpeedLoop == 2 || _externalPositionLoop == 2))
                    ValidOperations = RoundBoolLed.FAILED;
                else
                    ValidOperations = RoundBoolLed.PASSED;
            }
        }

        #region Task_NOT_IN_USE
        public const int START = 1;
        public const int STOP = 0;

        private Timer _calibrationGetStatus;
        const double _calibrationGetStatusInterval = 300;
        private void CalibrationGetStatusTask(int _mode = STOP)
        {
            switch(_mode)
            {
                case STOP:
                    lock(this)
                    {
                        if(GetInstance._calibrationGetStatus != null)
                        {
                            lock(GetInstance._calibrationGetStatus)
                            {
                                GetInstance._calibrationGetStatus.Stop();
                                //GetInstance._calibrationGetStatus.Elapsed -= GetInstance.sendPreStartOperation;
                                GetInstance._calibrationGetStatus = null;
                                Thread.Sleep(10);
                            }
                        }
                    }
                    break;
                case START:
                    if(GetInstance._calibrationGetStatus == null)
                    {
                        Thread.Sleep(100);
                        GetInstance._calibrationGetStatus = new Timer(_calibrationGetStatusInterval) { AutoReset = false };
                        //GetInstance._calibrationGetStatus.Elapsed += GetInstance.sendPreStartOperation;
                        GetInstance._calibrationGetStatus.Start();
                    }
                    break;
            }
        }
        #endregion Task_NOT_IN_USE

        private int Count = 0;
        private void CalibrationStart()
        {
            if(GetInstance.Count < GetInstance.OperationList.Count)
            {
                Rs232Interface.GetInstance.SendToParser(new PacketFields
                {
                    Data2Send = 1,
                    ID = Convert.ToInt16(GetInstance.OperationList.ElementAt(GetInstance.Count).Value.CommandId),
                    SubID = Convert.ToInt16(Convert.ToInt16(GetInstance.OperationList.ElementAt(GetInstance.Count).Value.CommandSubId) - 1),
                    IsSet = true,
                    IsFloat = false
                });
                Debug.WriteLine(GetInstance.OperationList.ElementAt(GetInstance.Count).Value.CommandId + "[" + Convert.ToInt16(Convert.ToInt16(GetInstance.OperationList.ElementAt(GetInstance.Count).Value.CommandSubId) - 1) + "]");
            }
        }
        private void CalibrationGetStatus()
        {
            int calibration_index = -1;
            int max_timeout = 15;
            int calibration_timeout = max_timeout;

            while(GetInstance.Count < GetInstance.OperationList.Count && /*!StartEnable*/ calibration_is_in_process)
            {
                if(calibration_index != Convert.ToInt16(GetInstance.OperationList.ElementAt(GetInstance.Count).Value.CommandSubId))
                {
                    calibration_index = Convert.ToInt16(GetInstance.OperationList.ElementAt(GetInstance.Count).Value.CommandSubId);
                    calibration_timeout = GetInstance.CalibrationWizardList.ElementAt(GetInstance.Count + 1).Value.CalibTimeout;
                }
                if(calibration_timeout == 0)
                {
                    EventRiser.Instance.RiseEevent(string.Format($"Calibration timeout occurred!"));
                    AbortCalib();
                    return;
                }
                //Rs232Interface.GetInstance.SendToParser(new PacketFields
                //{
                //    Data2Send = 1,
                //    ID = Convert.ToInt16(GetInstance.OperationList.ElementAt(GetInstance.Count).Value.CommandId),
                //    SubID = Convert.ToInt16(GetInstance.OperationList.ElementAt(GetInstance.Count).Value.CommandSubId),
                //    IsSet = false,
                //    IsFloat = false
                //});
                //Debug.WriteLine(GetInstance.OperationList.ElementAt(GetInstance.Count).Value.CommandId + "[" + Convert.ToInt16(GetInstance.OperationList.ElementAt(GetInstance.Count).Value.CommandSubId) + "]");
                Thread.Sleep(1000);
                calibration_timeout--;
                Debug.WriteLine("timeout" + calibration_timeout);
            }
        }
        public void updateCalibrationStatus(Tuple<int, int> commandidentifier, string newPropertyValue)
        {
            int StateTemp = 0;

            switch(Convert.ToInt16(newPropertyValue))
            {
                case 0:
                    StateTemp = RoundBoolLed.IDLE;
                    break;
                case 1:
                    StateTemp = RoundBoolLed.RUNNING;
                    break;
                case 2:
                    StateTemp = RoundBoolLed.FAILED;
                    break;
                case 3:
                    StateTemp = RoundBoolLed.PASSED;
                    break;
                case 5:
                    StateTemp = RoundBoolLed.STOPPED;
                    break;
                default:
                    StateTemp = RoundBoolLed.FAILED;
                    break;
            }
            if(/*StartEnable*/ !calibration_is_in_process)
                GetInstance.CalibrationWizardList[new Tuple<int, int>(6, commandidentifier.Item2)].CalibStatus = StateTemp;
            else
            {
                if(GetInstance.CalibrationWizardList[new Tuple<int, int>(6, commandidentifier.Item2)].CalibStatus != StateTemp)
                {
                    GetInstance.CalibrationWizardList[new Tuple<int, int>(6, commandidentifier.Item2)].CalibStatus = StateTemp;
                    if(StateTemp == RoundBoolLed.FAILED)
                    {
                        AbortCalib();
                    }
                    else if(StateTemp == RoundBoolLed.PASSED)
                    {
                        GetInstance.Count++;
                        CalibrationStart();
                    }
                }
                if(GetInstance.Count == GetInstance.OperationList.Count && /*!StartEnable*/ calibration_is_in_process && GetInstance.OperationList.Count != 0)
                    StartButtonStop();

                Thread.Sleep(100);
            }
            //CalibrationGetStatusTask(STOP);
        }
        private bool _send_update_parameters = false;
        public bool send_update_parameters
        {
            get { return _send_update_parameters; }
            set
            {
                _send_update_parameters = value;
                OnPropertyChanged();
            }
        }
        private void sendPreStartOperation()
        {
            GetInstance.send_update_parameters = true;
            GetInstance.send_operation_count = 0;

            Thread.Sleep(10);
            for(int i = 0; i < GetInstance.OperationList.Count; i++)
            {
                if(cancellationTokenCalib.IsCancellationRequested)
                {
                    Debug.WriteLine("sendPreStartOperation Stopped");
                    GetInstance.CalibrationWizardList.ElementAt(0).Value.CalibStatus = RoundBoolLed.STOPPED;
                    return;
                };
                Rs232Interface.GetInstance.SendToParser(new PacketFields
                {
                    Data2Send = GetInstance.OperationList.ElementAt(i).Value.CommandValue,
                    ID = Convert.ToInt16(GetInstance.OperationList.ElementAt(i).Value.CommandId),
                    SubID = Convert.ToInt16(GetInstance.OperationList.ElementAt(i).Value.CommandSubId),
                    IsSet = true,
                    IsFloat = GetInstance.OperationList.ElementAt(i).Value.IsFloat
                });
                Debug.WriteLine("Operation: " + GetInstance.OperationList.ElementAt(i).Value.CommandId + "[" + GetInstance.OperationList.ElementAt(i).Value.CommandSubId + "] = " + GetInstance.OperationList.ElementAt(i).Value.CommandValue + " - " + GetInstance.OperationList.ElementAt(i).Value.IsFloat.ToString());
                if(GetInstance.OperationList.ElementAt(i).Value.CommandName == "Load Default" ||
                    GetInstance.OperationList.ElementAt(i).Value.CommandName == "Save" ||
                    GetInstance.OperationList.ElementAt(i).Value.CommandName == "Reset")
                    Thread.Sleep(2000);
                Thread.Sleep(30);
            }
        }
        #endregion Tasks

        string _section = "Wizard";
        public void saveWizardParams()
        {
            string path = "\\MotorController\\Wizard\\";
            path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + path;
            if(!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string _file_name = "WizardParameters.ini";
            iniFile _wizard_parameters_file = new iniFile(path + _file_name);

            #region Save_Parameters

            _wizard_parameters_file.Write("Motor Type", Enum.GetNames(typeof(eMotorType)).ElementAt(MotorType), _section);
            _wizard_parameters_file.Write("Pole Paire", PolePair, _section);
            _wizard_parameters_file.Write("Continuous Current", ContinuousCurrent, _section);
            _wizard_parameters_file.Write("Max Speed", MaxSpeed, _section);
            _wizard_parameters_file.Write("Hall", Enum.GetNames(typeof(eHall)).ElementAt(HallEnDis), _section);
            _wizard_parameters_file.Write("Encoder Feedback", Enum.GetNames(typeof(eEncSel)).ElementAt(EncoderFeedback), _section);
            _wizard_parameters_file.Write("Encoder Resolution", EncoderResolution, _section);

            #endregion Save_Parameters

            #region Advanced_Configuration

            _wizard_parameters_file.Write("External Encoder Feedback", Enum.GetNames(typeof(eEncSel)).ElementAt(ExternalEncoder), _section);
            _wizard_parameters_file.Write("External Encoder Resolution", ExternalResolution, _section);
            _wizard_parameters_file.Write("Speed Loop", Enum.GetNames(typeof(eClaFdb)).ElementAt(ExternalSpeedLoop), _section);
            _wizard_parameters_file.Write("Position Loop", Enum.GetNames(typeof(eClaFdb)).ElementAt(ExternalPositionLoop), _section);
            _wizard_parameters_file.Write("Drive Mode", Enum.GetNames(typeof(eDriveMode)).ElementAt(ExternalDriveMode), _section);
            _wizard_parameters_file.Write("Command Source", Enum.GetNames(typeof(eCommandSource)).ElementAt(ExternalCommandSource), _section);

            #endregion Advanced_Configuration

            /*
            string tempar = Enum.GetNames(typeof(eMotorType)).ElementAt(MotorType); // Get string name of enum with int value. 

            int val = (int)EnumHelper.GetEnumValue<eMotorType>("Three_Phase_Brushless"); // Get int num of enum with string value.
            */
        }
        private void readWizardParams()
        {
            string path = "\\MotorController\\Wizard\\";
            path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + path;
            string _file_name = path + "WizardParameters.ini";

            if(!File.Exists(_file_name))
                EventRiser.Instance.RiseEevent(string.Format($"No wizard parameters file found!"));
            else
            {
                iniFile _wizard_parameters_file = new iniFile(_file_name);

                #region Read_Parameters

                MotorType = (int)EnumHelper.GetEnumValue<eMotorType>(_wizard_parameters_file.Read("Motor Type", _section));
                PolePair = _wizard_parameters_file.Read("Pole Paire", _section);
                ContinuousCurrent = _wizard_parameters_file.Read("Continuous Current", _section);
                MaxSpeed = _wizard_parameters_file.Read("Max Speed", _section);
                HallEnDis = (int)EnumHelper.GetEnumValue<eHall>(_wizard_parameters_file.Read("Hall", _section));
                EncoderFeedback = (int)EnumHelper.GetEnumValue<eEncSel>(_wizard_parameters_file.Read("Encoder Feedback", _section));
                EncoderResolution = _wizard_parameters_file.Read("Encoder Resolution", _section);

                #endregion Read_Parameters

                #region Advanced_Configuration

                ExternalEncoder = (int)EnumHelper.GetEnumValue<eEncSel>(_wizard_parameters_file.Read("External Encoder Feedback", _section));
                ExternalResolution = _wizard_parameters_file.Read("External Encoder Resolution", _section);
                ExternalSpeedLoop = (int)EnumHelper.GetEnumValue<eClaFdb>(_wizard_parameters_file.Read("Speed Loop", _section));
                ExternalPositionLoop = (int)EnumHelper.GetEnumValue<eClaFdb>(_wizard_parameters_file.Read("Position Loop", _section));
                ExternalDriveMode = (int)EnumHelper.GetEnumValue<eDriveMode>(_wizard_parameters_file.Read("Drive Mode", _section));
                ExternalCommandSource = (int)EnumHelper.GetEnumValue<eCommandSource>(_wizard_parameters_file.Read("Command Source", _section));

                #endregion Advanced_Configuration
            }
        }

        public virtual ICommand WizardWindowLoaded
        {
            get
            {
                return new RelayCommand(WizardWindowLoaded_Func);
            }
        }
        public ICommand WizardWindowClosed
        {
            get
            {
                return new RelayCommand(WizardWindowClosed_Func);
            }
        }
        public static bool _is_wizard_window_opened = false;
        private void WizardWindowLoaded_Func()
        {
            LeftPanelViewModel.GetInstance.cancelRefresh = new CancellationToken(true);
            RefreshManager.GetInstance.BuildGenericCommandsList_Func();
        }
        private void WizardWindowClosed_Func()
        {
            LeftPanelViewModel.GetInstance._wizard_window.Visibility = Visibility.Hidden;
            LeftPanelViewModel.GetInstance.cancelRefresh = new CancellationToken(true);
            RefreshManager.GetInstance.BuildGenericCommandsList_Func();
        }
    }
}
