using Abt.Controls.SciChart;
using SuperButton.Helpers;
using SuperButton.Models.DriverBlock;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;
using System.Diagnostics;
using SuperButton.CommandsDB;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Input;
using SuperButton.Models;

namespace SuperButton.ViewModels
{
    enum MODE
    {
        Normal = 0,
        Advanced = 1
    };
    enum ClaFdb
    {
        Cla_Fdb_None = 0x0000,
        Cla_Fdb_Hall = 0x0001,
        Cla_Fdb_Motor = 0x0002,
        Cla_Fdb_Ext = 0x0003,
        Cla_Fdb_End = 0x04
    };

    enum eEncSel
    {
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

    enum ComutationSource
    {
        Cmtn_Forced_Rotate = 1,
        Cmtn_Hall = 2,
        Cmtn_Enc1 = 3,
        Cmtn_Abs_Enc1 = 4,
        Cmtn_Hall_Inc_Enc1 = 5,
        Cmtn_DC_Brushed = 9
    };
    internal class WizardWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
            if (propertyName != "ValidOperations")
                VerifyValidOperation();
            if (propertyName == "StartEnable")
            {

            }
        }
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
        public Dictionary<Tuple<int, int>, CalibrationWizardViewModel> CalibrationWizardList = new Dictionary<Tuple<int, int>, CalibrationWizardViewModel>();
        public Dictionary<string, ObservableCollection<object>> CalibrationWizardListbySubGroup = new Dictionary<string, ObservableCollection<object>>();
        public Dictionary<Tuple<int, int>, DataViewModel> OperationList = new Dictionary<Tuple<int, int>, DataViewModel>();

        #region FIELDS
        private static readonly object Synlock = new object();
        private static WizardWindowViewModel _instance;
        #endregion FIELDS

        public static WizardWindowViewModel GetInstance
        {
            get
            {
                lock (Synlock)
                {
                    if (_instance != null)
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
            if (_instance == null)
            {
                CalibrationWizardListbySubGroup.Add("CalibrationList", new ObservableCollection<object>());
                BuildCalibrationWizardList();
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
        private string _polePaire = "";
        public string PolePair
        {
            get { return _polePaire; }
            set
            {
                _polePaire = value;
                OnPropertyChanged("PolePair");
            }
        }
        private string _continuousCurrent = "";
        public string ContinuousCurrent
        {
            get { return _continuousCurrent; }
            set { _continuousCurrent = value; OnPropertyChanged("ContinuousCurrent"); }
        }
        private string _maxSpeed = "";
        public string MaxSpeed
        {
            get { return _maxSpeed; }
            set { _maxSpeed = value; OnPropertyChanged("MaxSpeed"); }
        }
        private int _hallEnDis = 0;
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
        private int _encoderFeedback = 0;
        public int EncoderFeedback
        {
            get { return _encoderFeedback; }
            set
            {
                _encoderFeedback = value;
                switch (value)
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
                if (value == 0)
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
        private string _encoderResolution = "1000";
        public string EncoderResolution
        {
            get { return _encoderResolution; }
            set { _encoderResolution = value; OnPropertyChanged("EncoderResolution"); }
        }
        #endregion Motor_Parameter
        #region Calibration
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
            if (_instance != null)
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
                "Sending Operation", "PI Current Loop", "Hall Mapping", "Feedback Direction", "Electrical Angle", "PI Speed Loop", "PI Position Loop"
            };
            var SubID = new[]
            {
                "-1", "4", "6", "8", "14", "10", "12"
            };
            Dictionary<string, string> calibOperation = new Dictionary<string, string>();
            for (int i = 0; i < names.Length; i++)
                calibOperation.Add(names[i], SubID[i]);

            if (MotorType == 0 || HallEnDis == 0)
                calibOperation.Remove("Hall Mapping");
            if (!(EncoderFeedback == (int)eEncSel.Enc_Fdb_Ssi || EncoderFeedback == (int)eEncSel.Enc_Fdb_Abs_Sin_Cos))
                calibOperation.Remove("Electrical Angle");

            CalibrationWizardViewModel calibElement;
            for (int i = 0; i < calibOperation.Count; i++)
            {
                calibElement = new CalibrationWizardViewModel
                {
                    AdvanceMode_Calibration = CalibrationAdvancedMode,
                    CalibrationEnabled = i == 0 ? false : true,
                    CalibrationPerform = true,
                    CalibrationName = calibOperation.ElementAt(i).Key,
                    CalibStatus = 0,
                    CommandId = "6",
                    CommandSubId = calibOperation.ElementAt(i).Value
                };
                if (_instance != null)
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
        //private bool canExecute = true;
        //public void ChangeCanExecute(object obj)
        //{
        //    canExecute = !canExecute;
        //}
        //public bool CanExecute
        //{
        //    get
        //    {
        //        return this.canExecute;
        //    }

        //    set
        //    {
        //        if(this.canExecute == value)
        //        {
        //            return;
        //        }

        //        this.canExecute = value;
        //    }
        //}
        //private ICommand _start { get; set; }

        //public ICommand Start {
        //    get
        //    {
        //        return _start;
        //    }
        //    set
        //    {
        //        _start = value;
        //    }
        //}
        private async void StartButton()
        {
            StartEnable = false;
            await Task.Run(() => StartCalib());
            StartEnable = true;
        }
        private async void StartButtonStop()
        {
            StartEnable = true;
            Thread.Sleep(10);
            await Task.Run(() =>
            {
                for (int i = 1; i < GetInstance.CalibrationWizardList.Count; i++)
                    GetInstance.CalibrationWizardList.ElementAt(i).Value.CalibrationEnabled = true;
            }
            );
            //await Task.Run(() => CalibrationGetStatusTask(STOP));
        }
        private void StartCalib()
        {
            //updateCalibrationStatus(new Tuple<int, int>(6, -1), RoundBoolLed.RUNNING.ToString());

            //Debug.WriteLine("Start");
            //Thread.Sleep(4000);
            //Debug.WriteLine("End");
            //updateCalibrationStatus(new Tuple<int, int>(6, -1), RoundBoolLed.PASSED.ToString());
            //StartButtonStop();
            //return;


            if (!LeftPanelViewModel._app_running || ValidOperations == RoundBoolLed.FAILED)
                return;
            if (PolePair == "" || PolePair == "0" ||
                ContinuousCurrent == "" || ContinuousCurrent == "0" ||
                MaxSpeed == "" || MaxSpeed == "0" ||
                EncoderResolution == "" || EncoderResolution == "0")
                return;

            #region InitVariables
            DataViewModel operation = new DataViewModel();
            Int32 commandId = 0, commandSubId = 0;
            GetInstance.Count = 0;
            #endregion InitVariables

            //updateCalibrationStatus(new Tuple<int, int>(6, -1), "1");
            GetInstance.CalibrationWizardList[new Tuple<int, int>(6, -1)].CalibStatus = RoundBoolLed.RUNNING;
            Thread.Sleep(50);

            for (int i = 1; i < GetInstance.CalibrationWizardList.Count; i++)
                GetInstance.CalibrationWizardList.ElementAt(i).Value.CalibrationEnabled = false;

            GetInstance.OperationList.Clear();
            for (int i = 1; i < GetInstance.CalibrationWizardList.Count; i++)
            {
                GetInstance.CalibrationWizardList.ElementAt(i).Value.CalibStatus = RoundBoolLed.IDLE;

                operation = new DataViewModel { CommandName = GetInstance.CalibrationWizardList.ElementAt(i).Value.CalibrationName, CommandId = "6", CommandSubId = GetInstance.CalibrationWizardList.ElementAt(i).Value.CommandSubId, IsFloat = false, CommandValue = "1" };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                if (GetInstance.CalibrationWizardList.ElementAt(i).Value.CalibrationPerform)
                    GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
            }
            if (GetInstance.OperationList.Count == 0)
                return;

            GetInstance.OperationList.Clear();

            #region BuildOperationList
            string id_fdbck_cmd_temp = "", comutation_source = "";

            switch (EncoderFeedback)
            {
                case (int)eEncSel.Enc_Fdb_Inc1:
                    id_fdbck_cmd_temp = "71";
                    if (HallEnDis == 1) // if Hall Enable
                        comutation_source = ((int)ComutationSource.Cmtn_Hall_Inc_Enc1).ToString();
                    else
                    {
                        // Show warning popup when start clicked
                    }
                    break;
                case (int)eEncSel.Enc_Fdb_Inc_Sin_Cos:
                    id_fdbck_cmd_temp = "71";
                    if (HallEnDis == 1) // if Hall Enable
                        comutation_source = ((int)ComutationSource.Cmtn_Enc1).ToString();
                    else
                    {
                        // Show warning popup when start clicked
                    }
                    break;
                case (int)eEncSel.Enc_Fdb_Abs_Sin_Cos:
                    id_fdbck_cmd_temp = "72";
                    comutation_source = ((int)ComutationSource.Cmtn_Abs_Enc1).ToString();
                    break;
                case (int)eEncSel.Enc_Fdb_Ssi:
                    id_fdbck_cmd_temp = "73";
                    comutation_source = ((int)ComutationSource.Cmtn_Abs_Enc1).ToString();
                    break;
                default:
                    id_fdbck_cmd_temp = "";
                    comutation_source = ((int)ComutationSource.Cmtn_Hall).ToString();
                    break;
            }
            if (MotorType == 0)
                comutation_source = ((int)ComutationSource.Cmtn_DC_Brushed).ToString();

            int max_speed = (Convert.ToInt32(MaxSpeed) / 60) * Convert.ToInt32(EncoderResolution);
            int min_seed = -max_speed;

            if (ShortMode)
            {
                operation = new DataViewModel { CommandName = "Load Default", CommandId = "63", CommandSubId = "1", IsFloat = false, CommandValue = "1" };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
            }

            operation = new DataViewModel { CommandName = "Commutation Source", CommandId = "50", CommandSubId = "2", IsFloat = false, CommandValue = comutation_source };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            operation = new DataViewModel { CommandName = "Encoder Type", CommandId = "50", CommandSubId = "3", IsFloat = false, CommandValue = EncoderFeedback.ToString() };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            /*
            operation = new DataViewModel { CommandName = "External Encoder", CommandId = "50", CommandSubId = "4", IsFloat = false, CommandValue = "0" };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            operation = new DataViewModel { CommandName = "Command Source", CommandId = "50", CommandSubId = "5", IsFloat = false, CommandValue = "1" };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            operation = new DataViewModel { CommandName = "Speed Source", CommandId = "50", CommandSubId = "6", IsFloat = false, CommandValue = ((int)ClaFdb.Cla_Fdb_Motor).ToString() };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            operation = new DataViewModel { CommandName = "Position Source", CommandId = "50", CommandSubId = "7", IsFloat = false, CommandValue = ((int)ClaFdb.Cla_Fdb_Motor).ToString() };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
            */

            if (MotorType == 1)
            {
                operation = new DataViewModel { CommandName = "Pole Pair", CommandId = "51", CommandSubId = "1", IsFloat = false, CommandValue = PolePair };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
            }

            operation = new DataViewModel { CommandName = "Max Speed", CommandId = "53", CommandSubId = "1", IsFloat = false, CommandValue = max_speed.ToString() };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            operation = new DataViewModel { CommandName = "Min Speed", CommandId = "53", CommandSubId = "2", IsFloat = false, CommandValue = min_seed.ToString() };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            operation = new DataViewModel { CommandName = "Hall", CommandId = "70", CommandSubId = "1", IsFloat = false, CommandValue = HallEnDis.ToString() };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            if (id_fdbck_cmd_temp != "")
            {
                operation = new DataViewModel { CommandName = "Resolution", CommandId = id_fdbck_cmd_temp, CommandSubId = "5", IsFloat = false, CommandValue = EncoderResolution };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
            }

            if (EncoderFeedback == (int)eEncSel.Enc_Fdb_Ssi)
            {
                operation = new DataViewModel { CommandName = "PacketLenght", CommandId = "73", CommandSubId = "8", IsFloat = false, CommandValue = ((Math.Log(Convert.ToInt32(EncoderResolution)) / Math.Log(2)) + 1).ToString() };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
            }
            // min max speed change rpm
            // comutation source, if DC => dc brushed 4 options
            // Resolution command ID
            operation = new DataViewModel { CommandName = "Continuous Current", CommandId = "52", CommandSubId = "1", IsFloat = true, CommandValue = ContinuousCurrent };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            operation = new DataViewModel { CommandName = "Peak Current", CommandId = "52", CommandSubId = "2", IsFloat = true, CommandValue = ContinuousCurrent };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            switch (ExternalEncoder)
            {
                case (int)eEncSel.Enc_Fdb_Inc1:
                    id_fdbck_cmd_temp = "71";
                    break;
                case (int)eEncSel.Enc_Fdb_Inc_Sin_Cos:
                    id_fdbck_cmd_temp = "71";
                    break;
                case (int)eEncSel.Enc_Fdb_Abs_Sin_Cos:
                    id_fdbck_cmd_temp = "72";
                    break;
                case (int)eEncSel.Enc_Fdb_Ssi:
                    id_fdbck_cmd_temp = "73";
                    break;
                default:
                    id_fdbck_cmd_temp = "";
                    break;
            }

            operation = new DataViewModel { CommandName = "External Encoder Type", CommandId = "50", CommandSubId = "4", IsFloat = false, CommandValue = ExternalEncoder.ToString() };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            if (id_fdbck_cmd_temp != "")
            {
                operation = new DataViewModel { CommandName = "External Encoder Resolution", CommandId = id_fdbck_cmd_temp, CommandSubId = "5", IsFloat = false, CommandValue = ExternalResolution };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
            }

            operation = new DataViewModel { CommandName = "PI Speed Loop Feedback", CommandId = "50", CommandSubId = "6", IsFloat = false, CommandValue = (ExternalSpeedLoop + 1).ToString() };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

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

            operation = new DataViewModel { CommandName = "Save", CommandId = "63", CommandSubId = "0", IsFloat = false, CommandValue = "1" };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            operation = new DataViewModel { CommandName = "Reset", CommandId = "63", CommandSubId = "9", IsFloat = false, CommandValue = "1" };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            operation = new DataViewModel { CommandName = "Synchronisation Command", CommandId = "64", CommandSubId = "0", IsFloat = false, CommandValue = "1" };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation); // Restart Plot

            #endregion BuildOperationList
            sendPreStartOperation();
            GetInstance.OperationList.Clear();

            //updateCalibrationStatus(new Tuple<int, int>(6, -1), "3");
            GetInstance.CalibrationWizardList[new Tuple<int, int>(6, -1)].CalibStatus = RoundBoolLed.PASSED;
            Thread.Sleep(300);

            for (int i = 1; i < GetInstance.CalibrationWizardList.Count; i++)
            {
                operation = new DataViewModel {
                    CommandName = GetInstance.CalibrationWizardList.ElementAt(i).Value.CalibrationName,
                    CommandId = "6",
                    CommandSubId = GetInstance.CalibrationWizardList.ElementAt(i).Value.CommandSubId,
                    IsFloat = false,
                    CommandValue = "1"
                };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                if (GetInstance.CalibrationWizardList.ElementAt(i).Value.CalibrationPerform)
                {
                    GetInstance.CalibrationWizardList.ElementAt(i).Value.CalibrationEnabled = false;
                    GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
                }
            }
            //GetInstance.Count = 0;
            //await Task.Run(() =>
            //{
            //CalibrationGetStatusTask(START);

            CalibrationStart();
            CalibrationGetStatus();
            
            //});
        }
        public ActionCommand Abort { get { return new ActionCommand(AbortCalib); } }
        private void AbortCalib()
        {
            if (StartEnable)
                return;

            Rs232Interface.GetInstance.SendToParser(new PacketFields
            {
                Data2Send = 0,
                ID = 1,
                SubID = 0,
                IsSet = true,
                IsFloat = false
            });
            Thread.Sleep(10);

            Rs232Interface.GetInstance.SendToParser(new PacketFields
            {
                Data2Send = 0,
                ID = 63,
                SubID = 9,
                IsSet = true,
                IsFloat = false
            });

            StartButtonStop();
            Thread.Sleep(100);
            GetInstance.Count = GetInstance.OperationList.Count;
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
                if (!value)
                    CalibrationAdvancedMode = false;
                OnPropertyChanged("AdvancedConfig");
            }
        }
        private bool _shortMode = true;
        public bool ShortMode
        {
            get { return _shortMode; }
            set
            {
                _shortMode = value;
                OnPropertyChanged("ShortMode");
            }
        }
        private int _externalEncoder = 0;
        public int ExternalEncoder
        {
            get { return _externalEncoder; }
            set
            {
                _externalEncoder = value;
                switch (value)
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
                if (value == 0)
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
            set { _externalResolution = value; OnPropertyChanged("ExternalResolution"); }
        }
        private int _externalSpeedLoop = (int)ClaFdb.Cla_Fdb_Motor - 1;
        public int ExternalSpeedLoop
        {
            get { return _externalSpeedLoop; }
            set
            {
                _externalSpeedLoop = value;
                OnPropertyChanged("ExternalSpeedLoop");
            }
        }
        private int _externalPositionLoop = (int)ClaFdb.Cla_Fdb_Motor - 1;
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
        #endregion AdvancedConfiguration
        #region Tasks
        private bool _motorParameters = false;
        public void VerifyValidOperation()
        {
            //if(!_motorParameters)
            { // if Motor Parameters Group is not valid 
                switch (_motorType)
                {
                    case 0:
                        if (_continuousCurrent != "" && _continuousCurrent != "0")
                        {
                            if (_maxSpeed != "" && _maxSpeed != "0")
                            {
                                switch (_encoderFeedback)
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
                                        if (_encoderResolution != "" && _encoderResolution != "0")
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
                        if (_polePaire != "" && _polePaire != "0")
                        {
                            if (_continuousCurrent != "" && _continuousCurrent != "0")
                            {
                                if (_maxSpeed != "" && _maxSpeed != "0")
                                {
                                    switch (_hallEnDis)
                                    {
                                        case 0: // Hall Disable
                                            switch (_encoderFeedback)
                                            {
                                                case 0:
                                                    ValidOperations = RoundBoolLed.FAILED;
                                                    _motorParameters = false;
                                                    break;
                                                default:
                                                    if (_encoderResolution != "" && _encoderResolution != "0")
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
                                            switch (_encoderFeedback)
                                            {
                                                case 0:
                                                    ValidOperations = RoundBoolLed.PASSED;
                                                    _motorParameters = true;
                                                    break;
                                                default:
                                                    if (_encoderResolution != "" && _encoderResolution != "0")
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
            if (_motorParameters)// 
            {
                if (_encoderFeedback != _externalEncoder &&
                    _externalEncoder != 0 &&
                    _externalResolution != "" && _externalResolution != "0")
                    ValidOperations = RoundBoolLed.PASSED;
                else
                    ValidOperations = RoundBoolLed.FAILED;

                if (_motorType == 1 && _hallEnDis == 0 && (_externalSpeedLoop == 0 || _externalPositionLoop == 0))
                    ValidOperations = RoundBoolLed.FAILED;
                else
                    ValidOperations = RoundBoolLed.PASSED;
                if (_encoderFeedback == 0 && (_externalSpeedLoop == 1 || _externalPositionLoop == 1))
                    ValidOperations = RoundBoolLed.FAILED;
                else
                    ValidOperations = RoundBoolLed.PASSED;
                if (_externalEncoder == 0 && (_externalSpeedLoop == 2 || _externalPositionLoop == 2))
                    ValidOperations = RoundBoolLed.FAILED;
                else
                    ValidOperations = RoundBoolLed.PASSED;
            }
        }

        public const int START = 1;
        public const int STOP = 0;

        private Timer _calibrationGetStatus;
        const double _calibrationGetStatusInterval = 300;
        private void CalibrationGetStatusTask(int _mode = STOP)
        {
            switch (_mode)
            {
                case STOP:
                    lock (this)
                    {
                        if (GetInstance._calibrationGetStatus != null)
                        {
                            lock (GetInstance._calibrationGetStatus)
                            {
                                GetInstance._calibrationGetStatus.Stop();
                                //GetInstance._calibrationGetStatus.Elapsed -= GetInstance.CalibrationGetStatus;
                                GetInstance._calibrationGetStatus = null;
                                Thread.Sleep(10);
                            }
                        }
                    }
                    break;
                case START:
                    if (GetInstance._calibrationGetStatus == null)
                    {
                        Task.Factory.StartNew(action: () =>
                        {
                            Thread.Sleep(100);
                            GetInstance._calibrationGetStatus = new Timer(_calibrationGetStatusInterval) { AutoReset = true };
                            //GetInstance._calibrationGetStatus.Elapsed += GetInstance.CalibrationGetStatus;
                            GetInstance._calibrationGetStatus.Start();
                        });
                    }
                    break;
            }
        }
        private int Count = 0;
        private void CalibrationStart()
        {
            if (GetInstance.Count < GetInstance.OperationList.Count)
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
        private void CalibrationGetStatus(/*object sender, EventArgs e*/)
        {

            while (GetInstance.Count < GetInstance.OperationList.Count && StartEnable == false)
            {
                Rs232Interface.GetInstance.SendToParser(new PacketFields
                {
                    Data2Send = 1,
                    ID = Convert.ToInt16(GetInstance.OperationList.ElementAt(GetInstance.Count).Value.CommandId),
                    SubID = Convert.ToInt16(GetInstance.OperationList.ElementAt(GetInstance.Count).Value.CommandSubId),
                    IsSet = false,
                    IsFloat = false
                });
                Debug.WriteLine(GetInstance.OperationList.ElementAt(GetInstance.Count).Value.CommandId + "[" + Convert.ToInt16(GetInstance.OperationList.ElementAt(GetInstance.Count).Value.CommandSubId) + "]");
                Thread.Sleep(1000);
            }
            //else
            //CalibrationGetStatusTask(STOP);
        }
        public void updateCalibrationStatus(Tuple<int, int> commandidentifier, string newPropertyValue)
        {
            int StateTemp = 0;

            switch (Convert.ToInt16(newPropertyValue))
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
                default:
                    StateTemp = RoundBoolLed.FAILED;
                    break;
            }
            if (GetInstance.CalibrationWizardList[new Tuple<int, int>(6, commandidentifier.Item2)].CalibStatus != StateTemp)
            {
                GetInstance.CalibrationWizardList[new Tuple<int, int>(6, commandidentifier.Item2)].CalibStatus = StateTemp;
                if (StateTemp == RoundBoolLed.FAILED || StateTemp == RoundBoolLed.PASSED)
                {
                    GetInstance.Count++;
                    CalibrationStart();
                }
            }
            if (GetInstance.Count == GetInstance.OperationList.Count)
                StartButtonStop();
            Thread.Sleep(100);
            //CalibrationGetStatusTask(STOP);
        }
        private void sendPreStartOperation()
        {
            for (int i = 0; i < GetInstance.OperationList.Count; i++)
            {
                Rs232Interface.GetInstance.SendToParser(new PacketFields
                {
                    Data2Send = GetInstance.OperationList.ElementAt(i).Value.CommandValue,
                    ID = Convert.ToInt16(GetInstance.OperationList.ElementAt(i).Value.CommandId),
                    SubID = Convert.ToInt16(GetInstance.OperationList.ElementAt(i).Value.CommandSubId),
                    IsSet = true,
                    IsFloat = GetInstance.OperationList.ElementAt(i).Value.IsFloat
                });
                Debug.WriteLine("Operation: " + GetInstance.OperationList.ElementAt(i).Value.CommandId + "[" + GetInstance.OperationList.ElementAt(i).Value.CommandSubId + "] = " + GetInstance.OperationList.ElementAt(i).Value.CommandValue + " - " + GetInstance.OperationList.ElementAt(i).Value.IsFloat.ToString());
                if (GetInstance.OperationList.ElementAt(i).Value.CommandName == "Load Default" ||
                    GetInstance.OperationList.ElementAt(i).Value.CommandName == "Save" ||
                    GetInstance.OperationList.ElementAt(i).Value.CommandName == "Reset")
                    Thread.Sleep(2000);
                Thread.Sleep(10);
            }
        }
        #endregion Tasks
    }
}
