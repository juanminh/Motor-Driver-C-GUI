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
        Cmtn_Abs_Enc1 = 4
    };

    internal class WizardWindowViewModel : ViewModelBase
    {
        public Dictionary<Tuple<string>, CalibrationWizardViewModel> CalibrationWizardList = new Dictionary<Tuple<string>, CalibrationWizardViewModel>();
        public Dictionary<string, ObservableCollection<object>> CalibrationWizardListbySubGroup = new Dictionary<string, ObservableCollection<object>>();
        public Dictionary<Tuple<int, int>, DataViewModel> OperationList = new Dictionary<Tuple<int, int>, DataViewModel>();
        public bool wizardStatus = false;

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
            CalibrationWizardListbySubGroup.Add("CalibrationList", new ObservableCollection<object>());
            BuildCalibrationWizardList();
        }
        ~WizardWindowViewModel() { }
        #region Motor_Parameter
        private int _motorType = 0;
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
        private string _polePaire = "0";
        public string PolePair
        {
            get { return _polePaire; }
            set { _polePaire = value; OnPropertyChanged("PolePair"); }
        }
        private string _continuousCurrent = "0";
        public string ContinuousCurrent
        {
            get { return _continuousCurrent; }
            set { _continuousCurrent = value; OnPropertyChanged("ContinuousCurrent"); }
        }
        private string _maxSpeed = "0";
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
                if(value == 3)
                    cts_Motor = "4096";
                else
                    cts_Motor = "0";
                BuildCalibrationWizardList();
                OnPropertyChanged("EncoderFeedback");
            }
        }
        private string _cts_Motor = "0";
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
        private ObservableCollection<object> _calibList;
        public ObservableCollection<object> CalibList
        {
            get
            {
                return CalibrationWizardListbySubGroup["CalibrationList"];
            }
            set
            {
                _calibList = value;
                OnPropertyChanged();
            }

        }

        private void BuildCalibrationWizardList()
        {
            CalibrationWizardList.Clear();
            CalibrationWizardListbySubGroup["CalibrationList"].Clear();

            var names = new[]
            {
                "PI Current Loop", "Hall Mapping", "Feedback Direction", "Electrical Angle", "PI Speed Loop", "PI Position Loop"
            };
            var SubID = new[]
            {
                "3", "5", "7", "13", "9", "11"
            };
            Dictionary<string, string> calibOperation = new Dictionary<string, string>();
            for(int i = 0; i < names.Length; i++)
                calibOperation.Add(names[i], SubID[i]);

            if(MotorType == 0 || HallEnDis == 1)
                calibOperation.Remove("Hall Mapping");
            if(EncoderFeedback != 5)
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
                CalibrationWizardList.Add(new Tuple<string>(calibOperation.ElementAt(i).Key), calibElement);
                CalibrationWizardListbySubGroup["CalibrationList"].Add(calibElement);
            }
        }
        public ActionCommand Start { get { return new ActionCommand(StartCalib); } }
        private void StartCalib()
        {
            wizardStatus = true;
            OperationList.Clear();
            DataViewModel operation = new DataViewModel();
            Int32 commandId = 0, commandSubId = 0;
            #region BuildOperationList
            operation = new DataViewModel { CommandName = "Load Default", CommandId = "63", CommandSubId = "1", IsFloat = false, CommandValue = PolePair };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            operation = new DataViewModel { CommandName = "Speed Source", CommandId = "51", CommandSubId = "5", IsFloat = false, CommandValue = ((int)ClaFdb.Cla_Fdb_Motor).ToString() };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            operation = new DataViewModel { CommandName = "Position Source", CommandId = "51", CommandSubId = "6", IsFloat = false, CommandValue = ((int)ClaFdb.Cla_Fdb_Motor).ToString() };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            operation = new DataViewModel { CommandName = "External Encoder", CommandId = "50", CommandSubId = "4", IsFloat = false, CommandValue = "0" };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            operation = new DataViewModel { CommandName = "Peak Current", CommandId = "52", CommandSubId = "2", IsFloat = true, CommandValue = ContinuousCurrent };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            if(MotorType == 1)
            {
                operation = new DataViewModel { CommandName = "Pole Pair", CommandId = "51", CommandSubId = "1", IsFloat = false, CommandValue = PolePair };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
            }

            operation = new DataViewModel { CommandName = "Continuous Current", CommandId = "52", CommandSubId = "1", IsFloat = true, CommandValue = ContinuousCurrent };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            operation = new DataViewModel { CommandName = "Max Speed", CommandId = "53", CommandSubId = "2", IsFloat = false, CommandValue = MaxSpeed };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            operation = new DataViewModel { CommandName = "Hall", CommandId = "70", CommandSubId = "1", IsFloat = false, CommandValue = HallEnDis.ToString() };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            operation = new DataViewModel { CommandName = "Encoder Type", CommandId = "50", CommandSubId = "3", IsFloat = false, CommandValue = EncoderFeedback.ToString() };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            string id_fdbck_cmd_temp = "", comutation_source = "";

            switch(EncoderFeedback)
            {
                case (int)eEncSel.Enc_Fdb_Inc1:
                    id_fdbck_cmd_temp = "71";
                    comutation_source = ((int)ComutationSource.Cmtn_Enc1).ToString();
                    break;
                case (int)eEncSel.Enc_Fdb_Inc_Sin_Cos:
                    id_fdbck_cmd_temp = "71";
                    comutation_source = ((int)ComutationSource.Cmtn_Enc1).ToString();
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
            operation = new DataViewModel { CommandName = "Commutation Source", CommandId = "50", CommandSubId = "2", IsFloat = false, CommandValue = comutation_source };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            if(id_fdbck_cmd_temp != "")
            {
                operation = new DataViewModel { CommandName = "Resolution", CommandId = id_fdbck_cmd_temp, CommandSubId = "5", IsFloat = false, CommandValue = cts_Motor };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
            }

            operation = new DataViewModel { CommandName = "Save", CommandId = "63", CommandSubId = "0", IsFloat = false, CommandValue = "1" };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);

            operation = new DataViewModel { CommandName = "Reset", CommandId = "63", CommandSubId = "9", IsFloat = false, CommandValue = "1" };
            Int32.TryParse(operation.CommandId, out commandId);
            Int32.TryParse(operation.CommandSubId, out commandSubId);
            OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
            #endregion BuildOperationList
            sendPreStartOperation();
            OperationList.Clear();

            /*
            for(int i = 0; i < CalibList.Count; i++)
            {
                operation = new DataViewModel { CommandName = CalibrationWizardList.ElementAt(i).Key.ToString(), CommandId = "6", CommandSubId = CalibrationWizardList.ElementAt(i).Value.ToString(), IsFloat = false, CommandValue = "1" };
                Int32.TryParse(operation.CommandId, out commandId);
                Int32.TryParse(operation.CommandSubId, out commandSubId);
                OperationList.Add(new Tuple<int, int>(commandId, commandSubId), operation);
            }
            CalibrationStart();
            */
        }
        public ActionCommand Abort { get { return new ActionCommand(AbortCalib); } }
        private void AbortCalib()
        {
            Rs232Interface.GetInstance.SendToParser(new PacketFields
            {
                Data2Send = 0,
                ID = 1,
                SubID = 0,
                IsSet = true,
                IsFloat = false
            });

            Rs232Interface.GetInstance.SendToParser(new PacketFields
            {
                Data2Send = 0,
                ID = Convert.ToInt16(OperationList.ElementAt(Count).Value.CommandId),
                SubID = Convert.ToInt16(OperationList.ElementAt(Count).Value.CommandSubId),
                IsSet = true,
                IsFloat = false
            });
            CalibrationGetStatusTask(STOP);
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
                        if(_calibrationGetStatus != null)
                        {
                            lock(_calibrationGetStatus)
                            {
                                _calibrationGetStatus.Stop();
                                _calibrationGetStatus.Elapsed -= CalibrationGetStatus;
                                _calibrationGetStatus = null;
                                Thread.Sleep(10);
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
            Rs232Interface.GetInstance.SendToParser(new PacketFields
            {
                Data2Send = 1,
                ID = Convert.ToInt16(OperationList.ElementAt(Count).Value.CommandId),
                SubID = Convert.ToInt16(OperationList.ElementAt(Count).Value.CommandSubId),
                IsSet = true,
                IsFloat = false
            });
            CalibrationWizardList.ElementAt(Count).Value.CalibStatus = RoundBoolLed.RUNNING;
            CalibrationGetStatusTask(START);
        }
        private void CalibrationGetStatus(object sender, EventArgs e)
        {
            Rs232Interface.GetInstance.SendToParser(new PacketFields
            {
                Data2Send = 1,
                ID = Convert.ToInt16(OperationList.ElementAt(Count).Value.CommandId),
                SubID = Convert.ToInt16(OperationList.ElementAt(Count).Value.CommandSubId + 1),
                IsSet = false,
                IsFloat = false
            });
        }
        public void updateCalibrationStatus(Tuple<int, int> commandidentifier, string newPropertyValue, bool IntFloat = false)
        {
            switch(Convert.ToInt16(newPropertyValue))
            {
                case 0:
                    CalibrationWizardList.ElementAt(Count).Value.CalibStatus = RoundBoolLed.IDLE;
                    break;
                case 1:
                    CalibrationWizardList.ElementAt(Count).Value.CalibStatus = RoundBoolLed.RUNNING;
                    break;
                case 2:
                    CalibrationWizardList.ElementAt(Count).Value.CalibStatus = RoundBoolLed.FAILED;
                    Count++;
                    CalibrationStart();
                    break;
                case 3:
                    CalibrationWizardList.ElementAt(Count).Value.CalibStatus = RoundBoolLed.PASSED;
                    Count++;
                    CalibrationStart();
                    break;
                default:
                    CalibrationWizardList.ElementAt(Count).Value.CalibStatus = RoundBoolLed.FAILED;
                    Count++;
                    CalibrationStart();
                    break;
            }
            if(Count == OperationList.Count)
            {
                CalibrationGetStatusTask(STOP);
                return;
            }
        }
        private void sendPreStartOperation()
        {
            for(int i = 0; i < OperationList.Count; i++)
            {
                Rs232Interface.GetInstance.SendToParser(new PacketFields
                {
                    Data2Send = OperationList.ElementAt(i).Value.CommandValue,
                    ID = Convert.ToInt16(OperationList.ElementAt(i).Value.CommandId),
                    SubID = Convert.ToInt16(OperationList.ElementAt(i).Value.CommandSubId),
                    IsSet = true,
                    IsFloat = OperationList.ElementAt(i).Value.IsFloat
                });
                if(OperationList.ElementAt(i).Value.CommandName == "Load Default" ||
                    OperationList.ElementAt(i).Value.CommandName == "Save" ||
                    OperationList.ElementAt(i).Value.CommandName == "Reset")
                    Thread.Sleep(500);
                Thread.Sleep(10);
            }
        }
        #endregion Tasks
    }
}
