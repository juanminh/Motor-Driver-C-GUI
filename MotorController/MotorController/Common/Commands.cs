using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using MotorController.ViewModels;
using System.Windows.Media;
using System.Linq;
using MotorController.Helpers;
using Abt.Controls.SciChart;
using System.Collections;

namespace MotorController.Common
{
    enum eDeviceInfo
    {
        ID = 62,
        Serial_Number = 1,
        HW_Rev = 2,
        FW_Rev = 3,
        CanNode_ID = 8
    };
    enum eBaudRate
    {
        _4800 = 0,
        _9600 = 1,
        _19200 = 2,
        _38400 = 3,
        _57600 = 4,
        _115200 = 5,
        _230400 = 6,
        _460800 = 7,
        _921600 = 8
    };
    class Commands
    {
        int DataViewModel_FontSize = 12;
        const bool INT = false;
        const bool FLOAT = true;

        private Commands()
        {
            GenericCommandsGroup.Clear();
            GenericCommandsList.Clear();
            Enums.Clear();
            ErrorList.Clear();
            BuildAllCommandsGroup();
        }
        public void BuildAllCommandsGroup()
        {
            GenerateLPCommands();
            GenerateBPCommands();
            GenerateMotionCommands();
            GenerateMotionTabCommands();
            GenerateFeedBakcCommands();
            GeneratePidCommands();
            GenerateFilterCommands();
            GenerateDeviceCommands();
            GenerateIOTabCommands();
            CalibrationCmd();
            GenerateBodeCommands();
            GenerateMaintenanceList();
            GenerateDebugListCommands();
            GenerateCHList();
            BuildErrorList();

            GenerateToggleSwitchCommands();
        }
        static public void AssemblePacket(out PacketFields rxPacket, Int16 id, Int16 subId, bool isSet, bool isFloat, object data2Send)
        {
            rxPacket.ID = id;
            rxPacket.IsFloat = isFloat;
            rxPacket.IsSet = isSet;
            rxPacket.SubID = subId;
            rxPacket.Data2Send = data2Send;
        }

        #region Init_Dictionary
        public Dictionary<Tuple<int, int>, object> GenericCommandsList = new Dictionary<Tuple<int, int>, object>();
        public Dictionary<string, ObservableCollection<object>> GenericCommandsGroup = new Dictionary<string, ObservableCollection<object>>();
        
        public Dictionary<string, List<string>> Enums = new Dictionary<string, List<string>>();
        
        public Dictionary<int, string> ErrorList = new Dictionary<int, string>();

        #endregion Init_Dictionary

        private static readonly object Synlock = new object(); //Single tone variable
        private static Commands _instance;
        public static Commands GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new Commands();
                    return _instance;
                }
            }
            set
            {
                lock(Synlock)
                    _instance = value;
            }
        }

        DataViewModel Data = new DataViewModel();
        EnumViewModel eData = new EnumViewModel();
        UC_ToggleSwitchViewModel ToggleSwitchData = new UC_ToggleSwitchViewModel();
        CalibrationWizardViewModel CalibWizardData = new CalibrationWizardViewModel();
        BoolViewIndModel BoolData = new BoolViewIndModel();
        UC_ChannelViewModel Channel = new UC_ChannelViewModel();

        private void GenerateDeviceCommands()
        {
            for(int i = 0; i < Enum.GetNames(typeof(eDeviceInfo)).Length - 1; i++)
            {
                Data = new DataViewModel
                {
                    CommandName = EnumHelper.GetNames(Enum.GetNames(typeof(eDeviceInfo))).ElementAt(i),
                    CommandId = ((int)eDeviceInfo.ID).ToString(),//((int)EnumHelper.GetEnumValue<eDeviceInfo>(Enum.GetNames(typeof(eDeviceInfo)).ElementAt(0))).ToString(),
                    CommandSubId = ((int)EnumHelper.GetEnumValue<eDeviceInfo>(Enum.GetNames(typeof(eDeviceInfo)).ElementAt(i))).ToString(),
                    IsFloat = false,
                    FontSize = DataViewModel_FontSize,
                    ReadOnly = true
                };
                addData(typeof(DataViewModel), Data, "DeviceSerial");
                
                if(i < 3)
                    addData(typeof(DataViewModel), Data, "LPCommands List");
            }

            //Data = new DataViewModel
            //{
            //    CommandName = "FW Rev",
            //    CommandId = "62",
            //    CommandSubId = "3",
            //    CommandValue = "",
            //    IsFloat = false,
            //    FontSize = DataViewModel_FontSize,
            //    ReadOnly = true
            //};
            //addData(typeof(DataViewModel), Data, "LPCommands List");

            Enums.Add("Baudrate", EnumHelper.GetNames(Enum.GetNames(typeof(eBaudRate))).ToList());

            eData = new EnumViewModel
            {
                CommandName = "Baudrate",
                CommandId = "61",
                CommandSubId = "1",
                CommandList = EnumHelper.GetNames(Enum.GetNames(typeof(eBaudRate))).ToList(),
                CommandValue = ((int)EnumHelper.GetEnumValue<eBaudRate>(Enum.GetNames(typeof(eBaudRate)).ElementAt(0))).ToString(),//first enum in list
            };
            addData(typeof(EnumViewModel), eData, "BaudrateList");
            
            #region Synch Command cmdID 64 cmdSubID 0
            
            Data = new DataViewModel
            {
                CommandId = "64",
                CommandSubId = "0",
                CommandName = "PlotCommand",
                IsFloat = false,
                CommandValue = "0"
            };
            addData(typeof(DataViewModel), Data, "DeviceSynchCommand");
           
            Data = new DataViewModel
            {
                CommandId = "64",
                CommandSubId = "1",
                CommandName = "AutoBaud_Sync",
                IsFloat = false,
                CommandValue = "0"
            };
            addData(typeof(DataViewModel), Data, "DeviceSynchCommand");
            
            Data = new DataViewModel
            {
                CommandId = "61",
                CommandSubId = "1",
                CommandName = "AutoBaud_Baudrate",
                IsFloat = false,
                CommandValue = "0"
            };
            addData(typeof(DataViewModel), Data, "DeviceSynchCommand");
            #endregion
        }
        private void GeneratePidCommands()
        {
            var names = new[]
            {
                "Kp", "Ki", "Kc", "Kd", "kp range", "Range"
            };
            string[] index = new[]
            {
                "1", "2", "3", "4", "5", "6"
            };

            for(int i = 0; i < names.Length; i++)
            {
                Data = new DataViewModel
                {
                    CommandName = names[i],
                    CommandId = "82",
                    CommandSubId = index[i],
                    CommandValue = "",
                    IsFloat = names[i] == "Range" ? false : true
                };
                addData(typeof(DataViewModel), Data, "PIDSpeed");
                
                Data = new DataViewModel
                {
                    CommandName = names[i],
                    CommandId = "83",
                    CommandSubId = index[i],
                    CommandValue = "",
                    IsFloat = names[i] == "Range" ? false : true
                };
                addData(typeof(DataViewModel), Data, "PIDPosition");
            }

            names = new[]
            {
                "Kp", "Ki"
            };

            for(int i = 0; i < names.Length; i++)
            {
                Data = new DataViewModel
                {
                    CommandName = names[i],
                    CommandId = "81",
                    CommandSubId = (i + 1).ToString(CultureInfo.InvariantCulture),
                    CommandValue = "",
                    IsFloat = true
                };
                addData(typeof(DataViewModel), Data, "PIDCurrent");
            }
        }
        private void GenerateFilterCommands()
        {
            var names = new[]
            {
                "Number of sections", "section 0 a[0]", "section 0 a[1]", "section 0 a[2]",
                "section 0 b[0]", "section 0 b[1]", "section 0 b[2]"
            };

            for(int i = 0; i < names.Length; i++)
            {
                Data = new DataViewModel
                {
                    CommandName = names[i],
                    CommandId = "101",
                    CommandSubId = (i + 1).ToString(CultureInfo.InvariantCulture),
                    CommandValue = "",
                    IsFloat = i == 0 ? false : true
                };
                addData(typeof(DataViewModel), Data, "FilterList");
            }
            Data = new DataViewModel
            {
                CommandName = "Enable",
                CommandId = "101",
                CommandSubId = (0).ToString(CultureInfo.InvariantCulture),
                CommandValue = "",
                IsFloat = false
            };
        }
        private void GenerateFeedBakcCommands()
        {
            #region Hall
            var names = new[]
            {
                "Enable", "Roll High", "Roll Low", "Direction", "Counts Per Rev", "Speed LPF Cut-Off"
            };

            for(var i = 1; i < names.Length; i++)
            {
                Data = new DataViewModel
                {
                    CommandName = names[i],
                    CommandId = "70",
                    CommandSubId = (i + 1).ToString(CultureInfo.InvariantCulture),
                    CommandValue = "",
                    IsFloat = names[i] == "Speed LPF Cut-Off"
                };
                addData(typeof(DataViewModel), Data, "Hall");
            }

            for(int i = 1; i < 5; i++)
            {
                Data = new DataViewModel
                {
                    CommandName = "Hall Angle " + i.ToString(),
                    CommandId = "70",
                    CommandSubId = (i + 6).ToString(CultureInfo.InvariantCulture),
                    CommandValue = "",
                    IsFloat = false
                };
                addData(typeof(DataViewModel), Data, "Hall");
            }

            for(int i = 0; i < 6; i++)
            {
                Data = new DataViewModel
                {
                    CommandName = "Hall Map " + i.ToString(),
                    CommandId = "84",
                    CommandSubId = (i + 1).ToString(CultureInfo.InvariantCulture),
                    CommandValue = "",
                    IsFloat = false
                };
                addData(typeof(DataViewModel), Data, "Hall");
            }

            Data = new DataViewModel
            {
                CommandName = "Sample Period",
                CommandId = "70",
                CommandSubId = "15",
                CommandValue = "",
                IsFloat = false
            };
            addData(typeof(DataViewModel), Data, "Hall");
            
            #endregion Hall
            #region  FeedbackSyncList
            Data = new DataViewModel
            {
                CommandName = "Interpolation Gear",
                CommandId = "78",
                CommandSubId = "2",
                CommandValue = "",
                IsFloat = true
            };
            addData(typeof(DataViewModel), Data, "FeedbackSync");
            
            Data = new DataViewModel
            {
                CommandName = "Encoder-Hall Sync [C]",
                CommandId = "78",
                CommandSubId = "3",
                CommandValue = "",
                IsFloat = true
            };
            addData(typeof(DataViewModel), Data, "FeedbackSync");
            
            #endregion
            #region SSI

            names = new[]
            {
                "Enable", "Roll High", "Roll Low", "Direction", "LPF Cut-Off", "Baud rate", "Encoder Bits", "Clock Phase", "Clock Polarity", "Tail Bits", "Packet Delay", "Calibrated Angle", "Head Bits", "Sample Period"
            };
            var SubId = new[] { "1", "2", "3", "4", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" };
            var type = new[] { false, false, false, false, true, false, false, false, false, false, false, true, false, false };

            for(int i = 1; i < names.Length; i++)
            {
                Data = new DataViewModel
                {
                    CommandName = names[i],
                    CommandId = "73",
                    CommandSubId = SubId[i],
                    CommandValue = "",
                    IsFloat = type[i]
                };
                addData(typeof(DataViewModel), Data, "SSI_Feedback");
            }
            #endregion SSI
            #region EncderInput_1/2
            names = new[]
            {
                "Enable", "Roll High", "Roll Low", "Direction", "Counts Per Revolution",
                "Speed LPF", "Index Mode", "Reset Value", "Set Position Value"
            };
            bool[] IsFloat = new[] { false, false, false, false, false, true, false, false, false };
            for(int i = 1, k = 2; i < names.Length; i++, k++)
            {
                Data = new DataViewModel
                {
                    CommandName = names[i],
                    CommandId = "71",
                    CommandSubId = k.ToString(CultureInfo.InvariantCulture),
                    CommandValue = "",
                    IsFloat = IsFloat[i],
                };
                addData(typeof(DataViewModel), Data, "Qep1");

                Data = new DataViewModel
                {
                    CommandName = names[i],
                    CommandId = "72",
                    CommandSubId = k.ToString(CultureInfo.InvariantCulture),
                    CommandValue = "",
                    IsFloat = IsFloat[i],
                };
                addData(typeof(DataViewModel), Data, "Qep2");

                if(k == 7)
                    k = 8;
                if(k == 9)
                    k = 12;
            }

            Data = new DataViewModel
            {
                CommandName = "Resolution Sin/Cos",
                CommandId = "71",
                CommandSubId = 14.ToString(CultureInfo.InvariantCulture),
                CommandValue = "",
                IsFloat = false,
            };
            addData(typeof(DataViewModel), Data, "Qep1");

            var tmp1 = new List<string>
              {
                  "Index Disabled",
                  "One Shot",
                  "Continuous Refresh"
              };
            Enums.Add("Index Reset", tmp1);

            eData = new EnumViewModel
            {
                CommandName = "Index Reset",
                CommandId = "71",
                CommandSubId = "8",
                CommandList = Enums["Index Reset"],
                CommandValue = "1",//first enum in list
            };
            addData(typeof(EnumViewModel), eData, "Qep1Bis");
            
            eData = new EnumViewModel
            {
                CommandName = "Index Reset",
                CommandId = "72",
                CommandSubId = "8",
                CommandList = Enums["Index Reset"],
                CommandValue = "1",//first enum in list
            };
            addData(typeof(EnumViewModel), eData, "Qep2Bis");
            
            #endregion  Qep1Qep2
        }
        public void GenerateMotionCommands()
        {
            var tmp1 = new List<string>
              {
                  "Current Control",
                  "Speed Control",
                  "Position Control"
              };
            Enums.Add("Drive Mode", tmp1);

            eData = new EnumViewModel
            {
                CommandName = "Drive Mode",
                CommandId = "50",
                CommandSubId = "1",
                CommandList = Enums["Drive Mode"],
                CommandValue = "1",//first enum in list,
                FontSize = DataViewModel_FontSize

            };
            addData(typeof(EnumViewModel), eData, "Control");
            
            var tmp2 = new List<string>
             {
                "Brushed",
                "BL with Hall",
                "BL with Hall and Inc. encoder",
                "BL with Abs. encoder",
                "BL Stepper Sensorless"
             };
            Enums.Add("Electrical Commutation Type", tmp2);

            eData = new EnumViewModel
            {
                CommandName = "Electrical Commutation Type",
                CommandId = "50",
                CommandSubId = "2",
                CommandValue = "0", //first enum in list
                CommandList = Enums["Electrical Commutation Type"],
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(EnumViewModel), eData, "Control");

            tmp2 = new List<string>
             {
                "Disable",
                "Enable"
             };
            Enums.Add("Motor Hall", tmp2);

            eData = new EnumViewModel
            {
                CommandName = "Motor Hall",
                CommandId = "70",
                CommandSubId = "1",
                CommandValue = "0", //first enum in list
                CommandList = Enums["Motor Hall"],
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(EnumViewModel), eData, "Control");

            var tmp6 = new List<string>
             {
                "None",
                "Incremental 1",
                "Incremental Sin Cos",
                "Absolute Sin Cos",
                "Incremental 2",
                "SSI",
                "Resolver",
                "Com"
            };

            Enums.Add("Motor encoder", tmp6);
            eData = new EnumViewModel
            {
                CommandName = "Motor encoder",
                CommandId = "50",
                CommandSubId = "3",
                CommandValue = "0", //first enum in list
                CommandList = Enums["Motor encoder"],
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(EnumViewModel), eData, "Control");

            Enums.Add("External encoder", tmp6);
            eData = new EnumViewModel
            {
                CommandName = "External encoder",
                CommandId = "50",
                CommandSubId = "4",
                CommandValue = "0", //first enum in list
                CommandList = Enums["External encoder"],
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(EnumViewModel), eData, "Control");

            var tmp5 = new List<string>
             {
                "Digital Cmd",
                "Analog Cmd",
                "PWM Cmd",
                "Buffer Cmd",
                "Spi Cmd",
                "Signal gen Cmd"
            };
            Enums.Add("Command Source", tmp5);
            eData = new EnumViewModel
            {
                CommandName = "Command Source",
                CommandId = "50",
                CommandSubId = "5",
                CommandValue = "1", //first enum in list
                CommandList = Enums["Command Source"],
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(EnumViewModel), eData, "Control");

            var tmp3 = new List<string>
             {
                "Hall",
                "Motor",
                "External"
             };

            Enums.Add("Speed loop Fdb", tmp3);
            eData = new EnumViewModel
            {
                CommandName = "Speed loop Fdb",
                CommandId = "50",
                CommandSubId = "6",
                CommandValue = "1", //first enum in list
                CommandList = Enums["Speed loop Fdb"],
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(EnumViewModel), eData, "Control");

            Enums.Add("Position loop Fdb", tmp3);
            eData = new EnumViewModel
            {
                CommandName = "Position loop Fdb",
                CommandId = "50",
                CommandSubId = "7",
                CommandValue = "1", //first enum in list
                CommandList = Enums["Position loop Fdb"],
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(EnumViewModel), eData, "Control");

            Data = new DataViewModel
            {
                CommandName = "Pole Pair",
                CommandId = "51",
                CommandSubId = "1",
                CommandValue = "",
                IsFloat = false,
            };
            addData(typeof(DataViewModel), Data, "Motor");

            Data = new DataViewModel
            {
                CommandName = "Direction",
                CommandId = "51",
                CommandSubId = "2",
                CommandValue = "",
                IsFloat = false,
            };
            addData(typeof(DataViewModel), Data, "Motor");

            string[] commandName = { "Max speed [C/S]", "Min Speed [C/S]", "Max position [C]", "Min position [C]", "Enable Position Limit", "Motor stuck current", "Motor stuck speed", "Motor stuck Duration" };
            bool[] Type = { INT, INT, INT, INT, INT, FLOAT, INT, FLOAT };

            for(int i = 0; i < commandName.Length; i++)
            {
                Data = new DataViewModel
                {
                    CommandName = commandName[i],
                    CommandId = "53",
                    CommandSubId = (i + 1).ToString(),
                    CommandValue = "",
                    IsFloat = Type[i]
                };
                addData(typeof(DataViewModel), Data, "Motion Limit");
            }
        }
        public void GenerateBPCommands()
        {
            #region Commands1
            Data = new DataViewModel
            {
                CommandName = "Current [A]",
                CommandId = "3",
                CommandSubId = "0",
                CommandValue = "",
                IsFloat = true,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(DataViewModel), Data, "MotionCommand List");

            Data = new DataViewModel
            {
                CommandName = "Speed [C/S]",
                CommandId = "4",
                CommandSubId = "0",
                CommandValue = "",
                IsFloat = false,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(DataViewModel), Data, "MotionCommand List");

            Data = new DataViewModel
            {
                CommandName = "RPM",
                CommandId = "4",
                CommandSubId = "10",
                CommandValue = "",
                IsFloat = false,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(DataViewModel), Data, "MotionCommand List");

            Data = new DataViewModel
            {
                CommandName = "Speed Position [C/S]",
                CommandId = "5",
                CommandSubId = "2",
                CommandValue = "",
                IsFloat = false,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(DataViewModel), Data, "MotionCommand List");

            Data = new DataViewModel
            {
                CommandName = "Position Absolute [C]",
                CommandId = "5",
                CommandSubId = "0",
                CommandValue = "",
                IsFloat = false,
                IsSelected = false,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(DataViewModel), Data, "MotionCommand List");

            Data = new DataViewModel
            {
                CommandName = "Position Relative [C]",
                CommandId = "5",
                CommandSubId = "1",
                CommandValue = "",
                IsFloat = false,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(DataViewModel), Data, "MotionCommand List");

            Data = new DataViewModel
            {
                CommandName = "Accelaration [C/S^2]",
                CommandId = "54",
                CommandSubId = "3",
                CommandValue = "",
                IsFloat = false,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(DataViewModel), Data, "MotionCommand List2");
            
            Data = new DataViewModel
            {
                CommandName = "PTP Speed [C/S]",
                CommandId = "54",
                CommandSubId = "2",
                CommandValue = "",
                IsFloat = false,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(DataViewModel), Data, "MotionCommand List2");

            Data = new DataViewModel
            {
                CommandName = "Max Tracking Err [C]",
                CommandId = "54",
                CommandSubId = "6",
                CommandValue = "",
                IsFloat = false,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(DataViewModel), Data, "MotionCommand List2");

            #endregion Commands1
            #region Commands2
            var ProfilerModeEnum = new List<string>
              {
                  "PID",
                  "Trapezoid"
              };
            Enums.Add("Profiler Mode", ProfilerModeEnum);

            eData = new EnumViewModel
            {
                CommandName = "Profiler Mode",
                CommandId = "54",
                CommandSubId = "1",
                CommandList = Enums["Profiler Mode"],
                CommandValue = "1",//first enum in list
                FontSize = DataViewModel_FontSize - 1
            };
            addData(typeof(EnumViewModel), eData, "Profiler Mode");
            #endregion Commands2
            #region Commands3
            var SignalgeneratorTypeEnum = new List<string>
              {
                "GenDisabled",
                "RampUpDown",
                "SquareWave",
                "SinWave"
            };
            Enums.Add("S.G.Type", SignalgeneratorTypeEnum);

            eData = new EnumViewModel
            {
                CommandName = "Type",
                CommandId = "7",
                CommandSubId = "1",
                CommandList = Enums["S.G.Type"],
                CommandValue = "1",//first enum in list start at 0
                FontSize = DataViewModel_FontSize - 1
            };
            addData(typeof(EnumViewModel), eData, "S.G.Type");
            
            #endregion Commands3
            #region Commands4

            Data = new DataViewModel
            {
                CommandName = "Offset",
                CommandId = "7",
                CommandSubId = "5",
                CommandValue = "",
                IsFloat = false,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(DataViewModel), Data, "S.G.List");

            Data = new DataViewModel
            {
                CommandName = "Frequency [Hz]",
                CommandId = "7",
                CommandSubId = "6",
                CommandValue = "",
                IsFloat = true,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(DataViewModel), Data, "S.G.List");

            #endregion Commands4
            #region Status_1

            Data = new DataViewModel
            {
                CommandName = "PWM %",
                CommandId = "30",
                CommandSubId = "2",
                CommandValue = "",
                IsFloat = true,
                ReadOnly = true,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(DataViewModel), Data, "MotionStatus List");

            Data = new DataViewModel
            {
                CommandName = "Speed Fdb",
                CommandId = "25",
                CommandSubId = "0",
                CommandValue = "",
                IsFloat = false,
                ReadOnly = true,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(DataViewModel), Data, "MotionStatus List");

            Data = new DataViewModel
            {
                CommandName = "RPM",
                CommandId = "25",
                CommandSubId = "10",
                CommandValue = "",
                IsFloat = false,
                ReadOnly = true,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(DataViewModel), Data, "MotionStatus List");

            Data = new DataViewModel
            {
                CommandName = "IQ Current [A]",
                CommandId = "30",
                CommandSubId = "0",
                CommandValue = "",
                IsFloat = true,
                ReadOnly = true,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(DataViewModel), Data, "MotionStatus List");

            Data = new DataViewModel
            {
                CommandName = "ID Current [A]",
                CommandId = "30",
                CommandSubId = "1",
                CommandValue = "",
                IsFloat = true,
                ReadOnly = true,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(DataViewModel), Data, "MotionStatus List");

            Data = new DataViewModel
            {
                CommandName = "Ia",
                CommandId = "30",
                CommandSubId = "10",
                CommandValue = "",
                IsFloat = true,
                ReadOnly = true,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(DataViewModel), Data, "MotionStatus List");

            Data = new DataViewModel
            {
                CommandName = "Ib",
                CommandId = "30",
                CommandSubId = "11",
                CommandValue = "",
                IsFloat = true,
                ReadOnly = true,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(DataViewModel), Data, "MotionStatus List");

            Data = new DataViewModel
            {
                CommandName = "Ic",
                CommandId = "30",
                CommandSubId = "12",
                CommandValue = "",
                IsFloat = true,
                ReadOnly = true,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(DataViewModel), Data, "MotionStatus List");

            Data = new DataViewModel
            {
                CommandName = "Temperature [C]",
                CommandId = "32",
                CommandSubId = "1",
                CommandValue = "",
                IsFloat = true,
                ReadOnly = true,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(DataViewModel), Data, "MotionStatus List");
            
            #endregion Status_1
            #region Status2

            var names = new[]
            {
                "Input 1", "Input 2", "Input 3", "Input 4"
            };
            for(int i = 1; i < 5; i++)
            {
                BoolData = new BoolViewIndModel
                {
                    CommandName = names[i - 1],
                    CommandValue = 0,
                    CommandId = "29",
                    CommandSubId = i.ToString(),
                    IsFloat = false
                };
                addData(typeof(BoolViewIndModel), BoolData, "Digital Input List");
            }
            #endregion Status2
            #region Status_3
            Data = new DataViewModel
            {
                CommandName = "Main",
                CommandId = "26",
                CommandSubId = "0",
                CommandValue = "",
                IsFloat = false,
                ReadOnly = true,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(DataViewModel), Data, "Position counters List");

            Data = new DataViewModel
            {
                CommandName = "Hall",
                CommandId = "26",
                CommandSubId = "1",
                CommandValue = "",
                IsFloat = false,
                ReadOnly = true,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(DataViewModel), Data, "Position counters List");

            Data = new DataViewModel
            {
                CommandName = "Motor",
                CommandId = "26",
                CommandSubId = "2",
                CommandValue = "",
                IsFloat = false,
                ReadOnly = true,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(DataViewModel), Data, "Position counters List");

            Data = new DataViewModel
            {
                CommandName = "External",
                CommandId = "26",
                CommandSubId = "3",
                CommandValue = "",
                IsFloat = false,
                ReadOnly = true,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(DataViewModel), Data, "Position counters List");
            #endregion Status_3
        }
        public void GenerateLPCommands()
        {
            #region Commands1
            Data = new DataViewModel
            {
                CommandName = "SN",
                CommandId = "62",
                CommandSubId = "1",
                CommandValue = "",
                IsFloat = false
            };
            //addData(typeof(DataViewModel), Data, "LPCommands List");

            //DataViewCommandsListLP.Add(new Tuple<int, int>(62, 1), data);
            //if(!GenericCommandsList.ContainsKey(new Tuple<int, int>(Convert.ToInt16(Data.CommandId), Convert.ToInt16(Data.CommandSubId))))
            //    GenericCommandsList.Add(new Tuple<int, int>(Convert.ToInt16(Data.CommandId), Convert.ToInt16(Data.CommandSubId)), Data);
            //DataCommandsListbySubGroup["LPCommands List"].Add(data);

            Data = new DataViewModel
            {
                CommandName = "HW Rev",
                CommandId = "62",
                CommandSubId = "2",
                CommandValue = "",
                IsFloat = false,
            };
            //addData(typeof(DataViewModel), Data, "LPCommands List");

            //DataViewCommandsListLP.Add(new Tuple<int, int>(62, 2), data);
            //if(!GenericCommandsList.ContainsKey(new Tuple<int, int>(Convert.ToInt16(Data.CommandId), Convert.ToInt16(Data.CommandSubId))))
            //    GenericCommandsList.Add(new Tuple<int, int>(Convert.ToInt16(Data.CommandId), Convert.ToInt16(Data.CommandSubId)), Data);
            //DataCommandsListbySubGroup["LPCommands List"].Add(data);

            

            //DataViewCommandsListLP.Add(new Tuple<int, int>(62, 3), data);
            //DataViewCommandsList.Add(new Tuple<int, int>(62, 3), data);
            //if(!GenericCommandsList.ContainsKey(new Tuple<int, int>(Convert.ToInt16(Data.CommandId), Convert.ToInt16(Data.CommandSubId))))
            //    GenericCommandsList.Add(new Tuple<int, int>(Convert.ToInt16(Data.CommandId), Convert.ToInt16(Data.CommandSubId)), Data);
            //if(!DataCommandsListbySubGroup.ContainsKey("LPCommands List"))
            //    DataCommandsListbySubGroup.Add("LPCommands List", new ObservableCollection<object>());

            //DataCommandsListbySubGroup["LPCommands List"].Add(Data);
            #endregion Commands1

            #region Command3
            Data = new DataViewModel
            {
                CommandName = "Driver Status",
                CommandId = "33",
                CommandSubId = "1",
                CommandValue = "",
                IsFloat = false,
                ReadOnly = true
            };
            addData(typeof(DataViewModel), Data, "DriverStatus List");
            
            #endregion Command3
        }
        private void GenerateMotionTabCommands()
        {
            var names = new[]
            {
                "Continuous Current Limit [A]", "Peak Current Limit [A]", "Peak Time [sec]", "PWM limit [%]"
            };

            for(int i = 0; i < names.Length; i++)
            {
                Data = new DataViewModel
                {
                    CommandName = names[i],
                    CommandId = "52",
                    CommandSubId = (i + 1).ToString(CultureInfo.InvariantCulture),
                    CommandValue = "",
                    IsFloat = true,
                };
                addData(typeof(DataViewModel), Data, "CurrentLimit List");
            }
        }
        private void GenerateIOTabCommands()
        {
            var names = new[]
            {
                "Ampere/Volt", "RPM/Volt", "Counts/Volt", "Offset", "Dead Zone", "Direction", "LPF Cut-Off"
            };
            for(int i = 0; i < names.Length; i++)
            {
                Data = new DataViewModel
                {
                    CommandName = names[i],
                    CommandId = "110",
                    CommandSubId = (i).ToString(CultureInfo.InvariantCulture),
                    CommandValue = "",
                    IsFloat = i == 5 ? false : true,
                };
                addData(typeof(DataViewModel), Data, "AnalogCommand List");
            }
        }
        private void GenerateBodeCommands()
        {
            string[] names = { "Control Loop", "Frequency Start", "Frequency End", "Amplitude", "PointsDec" };
            bool[] type = { false, true, true, true, false, false };

            for(int i = 1; i < names.Length; i++)
            {
                Data = new DataViewModel
                {
                    CommandName = names[i],
                    CommandId = "15",
                    CommandSubId = (i + 1).ToString(CultureInfo.InvariantCulture),
                    CommandValue = "",
                    IsFloat = type[i]
                };
                addData(typeof(DataViewModel), Data, "DataBodeList");
            }

            Data = new DataViewModel
            {
                CommandName = "Status",
                CommandId = "6",
                CommandSubId = (15).ToString(CultureInfo.InvariantCulture),
                CommandValue = "",
                IsFloat = false
            };

            var tmp1 = new List<string>
              {
                  "Current Control",
                  "Speed Control",
                  "Position Control",
                  "Position Speed Control",
                  "ST Position Time Control",
                  "ST Position Control"
            };
            Enums.Add("Control Loop", tmp1);

            eData = new EnumViewModel
            {
                CommandName = "Control Loop",
                CommandId = "15",
                CommandSubId = "1",
                CommandList = Enums["Control Loop"],
                CommandValue = "1",//first enum in list,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(EnumViewModel), eData, "EnumBodeList");

            tmp1 = new List<string>
              {
                "Bode Current",
                "Bode Speed",
                "Bode Position"
            };
            Enums.Add("Bode Fdbck", tmp1);

            eData = new EnumViewModel
            {
                CommandName = "Bode Fdbck",
                CommandId = "15",
                CommandSubId = "6",
                CommandList = Enums["Bode Fdbck"],
                CommandValue = "0",//first enum in list,
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(EnumViewModel), eData, "EnumBodeList");
        }
        private void GenerateMaintenanceList()
        {
            Data = new DataViewModel
            {
                CommandName = "Flash Checksum",
                CommandId = "63",
                CommandSubId = "11",
                CommandValue = "",
                IsFloat = false,
            };
            var names = new[] { "Save", "Load Manufacture defaults" }; //, "Reboot Driver", "Enable Protected Write", "Enable Loader"};
            var ID = new[] { "63", "63" }; //, "63", "63", "65" };
            var subID = new[] { "0", "1" }; //, "2", "10", "0" };
            for(int i = 0; i < names.Length; i++)
            {
                Data = new DataViewModel
                {
                    CommandName = names[i],
                    CommandId = ID[i],
                    CommandSubId = subID[i],
                    CommandValue = "",
                    IsFloat = true,
                };
            }
        }
        private void UpperMainPannelList()
        {
            Data = new DataViewModel
            {
                CommandName = "CH1",
                CommandId = "60",
                CommandSubId = "1",
                CommandValue = "",
                IsFloat = false,
            };

            Data = new DataViewModel
            {
                CommandName = "CH2",
                CommandId = "60",
                CommandSubId = "2",
                CommandValue = "",
                IsFloat = false,
            };
        }
        private void CalibrationCmd()
        {
            var names = new[]
            {
                "Current Offset", "PI Current Loop", "Hall Mapping", "Feedback Direction", "PI Speed Loop", "PI Position Loop", "Abs. Enc."
            };
            int[] CalibTimeout = new int[] { 10, 10, 70, 15, 60, 60, 60 };

            for(int i = 0; i < names.Length; i++) // Calibration Button
            {
                #region CalibrationList_ToggleSwitch
                ToggleSwitchData = new UC_ToggleSwitchViewModel
                {
                    Label = names[i],
                    CommandId = 6,
                    CommandSubId = (short)(i * 2 + 1),
                    CheckedBackground_final = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2a5ed5")),
                    CheckedText = "ON",
                    UnCheckedText = "OFF",
                    Height = 20
                };
                addData(typeof(UC_ToggleSwitchViewModel), ToggleSwitchData, "CalibrationList_ToggleSwitch");

                #endregion CalibrationList_ToggleSwitch
                CalibWizardData = new CalibrationWizardViewModel
                {
                    isWizard = false,
                    CalibrationName = names[i],
                    CalibStatus = 0,
                    CommandId = "6",
                    CommandSubId = (i * 2 + 2).ToString(CultureInfo.InvariantCulture),
                    CalibTimeout = CalibTimeout[i]
                };
                addData(typeof(CalibrationWizardViewModel), CalibWizardData, "Calibration Result List");
            }
        }
        private void BuildErrorList()
        {
            // Com. Error: 
            ErrorList.Add(2, "BAD COMMAND");
            ErrorList.Add(3, "BAD INDEX");
            ErrorList.Add(5, "NO INTERPRETER MEANING");
            ErrorList.Add(6, "PROGRAM NOT RUNNING");
            ErrorList.Add(7, "MODE NOT STARTED");
            ErrorList.Add(11, "CANNOT WRITE TO FLASH");
            ErrorList.Add(12, "COMMAND NOT AVAILABLE");
            ErrorList.Add(13, "UART BUSY");
            ErrorList.Add(18, "EMPTY ASSIGN");
            ErrorList.Add(19, "BAD COMMAND FORMAT");
            ErrorList.Add(21, "OPERAND OUT OF RANGE");
            ErrorList.Add(22, "ZERO DIVISION");
            ErrorList.Add(23, "COMMAND NOT ASSIGNED");
            ErrorList.Add(24, "BAD OPERAND");
            ErrorList.Add(25, "COMMAND NOT VALID");
            ErrorList.Add(26, "MOTION MODE NOT VALID");
            ErrorList.Add(28, "OUT OF LIMIT RANGE");
            ErrorList.Add(30, "NO PROGRAM TO CONTINUE");
            ErrorList.Add(32, "COMMUNICATION ERROR");
            ErrorList.Add(37, "HALL DEFINED SAME LOCATION");
            ErrorList.Add(38, "HALL READING ERROR");
            ErrorList.Add(39, "MOTION START PAST");
            ErrorList.Add(41, "COMMAND NOT SUPPORTED");
            ErrorList.Add(42, "NO SUCH LABEL");
            ErrorList.Add(57, "MOTOR MUST BE OFF");
            ErrorList.Add(58, "MOTOR MUST BE ON");
            ErrorList.Add(60, "BAD UNIT MODE");
            ErrorList.Add(66, "DRIVE NOT READY");
            ErrorList.Add(71, "HOMING BUSY");
            ErrorList.Add(72, "MODULO MUST EVEN");
            ErrorList.Add(73, "SET POSITION");
            ErrorList.Add(127, "MODULO RANGE MUST POSITIVE");
            ErrorList.Add(166, "OUT OF MODULO RANGE");
            ErrorList.Add(200, "Reset Driver occurred");
        }

        private void GenerateDebugListCommands()
        {
            #region Operation
            Data = new DataViewModel
            {
                CommandName = "Checksum",
                CommandId = "62",
                CommandSubId = "10",
                CommandValue = "",
                IsFloat = false,
            };
            addData(typeof(DataViewModel), Data, "InternalParam List");
            #endregion Operation
        }

        public void driver_error_occured(int commandId, int commandSubId, Int32 transit)
        {
            string result;
            if(ErrorList.TryGetValue(transit, out result))
            {
                EventRiser.Instance.RiseEevent(string.Format($"Com. Error: " + result));
                if(/*!WizardWindowViewModel.GetInstance.StartEnable*/ WizardWindowViewModel.calibration_is_in_process)
                {
                    WizardWindowViewModel.GetInstance.AbortCalib();
                }
            }
            else
                EventRiser.Instance.RiseEevent(string.Format($"Error: " + commandId.ToString() + "[" + commandSubId.ToString() + "] = " + transit.ToString()));

        }
        private void GenerateToggleSwitchCommands()
        {
            #region Operation
            ToggleSwitchData = new UC_ToggleSwitchViewModel
            {
                Label = "Motor",
                CommandId = 1,
                CommandSubId = 0,
                CheckedBackground_final = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2a5ed5")),
                CheckedText = "ON",
                UnCheckedText = "OFF"
            };
            addData(typeof(UC_ToggleSwitchViewModel), ToggleSwitchData, "MotorControl");
            
            #endregion Operation
            
            #region Feedback_Sync
            ToggleSwitchData = new UC_ToggleSwitchViewModel
            {
                Label = "External interpolation",
                CommandId = 78,
                CommandSubId = 1,
                CheckedBackground_final = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2a5ed5")),
                CheckedText = "ON",
                UnCheckedText = "OFF"
            };
            addData(typeof(UC_ToggleSwitchViewModel), ToggleSwitchData, "Feedback Sync");
            #endregion Feedback_Sync

            #region PID
            ToggleSwitchData = new UC_ToggleSwitchViewModel
            {
                Label = "Close Loop",
                CommandId = 82,
                CommandSubId = 10,
                CheckedBackground_final = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2a5ed5")),
                CheckedText = "ON",
                UnCheckedText = "OFF"
            };
            addData(typeof(UC_ToggleSwitchViewModel), ToggleSwitchData, "PID_speed_loop");
            
            ToggleSwitchData = new UC_ToggleSwitchViewModel
            {
                Label = "Close Loop",
                CommandId = 81,
                CommandSubId = 10,
                CheckedBackground_final = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2a5ed5")),
                CheckedText = "ON",
                UnCheckedText = "OFF"
            };
            addData(typeof(UC_ToggleSwitchViewModel), ToggleSwitchData, "PID_current_loop");
            
            ToggleSwitchData = new UC_ToggleSwitchViewModel
            {
                Label = "Close Loop",
                CommandId = 83,
                CommandSubId = 10,
                CheckedBackground_final = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2a5ed5")),
                CheckedText = "ON",
                UnCheckedText = "OFF"
            };
            addData(typeof(UC_ToggleSwitchViewModel), ToggleSwitchData, "PID_position_loop");
            
            #endregion PID

            #region Filter
            ToggleSwitchData = new UC_ToggleSwitchViewModel
            {
                Label = "Enable",
                CommandId = 101,
                CommandSubId = 0,
                CheckedBackground_final = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2a5ed5")),
                CheckedText = "ON",
                UnCheckedText = "OFF"
            };
            addData(typeof(UC_ToggleSwitchViewModel), ToggleSwitchData, "Filter_Enable");
            
            #endregion Filter
            #region Bode
            
            ToggleSwitchData = new UC_ToggleSwitchViewModel
            {
                Label = "Bode Operation",
                CommandId = 6,
                CommandSubId = 15,
                CheckedBackground_final = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2a5ed5")),
                CheckedText = "STOP",
                UnCheckedText = "START"
            };
            addData(typeof(UC_ToggleSwitchViewModel), ToggleSwitchData, "BodeStart");
            #endregion Bode

            #region MaintenanceOperation
            string[] name = { "Save Parameters To Driver", "Load Manufacturer defaults", "Reset", "Protected Params" };
                            //"Protected Params          "
            short[] subID = { 0, 1, 9, 10 };

            for(int i = 0; i < name.Length; i++)
            {
                ToggleSwitchData = new UC_ToggleSwitchViewModel
                {
                    Label = name[i],
                    CommandId = 63,
                    CommandSubId = subID[i],
                    CheckedBackground_final = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2a5ed5")),
                    CheckedText = "ON",
                    UnCheckedText = "OFF"
                };
                addData(typeof(UC_ToggleSwitchViewModel), ToggleSwitchData, "MaintenanceOperation");
                if(i < 2)
                    addData(typeof(UC_ToggleSwitchViewModel), ToggleSwitchData, "WizardOperation");
            }
            #endregion MaintenanceOperation

            #region BP_ToggleSwitch
            ToggleSwitchData = new UC_ToggleSwitchViewModel
            {
                Label = "Stop Motion",
                CommandId = 1,
                CommandSubId = 1,
                CheckedBackground_final = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2a5ed5")),
                CheckedText = "STOP",
                UnCheckedText = "START",
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(UC_ToggleSwitchViewModel), ToggleSwitchData, "BP_ToggleSwitch");
            ToggleSwitchData = new UC_ToggleSwitchViewModel
            {
                Label = "Power Out",
                CommandId = 12,
                CommandSubId = 1,
                CheckedBackground_final = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2a5ed5")),
                CheckedText = "ON",
                UnCheckedText = "OFF",
                FontSize = DataViewModel_FontSize
            };
            addData(typeof(UC_ToggleSwitchViewModel), ToggleSwitchData, "BP_ToggleSwitch");
            #endregion BP_ToggleSwitch
        }
        private void GenerateCHList()
        {
            Channel = new UC_ChannelViewModel
            {
                Label = "Ch1",
                CommandId = 60,
                CommandSubId = 1,
                ChBackground = Consts._project == Consts.eProject.REDLER ? (Color)ColorConverter.ConvertFromString("#CCF7E31D") : (Color)ColorConverter.ConvertFromString("#CCF7E31D"),
                Gain = "1",
                IsEnabled = false,
                IsOpened = false
            };
            addData(typeof(UC_ChannelViewModel), Channel, "ChannelsList");

            Channel = new UC_ChannelViewModel
            {
                Label = "Ch2",
                CommandId = 60,
                CommandSubId = 2,
                ChBackground = Consts._project == Consts.eProject.REDLER ? (Color)ColorConverter.ConvertFromString("#7F1810D4") : Colors.CornflowerBlue,
                Gain = "1",
                IsEnabled = false,
                IsOpened = false
            };
            addData(typeof(UC_ChannelViewModel), Channel, "ChannelsList");
        }
        public void addData(Type data_type, object _data, string destination_list)
        {
            switch(data_type.Name)
            {
                case "DataViewModel":
                    if(!GenericCommandsList.ContainsKey(new Tuple<int, int>(Convert.ToInt16(((DataViewModel)_data).CommandId), Convert.ToInt16(((DataViewModel)_data).CommandSubId))))
                        GenericCommandsList.Add(new Tuple<int, int>(Convert.ToInt16(((DataViewModel)_data).CommandId), Convert.ToInt16(((DataViewModel)_data).CommandSubId)), (DataViewModel)_data);
                    if(!GenericCommandsGroup.ContainsKey(destination_list))
                        GenericCommandsGroup.Add(destination_list, new ObservableCollection<object>());
                    GenericCommandsGroup[destination_list].Add((DataViewModel)_data);
                    break;
                case "EnumViewModel":
                    if(!GenericCommandsList.ContainsKey(new Tuple<int, int>(Convert.ToInt16(((EnumViewModel)_data).CommandId), Convert.ToInt16(((EnumViewModel)_data).CommandSubId))))
                        GenericCommandsList.Add(new Tuple<int, int>(Convert.ToInt16(((EnumViewModel)_data).CommandId), Convert.ToInt16(((EnumViewModel)_data).CommandSubId)), (EnumViewModel)_data);
                    if(!GenericCommandsGroup.ContainsKey(destination_list))
                        GenericCommandsGroup.Add(destination_list, new ObservableCollection<object>());
                    GenericCommandsGroup[destination_list].Add((EnumViewModel)_data);
                    break;
                case "CalibrationButtonModel":
                    //if(!GenericCommandsList.ContainsKey(new Tuple<int, int>(Convert.ToInt16(((CalibrationButtonModel)_data).CommandId), Convert.ToInt16(((CalibrationButtonModel)_data).CommandSubId))))
                    //    GenericCommandsList.Add(new Tuple<int, int>(Convert.ToInt16(((CalibrationButtonModel)_data).CommandId), Convert.ToInt16(((CalibrationButtonModel)_data).CommandSubId)), (CalibrationButtonModel)_data);
                    //if(!CalibartionCommandsListbySubGroup.ContainsKey(destination_list))
                    //    CalibartionCommandsListbySubGroup.Add(destination_list, new ObservableCollection<object>());
                    //CalibartionCommandsListbySubGroup[destination_list].Add((CalibrationButtonModel)_data);
                    break;
                case "UC_ToggleSwitchViewModel":
                    if(!GenericCommandsList.ContainsKey(new Tuple<int, int>(Convert.ToInt16(((UC_ToggleSwitchViewModel)_data).CommandId), Convert.ToInt16(((UC_ToggleSwitchViewModel)_data).CommandSubId))))
                        GenericCommandsList.Add(new Tuple<int, int>(Convert.ToInt16(((UC_ToggleSwitchViewModel)_data).CommandId), Convert.ToInt16(((UC_ToggleSwitchViewModel)_data).CommandSubId)), (UC_ToggleSwitchViewModel)_data);
                    if(!GenericCommandsGroup.ContainsKey(destination_list))
                        GenericCommandsGroup.Add(destination_list, new ObservableCollection<object>());
                    GenericCommandsGroup[destination_list].Add((UC_ToggleSwitchViewModel)_data);
                    break;
                case "CalibrationWizardViewModel":
                    if(!GenericCommandsList.ContainsKey(new Tuple<int, int>(Convert.ToInt16(((CalibrationWizardViewModel)_data).CommandId), Convert.ToInt16(((CalibrationWizardViewModel)_data).CommandSubId))))
                        GenericCommandsList.Add(new Tuple<int, int>(Convert.ToInt16(((CalibrationWizardViewModel)_data).CommandId), Convert.ToInt16(((CalibrationWizardViewModel)_data).CommandSubId)), (CalibrationWizardViewModel)_data);
                    if(!GenericCommandsGroup.ContainsKey(destination_list))
                        GenericCommandsGroup.Add(destination_list, new ObservableCollection<object>());
                    GenericCommandsGroup[destination_list].Add((CalibrationWizardViewModel)_data);
                    break;
                case "BoolViewIndModel":
                    if(!GenericCommandsList.ContainsKey(new Tuple<int, int>(Convert.ToInt16(((BoolViewIndModel)_data).CommandId), Convert.ToInt16(((BoolViewIndModel)_data).CommandSubId))))
                        GenericCommandsList.Add(new Tuple<int, int>(Convert.ToInt16(((BoolViewIndModel)_data).CommandId), Convert.ToInt16(((BoolViewIndModel)_data).CommandSubId)), (BoolViewIndModel)_data);
                    if(!GenericCommandsGroup.ContainsKey(destination_list))
                        GenericCommandsGroup.Add(destination_list, new ObservableCollection<object>());
                    GenericCommandsGroup[destination_list].Add((BoolViewIndModel)_data);
                    break;
                case "UC_ChannelViewModel":
                    if(!GenericCommandsList.ContainsKey(new Tuple<int, int>(Convert.ToInt16(((UC_ChannelViewModel)_data).CommandId), Convert.ToInt16(((UC_ChannelViewModel)_data).CommandSubId))))
                        GenericCommandsList.Add(new Tuple<int, int>(Convert.ToInt16(((UC_ChannelViewModel)_data).CommandId), Convert.ToInt16(((UC_ChannelViewModel)_data).CommandSubId)), (UC_ChannelViewModel)_data);
                    if(!GenericCommandsGroup.ContainsKey(destination_list))
                        GenericCommandsGroup.Add(destination_list, new ObservableCollection<object>());
                    GenericCommandsGroup[destination_list].Add((UC_ChannelViewModel)_data);
                    break;
                case "DebugObjModel":
                    if(!GenericCommandsList.ContainsKey(new Tuple<int, int>(Convert.ToInt16(((DebugObjModel)_data).ID), Convert.ToInt16(((DebugObjModel)_data).Index))))
                        GenericCommandsList.Add(new Tuple<int, int>(Convert.ToInt16(((DebugObjModel)_data).ID), Convert.ToInt16(((DebugObjModel)_data).Index)), (DebugObjModel)_data);
                    if(!GenericCommandsGroup.ContainsKey(destination_list))
                        GenericCommandsGroup.Add(destination_list, new ObservableCollection<object>());
                    int contains = 0;
                    if(GenericCommandsGroup[destination_list].Count > 0)
                    {
                        for(int i = 0; i < GenericCommandsGroup[destination_list].Count; i++)
                        {
                            if(DebugViewModel.GetInstance.CompareDebugObj((DebugObjModel)_data, GenericCommandsGroup["Debug List"][i] as DebugObjModel))
                                break;
                            else
                                contains++;
                        }
                    }
                    if(contains == GenericCommandsGroup[destination_list].Count)
                        GenericCommandsGroup[destination_list].Add((DebugObjModel)_data);

                    break;
            }
        }
        public void _reset_data()
        {
            for(int i = 0; i < GenericCommandsList.Count; i++)
            {
                Type _type = GenericCommandsList.ElementAt(i).Value.GetType();
                switch(_type.Name)
                {
                    case "DataViewModel":
                        ((DataViewModel)GenericCommandsList.ElementAt(i).Value).CommandValue = "";
                        break;
                    case "CalibrationWizardViewModel":
                        ((CalibrationWizardViewModel)GenericCommandsList.ElementAt(i).Value).CalibStatus = 0;
                        break;
                    case "UC_ToggleSwitchViewModel":
                        ((UC_ToggleSwitchViewModel)GenericCommandsList.ElementAt(i).Value).IsChecked = false;
                        break;
                    case "EnumViewModel":
                            ((EnumViewModel)GenericCommandsList.ElementAt(i).Value).SelectedItem = ((EnumViewModel)GenericCommandsList.ElementAt(i).Value).CommandList[0];
                        break;
                    case "BoolViewIndModel":
                        ((BoolViewIndModel)GenericCommandsList.ElementAt(i).Value).CommandValue = 0;
                        break;
                    case "UC_ChannelViewModel":
                        ((UC_ChannelViewModel)GenericCommandsList.ElementAt(i).Value).ChSelectedIndex = 0;
                        break;
                }
            }
        }
    }
}

