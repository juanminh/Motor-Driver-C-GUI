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

    internal class WizardWindowViewModel : ViewModelBase
    {
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
            }
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
        private string _polePaire = "1";
        public string PolePair
        {
            get { return _polePaire; }
            set { _polePaire = value; OnPropertyChanged("PolePair"); }
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
        private int _encoderFeedback = 0;
        public int EncoderFeedback
        {
            get { return _encoderFeedback; }
            set
            {
                _encoderFeedback = value;
                switch(value)
                {
                    case 0:
                        cts_Motor = "1000";
                        PolePair = "1";
                        break;
                    case 3:
                        cts_Motor = "4096";
                        break;
                    default:
                        cts_Motor = cts_Motor;
                        break;
                }
                BuildCalibrationWizardList();
                OnPropertyChanged("EncoderFeedback");
            }
        }
        private string _cts_Motor = "1000";
        public string cts_Motor
        {
            get { return _cts_Motor; }
            set { _cts_Motor = value; OnPropertyChanged("cts_Motor"); }
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
        #endregion Motor_Parameter
        #region Calibration
        private bool _startEnable = true;
        public bool StartEnable {
            get { return _startEnable; }
            set {
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
                "PI Current Loop", "Hall Mapping", "Feedback Direction", "Electrical Angle", "PI Speed Loop", "PI Position Loop"
            };
            var SubID = new[]
            {
                "4", "6", "8", "14", "10", "12"
            };
            Dictionary<string, string> calibOperation = new Dictionary<string, string>();
            for(int i = 0; i < names.Length; i++)
                calibOperation.Add(names[i], SubID[i]);

            if(MotorType == 0 || HallEnDis == 0)
                calibOperation.Remove("Hall Mapping");
            if(!(EncoderFeedback == 5 || EncoderFeedback == 3))
                calibOperation.Remove("Electrical Angle");

            CalibrationWizardViewModel calibElement;
            for(int i = 0; i < calibOperation.Count; i++)
            {
                calibElement = new CalibrationWizardViewModel
                {
                    AdvanceMode_Calibration = CalibrationAdvancedMode,
                    CalibrationPerform = true,
                    CalibrationName = calibOperation.ElementAt(i).Key,
                    CalibStatus = 0,
                    CommandId = "6",
                    CommandSubId = calibOperation.ElementAt(i).Value
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
        public ActionCommand Start { get { return new ActionCommand(StartCalib); } }
        private void StartCalib()
        {
            if(!LeftPanelViewModel._app_running)
                return;
            if(PolePair == "" || PolePair == "0" || ContinuousCurrent == "" || ContinuousCurrent == "0" || MaxSpeed == "" || MaxSpeed == "0" || cts_Motor == "" || cts_Motor == "0")
                return;
            
            //StartEnable = false;
            #region InitVariables
            DataViewModel operation = new DataViewModel();
            Int32 commandId = 0, commandSubId = 0;
            GetInstance.Count = 0;
            #endregion InitVariables

            GetInstance.OperationList.Clear();
            for(int i = 0; i < GetInstance.CalibrationWizardList.Count; i++)
            {
                operation = new DataViewModel { CommandName = GetInstance.CalibrationWizardList.ElementAt(i).Value.CalibrationName, CommandId = "6", CommandSubId = GetInstance.CalibrationWizardList.ElementAt(i).Value.CommandSubId, IsFloat = false, CommandValue = "1" };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                if(GetInstance.CalibrationWizardList.ElementAt(i).Value.CalibrationPerform)
                    GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
            }
            if(GetInstance.OperationList.Count == 0)
            {
                //StartEnable = true;
                return;
            }
            //else
                //StartEnable = false;

            GetInstance.OperationList.Clear();

            for(int i = 0; i < GetInstance.CalibrationWizardList.Count; i++)
                GetInstance.CalibrationWizardList.ElementAt(i).Value.CalibStatus = RoundBoolLed.IDLE;

            #region BuildOperationList
            string id_fdbck_cmd_temp = "", comutation_source = "";

            switch(EncoderFeedback)
            {
                case (int)eEncSel.Enc_Fdb_Inc1:
                    id_fdbck_cmd_temp = "71";
                    if(HallEnDis == 1) // if Hall Enable
                        comutation_source = ((int)ComutationSource.Cmtn_Hall_Inc_Enc1).ToString();
                    else
                    {
                        // Show warning popup when start clicked
                    }
                    break;
                case (int)eEncSel.Enc_Fdb_Inc_Sin_Cos:
                    id_fdbck_cmd_temp = "71";
                    if(HallEnDis == 1) // if Hall Enable
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
            if(MotorType == 0)
                comutation_source = ((int)ComutationSource.Cmtn_DC_Brushed).ToString();

            int max_speed = (Convert.ToInt32(MaxSpeed) / 60) * Convert.ToInt32(cts_Motor);
            int min_seed = -max_speed;

            operation = new DataViewModel { CommandName = "Load Default", CommandId = "63", CommandSubId = "1", IsFloat = false, CommandValue = "1" };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            operation = new DataViewModel { CommandName = "Drive Mode", CommandId = "50", CommandSubId = "1", IsFloat = false, CommandValue = "2" }; // Speed Control
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            operation = new DataViewModel { CommandName = "Commutation Source", CommandId = "50", CommandSubId = "2", IsFloat = false, CommandValue = comutation_source };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            operation = new DataViewModel { CommandName = "Encoder Type", CommandId = "50", CommandSubId = "3", IsFloat = false, CommandValue = EncoderFeedback.ToString() };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

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

            if(MotorType == 1)
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

            if(id_fdbck_cmd_temp != "")
            {
                operation = new DataViewModel { CommandName = "Resolution", CommandId = id_fdbck_cmd_temp, CommandSubId = "5", IsFloat = false, CommandValue = cts_Motor };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
            }

            if(EncoderFeedback == (int)eEncSel.Enc_Fdb_Ssi)
            {
                operation = new DataViewModel { CommandName = "PacketLenght", CommandId = "73", CommandSubId = "8", IsFloat = false, CommandValue = ((Math.Log(Convert.ToInt32(cts_Motor)) / Math.Log(2)) + 1).ToString() };
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

            operation = new DataViewModel { CommandName = "Save", CommandId = "63", CommandSubId = "0", IsFloat = false, CommandValue = "1" };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            operation = new DataViewModel { CommandName = "Reset", CommandId = "63", CommandSubId = "9", IsFloat = false, CommandValue = "1" };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
            #endregion BuildOperationList
            sendPreStartOperation();
            GetInstance.OperationList.Clear();

            for(int i = 0; i < GetInstance.CalibrationWizardList.Count; i++)
            {
                operation = new DataViewModel { CommandName = GetInstance.CalibrationWizardList.ElementAt(i).Value.CalibrationName, CommandId = "6", CommandSubId = GetInstance.CalibrationWizardList.ElementAt(i).Value.CommandSubId, IsFloat = false, CommandValue = "1" };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                if(GetInstance.CalibrationWizardList.ElementAt(i).Value.CalibrationPerform)
                    GetInstance.OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
            }
            CalibrationGetStatusTask(START);
            CalibrationStart();

        }
        public ActionCommand Abort { get { return new ActionCommand(AbortCalib); } }
        private void AbortCalib()
        {
            if(StartEnable)
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

            //if(GetInstance.Count < GetInstance.OperationList.Count)
            //{
            //    Rs232Interface.GetInstance.SendToParser(new PacketFields
            //    {
            //        Data2Send = 0,
            //        ID = Convert.ToInt16(GetInstance.OperationList.ElementAt(GetInstance.Count).Value.CommandId),
            //        SubID = Convert.ToInt16(Convert.ToInt16(GetInstance.OperationList.ElementAt(GetInstance.Count).Value.CommandSubId) - 1),
            //        IsSet = true,
            //        IsFloat = false
            //    });
            //    GetInstance.CalibrationWizardList[new Tuple<int, int>(6, Convert.ToInt16(GetInstance.OperationList.ElementAt(GetInstance.Count).Value.CommandSubId))].CalibStatus = RoundBoolLed.FAILED;
            //}

            //StartEnable = true;
            CalibrationGetStatusTask(STOP);
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
                if(!value)
                    CalibrationAdvancedMode = false;
                OnPropertyChanged("AdvancedConfig");
            }
        }
        #endregion AdvancedConfiguration
        #region Tasks
        public const int START = 1;
        public const int STOP = 0;

        private Timer _calibrationGetStatus;
        const double _calibrationGetStatusInterval = 300;
        public void CalibrationGetStatusTask(int _mode)
        {
            switch(_mode)
            {
                case STOP:
                    lock(this)
                    {
                        //StartEnable = true;
                        if(_calibrationGetStatus != null)
                        {
                            lock(_calibrationGetStatus)
                            {
                                _calibrationGetStatus.Stop();
                                _calibrationGetStatus.Elapsed -= CalibrationGetStatus;
                                _calibrationGetStatus = null;
                                Thread.Sleep(10);
                                //StartEnable = true;
                            }
                        }
                    }
                    break;
                case START:
                    if(_calibrationGetStatus == null)
                    {
                        Task.Factory.StartNew(action: () =>
                        {
                            Thread.Sleep(100);
                            _calibrationGetStatus = new Timer(_calibrationGetStatusInterval) { AutoReset = true };
                            _calibrationGetStatus.Elapsed += CalibrationGetStatus;
                            _calibrationGetStatus.Start();
                        });
                    }
                    break;
            }
        }

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
        private void CalibrationGetStatus(object sender, EventArgs e)
        {
            if(GetInstance.Count < GetInstance.OperationList.Count)
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
                default:
                    StateTemp = RoundBoolLed.FAILED;
                    break;
            }
            if(GetInstance.CalibrationWizardList[new Tuple<int, int>(6, commandidentifier.Item2)].CalibStatus != StateTemp)
            {
                GetInstance.CalibrationWizardList[new Tuple<int, int>(6, commandidentifier.Item2)].CalibStatus = StateTemp;
                if(StateTemp == RoundBoolLed.FAILED || StateTemp == RoundBoolLed.PASSED)
                {
                    GetInstance.Count++;
                    CalibrationStart();
                }
            }
            if(GetInstance.Count == GetInstance.OperationList.Count)
            {
                //StartEnable = true;
                Thread.Sleep(10);
                //AbortCalib();
                //CalibrationGetStatusTask(STOP);
            }
        }
        private void sendPreStartOperation()
        {
            for(int i = 0; i < GetInstance.OperationList.Count; i++)
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
                if(GetInstance.OperationList.ElementAt(i).Value.CommandName == "Load Default" ||
                    GetInstance.OperationList.ElementAt(i).Value.CommandName == "Save" ||
                    GetInstance.OperationList.ElementAt(i).Value.CommandName == "Reset")
                    Thread.Sleep(2000);
                Thread.Sleep(10);
            }
        }
        #endregion Tasks
    }
}
