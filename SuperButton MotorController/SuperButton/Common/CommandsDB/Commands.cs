using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using SuperButton.ViewModels;
using System.Windows.Media;
using System.Linq;
using SuperButton.Helpers;

namespace SuperButton.CommandsDB
{
    enum eDeviceInfo
    {
        Serial_Number = 1,
        Hardware_Rev = 2,
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
        const bool INT = false;
        const bool FLOAT = true;

        private Commands()
        {
            GenerateMotionCommands();
            GenerateFeedBakcCommands();
            GeneratePidCommands();
            GenerateFilterCommands();
            GenerateDeviceCommands();
            GenerateBPCommands();
            GenerateLPCommands();
            GenerateMotionTabCommands();
            GenerateMaintenanceList();
            UpperMainPannelList();
            CalibrationCmd();
            BuildErrorList();
            GenerateDebugListCommands();
            GenerateIOTabCommands();
            GenerateBodeCommands();
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
        public Dictionary<Tuple<int, int>, DataViewModel> DataViewCommandsList = new Dictionary<Tuple<int, int>, DataViewModel>();
        public Dictionary<Tuple<int, int>, DataViewModel> DataViewCommandsListLP = new Dictionary<Tuple<int, int>, DataViewModel>();
        public Dictionary<Tuple<int, int>, EnumViewModel> EnumViewCommandsList = new Dictionary<Tuple<int, int>, EnumViewModel>();
        public Dictionary<Tuple<int, int>, CalibrationButtonModel> CalibartionCommandsList = new Dictionary<Tuple<int, int>, CalibrationButtonModel>();
        public Dictionary<Tuple<int, int, bool>, DebugObjModel> DebugCommandsList = new Dictionary<Tuple<int, int, bool>, DebugObjModel>();


        public Dictionary<string, List<string>> Enums = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> EnumsQep1 = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> EnumsQep2 = new Dictionary<string, List<string>>();

        public Dictionary<string, ObservableCollection<object>> EnumCommandsListbySubGroup = new Dictionary<string, ObservableCollection<object>>();
        public Dictionary<string, ObservableCollection<object>> DataCommandsListbySubGroup = new Dictionary<string, ObservableCollection<object>>();
        public Dictionary<string, ObservableCollection<object>> CalibartionCommandsListbySubGroup = new Dictionary<string, ObservableCollection<object>>();
        public Dictionary<string, ObservableCollection<object>> DebugCommandsListbySubGroup = new Dictionary<string, ObservableCollection<object>>();

        public Dictionary<int, string> ErrorList = new Dictionary<int, string>();
        public Dictionary<Tuple<int, int>, BoolViewIndModel> DigitalInputList = new Dictionary<Tuple<int, int>, BoolViewIndModel>();
        public Dictionary<string, ObservableCollection<object>> DigitalInputListbySubGroup = new Dictionary<string, ObservableCollection<object>>();

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
        }

        DataViewModel Data = new DataViewModel();
        EnumViewModel eData = new EnumViewModel();

        private void GenerateDeviceCommands()
        {
            DataCommandsListbySubGroup.Add("DeviceSerial", new ObservableCollection<object>());

            for(int i = 0; i < Enum.GetNames(typeof(eDeviceInfo)).Length; i++)
            {
                Data = new DataViewModel
                {
                    CommandName = EnumHelper.GetNames(Enum.GetNames(typeof(eDeviceInfo))).ElementAt(i),
                    CommandId = "62",
                    CommandSubId = ((int)EnumHelper.GetEnumValue<eDeviceInfo>(Enum.GetNames(typeof(eDeviceInfo)).ElementAt(i))).ToString(),
                    IsFloat = false
                };

                DataViewCommandsList.Add(new Tuple<int, int>(Convert.ToInt16(Data.CommandId), Convert.ToInt16(Data.CommandSubId)), Data);
                DataCommandsListbySubGroup["DeviceSerial"].Add(Data);
            }

            Enums.Add("Baudrate", EnumHelper.GetNames(Enum.GetNames(typeof(eBaudRate))).ToList());

            eData = new EnumViewModel
            {
                CommandName = "Baudrate",
                CommandId = "61",
                CommandSubId = "1",
                CommandList = EnumHelper.GetNames(Enum.GetNames(typeof(eBaudRate))).ToList(),
                CommandValue = ((int)EnumHelper.GetEnumValue<eBaudRate>(Enum.GetNames(typeof(eBaudRate)).ElementAt(0))).ToString(),//first enum in list
            };

            EnumViewCommandsList.Add(new Tuple<int, int>(Convert.ToInt16(eData.CommandId), Convert.ToInt16(eData.CommandSubId)), eData);
            EnumCommandsListbySubGroup.Add("BaudrateList", new ObservableCollection<object> { eData });
            #region Synch Command cmdID 64 cmdSubID 0

            DataCommandsListbySubGroup.Add("DeviceSynchCommand", new ObservableCollection<object>());
            Data = new DataViewModel
            {
                CommandId = "64",
                CommandSubId = "0",
                CommandName = "PlotCommand",
                IsFloat = false,
                CommandValue = "0"
            };
            DataCommandsListbySubGroup["DeviceSynchCommand"].Add(Data); // 0

            Data = new DataViewModel
            {
                CommandId = "64",
                CommandSubId = "1",
                CommandName = "AutoBaud_Sync",
                IsFloat = false,
                CommandValue = "0"
            };
            DataCommandsListbySubGroup["DeviceSynchCommand"].Add(Data); // 1

            Data = new DataViewModel
            {
                CommandId = "1",
                CommandSubId = "0",
                CommandName = "AutoBaud_Motor",
                IsFloat = false,
                CommandValue = "0"
            };
            DataCommandsListbySubGroup["DeviceSynchCommand"].Add(Data); // 2

            Data = new DataViewModel
            {
                CommandId = "61",
                CommandSubId = "1",
                CommandName = "AutoBaud_Baudrate",
                IsFloat = false,
                CommandValue = "0"
            };
            DataCommandsListbySubGroup["DeviceSynchCommand"].Add(Data); // 3
            #endregion

        }
        private void GeneratePidCommands()
        {
            var names = new[]
            {
                "Kp", "Ki", "Kd", "kp range", "Range"
            };
            string[] index = new[]
            {
                "1", "2", "4", "5", "6"
            };
            DataCommandsListbySubGroup.Add("PIDSpeed", new ObservableCollection<object>());
            DataCommandsListbySubGroup.Add("PIDPosition", new ObservableCollection<object>());
            DataCommandsListbySubGroup.Add("PIDListBackGround", new ObservableCollection<object>());

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
                DataViewCommandsList.Add(new Tuple<int, int>(Convert.ToInt16(Data.CommandId), Convert.ToInt16(Data.CommandSubId)), Data);
                DataCommandsListbySubGroup["PIDSpeed"].Add(Data);


                Data = new DataViewModel
                {
                    CommandName = names[i],
                    CommandId = "83",
                    CommandSubId = index[i],
                    CommandValue = "",
                    IsFloat = names[i] == "Range" ? false : true
                };
                DataViewCommandsList.Add(new Tuple<int, int>(Convert.ToInt16(Data.CommandId), Convert.ToInt16(Data.CommandSubId)), Data);
                DataCommandsListbySubGroup["PIDPosition"].Add(Data);
            }
            var data1 = new DataViewModel
            {
                CommandName = "close loop",
                CommandId = "82",
                CommandSubId = (10).ToString(CultureInfo.InvariantCulture),
                CommandValue = "",
                IsFloat = false
            };
            DataCommandsListbySubGroup["PIDListBackGround"].Add(data1);

            data1 = new DataViewModel
            {
                CommandName = "close loop",
                CommandId = "83",
                CommandSubId = (10).ToString(CultureInfo.InvariantCulture),
                CommandValue = "",
                IsFloat = false
            };
            DataCommandsListbySubGroup["PIDListBackGround"].Add(data1);

            data1 = new DataViewModel
            {
                CommandName = "close loop",
                CommandId = "81",
                CommandSubId = (10).ToString(CultureInfo.InvariantCulture),
                CommandValue = "",
                IsFloat = false
            };
            DataCommandsListbySubGroup["PIDListBackGround"].Add(data1);

            names = new[]
            {
                "Kp", "Ki"
            };

            DataCommandsListbySubGroup.Add("PIDCurrent", new ObservableCollection<object>());

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
                DataViewCommandsList.Add(new Tuple<int, int>(Convert.ToInt16(Data.CommandId), Convert.ToInt16(Data.CommandSubId)), Data);
                DataCommandsListbySubGroup["PIDCurrent"].Add(Data);
            }
        }
        private void GenerateFilterCommands()
        {
            var names = new[]
            {
                "Number of sections", "section 0 a[0]", "section 0 a[1]", "section 0 a[2]",
                "section 0 b[0]", "section 0 b[1]", "section 0 b[2]"
            };
            DataCommandsListbySubGroup.Add("FilterList", new ObservableCollection<object>());
            DataCommandsListbySubGroup.Add("FilterBackGround", new ObservableCollection<object>());

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
                DataViewCommandsList.Add(new Tuple<int, int>(Convert.ToInt16(Data.CommandId), Convert.ToInt16(Data.CommandSubId)), Data);
                DataCommandsListbySubGroup["FilterList"].Add(Data);
               
            }
            var data1 = new DataViewModel
            {
                CommandName = "Enable",
                CommandId = "101",
                CommandSubId = (0).ToString(CultureInfo.InvariantCulture),
                CommandValue = "",
                IsFloat = false
            };
            DataCommandsListbySubGroup["FilterBackGround"].Add(data1);
            
        }
        private void GenerateFeedBakcCommands()
        {
            #region Hall
            var names = new[]
            {
                "Enable", "Roll High", "Roll Low", "Direction", "Counts Per Rev", "Speed LPF Cut-Off"
            };

            DataCommandsListbySubGroup.Add("Hall", new ObservableCollection<object>());
            DataCommandsListbySubGroup.Add("Qep1", new ObservableCollection<object>());
            DataCommandsListbySubGroup.Add("Qep2", new ObservableCollection<object>());
            DataCommandsListbySubGroup.Add("SSI_Feedback", new ObservableCollection<object>());

            for(var i = 1; i < names.Length; i++)
            {
                var data = new DataViewModel();
                data = new DataViewModel
                {
                    CommandName = names[i],
                    CommandId = "70",
                    CommandSubId = (i + 1).ToString(CultureInfo.InvariantCulture),
                    CommandValue = "",
                    IsFloat = names[i] == "Speed LPF Cut-Off"
                };
                DataViewCommandsList.Add(new Tuple<int, int>(70, i + 1), data);
                DataCommandsListbySubGroup["Hall"].Add(data);
            }

            for(int i = 1; i < 5; i++)
            {
                var data = new DataViewModel
                {
                    CommandName = "Hall Angle " + i.ToString(),
                    CommandId = "70",
                    CommandSubId = (i + 6).ToString(CultureInfo.InvariantCulture),
                    CommandValue = "",
                    IsFloat = false
                };
                DataViewCommandsList.Add(new Tuple<int, int>(70, i + 6), data);
                DataCommandsListbySubGroup["Hall"].Add(data);
            }

            for(int i = 0; i < 6; i++)
            {
                var data = new DataViewModel
                {
                    CommandName = "Hall Map " + i.ToString(),
                    CommandId = "84",
                    CommandSubId = (i + 1).ToString(CultureInfo.InvariantCulture),
                    CommandValue = "",
                    IsFloat = false
                };
                DataViewCommandsList.Add(new Tuple<int, int>(84, (i + 1)), data);
                DataCommandsListbySubGroup["Hall"].Add(data);
            }

            var data2 = new DataViewModel
            {
                CommandName = "Sample Period",
                CommandId = "70",
                CommandSubId = "15",
                CommandValue = "",
                IsFloat = false
            };
            DataViewCommandsList.Add(new Tuple<int, int>(70, 15), data2);
            DataCommandsListbySubGroup["Hall"].Add(data2);
            #endregion Hall
            #region SSI

            names = new[]
            {
                "Enable", "Roll High", "Roll Low", "Direction", "Counts Per Rev", "LPF Cut-Off", "Baud rate", "Encoder Bits", "Clock Phase", "Clock Polarity", "Tail Bits", "Packet Delay", "Calibrated Angle", "Head Bits", "Sample Period"
            };
            var SubId = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" };
            var type = new[] { false, false, false, false, false, true, false, false, false, false, false, false, true, false, false };

            for(int i = 1; i < names.Length; i++)
            {
                var data = new DataViewModel
                {
                    CommandName = names[i],
                    CommandId = "73",
                    CommandSubId = SubId[i],
                    CommandValue = "",
                    IsFloat = type[i]
                };

                DataViewCommandsList.Add(new Tuple<int, int>(73, Convert.ToInt16(SubId[i])), data);
                DataCommandsListbySubGroup["SSI_Feedback"].Add(data);
            }
            #endregion SSI
            #region Qep1Qep2
            names = new[]
            {
                "Enable", "Roll High", "Roll Low", "Direction", "Counts Per Rev",
                "Speed LPF", "Index Mode", "Reset Value", "Set Position Value"
            };
            bool[] IsFloat = new[] { false, false, false, false, false, true, false, false, false };
            for(int i = 1, k = 2; i < names.Length; i++, k++)
            {
                var data = new DataViewModel
                {
                    CommandName = names[i],
                    CommandId = "71",
                    CommandSubId = k.ToString(CultureInfo.InvariantCulture),
                    CommandValue = "",
                    IsFloat = IsFloat[i],
                };
                DataViewCommandsList.Add(new Tuple<int, int>(71, k), data);
                DataCommandsListbySubGroup["Qep1"].Add(data);

                data = new DataViewModel
                {
                    CommandName = names[i],
                    CommandId = "72",
                    CommandSubId = k.ToString(CultureInfo.InvariantCulture),
                    CommandValue = "",
                    IsFloat = IsFloat[i],
                };
                DataViewCommandsList.Add(new Tuple<int, int>(72, k), data);
                DataCommandsListbySubGroup["Qep2"].Add(data);

                if(k == 7)
                    k = 8;
                if(k == 9)
                    k = 12;
            }

            var dataB = new DataViewModel
            {
                CommandName = "Resolution Sin/Cos",
                CommandId = "71",
                CommandSubId = 14.ToString(CultureInfo.InvariantCulture),
                CommandValue = "",
                IsFloat = false,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(71, 14), dataB);
            DataCommandsListbySubGroup["Qep1"].Add(dataB);

            //Qep1FdBckList Qep1Bis
            var tmp1 = new List<string>
              {
                  "Index Disabled",
                  "One Shot",
                  "Continuous Refresh"
              };
            EnumsQep1.Add("Index Reset", tmp1);

            var enum1 = new EnumViewModel
            {
                CommandName = "Index Reset",
                CommandId = "71",
                CommandSubId = "8",
                CommandList = EnumsQep1["Index Reset"],
                CommandValue = "1",//first enum in list
                IsFloat = false,
                //SelectedValue = "0",
            };

            EnumViewCommandsList.Add(new Tuple<int, int>(71, 8), enum1);

            EnumCommandsListbySubGroup.Add("Qep1Bis", new ObservableCollection<object>
            {
              enum1
            });

            var tmp2 = new List<string>
              {
                  "Index Disabled",
                  "One Shot",
                  "Continuous Refresh"
              };
            EnumsQep2.Add("Index Reset", tmp2);

            var enum2 = new EnumViewModel
            {
                CommandName = "Index Reset",
                CommandId = "72",
                CommandSubId = "8",
                CommandList = EnumsQep2["Index Reset"],
                CommandValue = "1",//first enum in list
                IsFloat = false,
                //SelectedValue = "0",
            };

            EnumViewCommandsList.Add(new Tuple<int, int>(72, 8), enum2);

            EnumCommandsListbySubGroup.Add("Qep2Bis", new ObservableCollection<object>
            {
              enum2
            });
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

            var enum1 = new EnumViewModel
            {
                CommandName = "Drive Mode",
                CommandId = "50",
                CommandSubId = "1",
                CommandList = Enums["Drive Mode"],
                CommandValue = "1",//first enum in list
            };

            EnumViewCommandsList.Add(new Tuple<int, int>(50, 1), enum1);

            EnumCommandsListbySubGroup.Add("Control", new ObservableCollection<object>
            {
              enum1
            });

            var tmp2 = new List<string>
             {
                "Brushed",
                "BL with Hall",
                "BL with Hall and Incremental encoder",
                "BL with Absolute encoder",
                "BL Stepper Sensorless"
             };
            Enums.Add("Electrical Commutation Type", tmp2);

            var enum2 = new EnumViewModel
            {
                CommandName = "Electrical Commutation Type",
                CommandId = "50",
                CommandSubId = "2",
                CommandValue = "0", //first enum in list
                CommandList = Enums["Electrical Commutation Type"]
            };
            EnumViewCommandsList.Add(new Tuple<int, int>(50, 2), enum2);
            EnumCommandsListbySubGroup["Control"].Add(enum2);

            tmp2 = new List<string>
             {
                "Disable",
                "Enable"
             };
            Enums.Add("Motor Hall", tmp2);

            enum2 = new EnumViewModel
            {
                CommandName = "Motor Hall",
                CommandId = "70",
                CommandSubId = "1",
                CommandValue = "0", //first enum in list
                CommandList = Enums["Motor Hall"]
            };
            EnumViewCommandsList.Add(new Tuple<int, int>(70, 1), enum2);
            EnumCommandsListbySubGroup["Control"].Add(enum2);

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
            var enum6 = new EnumViewModel
            {
                CommandName = "Motor encoder",
                CommandId = "50",
                CommandSubId = "3",
                CommandValue = "0", //first enum in list
                CommandList = Enums["Motor encoder"]
            };
            EnumViewCommandsList.Add(new Tuple<int, int>(50, 3), enum6);
            EnumCommandsListbySubGroup["Control"].Add(enum6);

            Enums.Add("External encoder", tmp6);
            var enum7 = new EnumViewModel
            {
                CommandName = "External encoder",
                CommandId = "50",
                CommandSubId = "4",
                CommandValue = "0", //first enum in list
                CommandList = Enums["External encoder"]
            };
            EnumViewCommandsList.Add(new Tuple<int, int>(50, 4), enum7);
            EnumCommandsListbySubGroup["Control"].Add(enum7);

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
            var enum5 = new EnumViewModel
            {
                CommandName = "Command Source",
                CommandId = "50",
                CommandSubId = "5",
                CommandValue = "1", //first enum in list
                CommandList = Enums["Command Source"]
            };
            EnumViewCommandsList.Add(new Tuple<int, int>(50, 5), enum5);
            EnumCommandsListbySubGroup["Control"].Add(enum5);

            var tmp3 = new List<string>
             {
                "Hall",
                "Motor",
                "External"
             };

            Enums.Add("Speed loop Fdb", tmp3);
            var enum3 = new EnumViewModel
            {
                CommandName = "Speed loop Fdb",
                CommandId = "50",
                CommandSubId = "6",
                CommandValue = "1", //first enum in list
                CommandList = Enums["Speed loop Fdb"]
            };
            EnumViewCommandsList.Add(new Tuple<int, int>(50, 6), enum3);
            EnumCommandsListbySubGroup["Control"].Add(enum3);


            Enums.Add("Position loop Fdb", tmp3);
            var enum4 = new EnumViewModel
            {
                CommandName = "Position loop Fdb",
                CommandId = "50",
                CommandSubId = "7",
                CommandValue = "1", //first enum in list
                CommandList = Enums["Position loop Fdb"]
            };
            EnumViewCommandsList.Add(new Tuple<int, int>(50, 7), enum4);
            EnumCommandsListbySubGroup["Control"].Add(enum4);

            var data1 = new DataViewModel
            {
                CommandName = "Pole Pair",
                CommandId = "51",
                CommandSubId = "1",
                CommandValue = "",
                IsFloat = false,
            };
            DataCommandsListbySubGroup.Add("Motor", new ObservableCollection<object> { data1 });
            DataViewCommandsList.Add(new Tuple<int, int>(51, 1), data1);


            var data2 = new DataViewModel
            {
                CommandName = "Direction",
                CommandId = "51",
                CommandSubId = "2",
                CommandValue = "",
                IsFloat = false,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(51, 2), data2);
            DataCommandsListbySubGroup["Motor"].Add(data2);

            string[] commandName = { "Max speed [C/S]", "Min Speed [C/S]", "Max position [C]", "Min position [C]", "Enable Position Limit", "Motor stuck current", "Motor stuck speed", "Motor stuck Duration" };
            bool[] Type = { INT, INT, INT, INT, INT, FLOAT, INT, FLOAT };

            DataCommandsListbySubGroup.Add("Motion Limit", new ObservableCollection<object>());

            for(int i = 0; i < commandName.Length; i++)
            {
                var data = new DataViewModel
                {
                    CommandName = commandName[i],
                    CommandId = "53",
                    CommandSubId = (i + 1).ToString(),
                    CommandValue = "",
                    IsFloat = Type[i]
                };
                DataViewCommandsList.Add(new Tuple<int, int>(53, (i + 1)), data);
                DataCommandsListbySubGroup["Motion Limit"].Add(data);
            }
        }
        public void GenerateBPCommands()
        {
            #region Commands1
            DataCommandsListbySubGroup.Add("MotionCommand List", new ObservableCollection<object>());
            DataCommandsListbySubGroup.Add("MotionCommand List2", new ObservableCollection<object>());

            var data = new DataViewModel
            {
                CommandName = "Current [A]",
                CommandId = "3",
                CommandSubId = "0",
                CommandValue = "",
                IsFloat = true,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(3, 0), data);
            DataCommandsListbySubGroup["MotionCommand List"].Add(data);

            data = new DataViewModel
            {
                CommandName = "Speed [C/S]",
                CommandId = "4",
                CommandSubId = "0",
                CommandValue = "",
                IsFloat = false,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(4, 0), data);
            DataCommandsListbySubGroup["MotionCommand List"].Add(data);

            data = new DataViewModel
            {
                CommandName = "RPM",
                CommandId = "4",
                CommandSubId = "10",
                CommandValue = "",
                IsFloat = false,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(4, 10), data);
            DataCommandsListbySubGroup["MotionCommand List"].Add(data);

            data = new DataViewModel
            {
                CommandName = "Speed Position [C/S]",
                CommandId = "5",
                CommandSubId = "2",
                CommandValue = "",
                IsFloat = false,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(5, 2), data);
            DataCommandsListbySubGroup["MotionCommand List"].Add(data);

            data = new DataViewModel
            {
                CommandName = "Position Absolute [C]",
                CommandId = "5",
                CommandSubId = "0",
                CommandValue = "",
                IsFloat = false,
                IsSelected = false,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(5, 0), data);
            DataCommandsListbySubGroup["MotionCommand List"].Add(data);

            data = new DataViewModel
            {
                CommandName = "Position Relative [C]",
                CommandId = "5",
                CommandSubId = "1",
                CommandValue = "",
                IsFloat = false,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(5, 1), data);
            DataCommandsListbySubGroup["MotionCommand List"].Add(data);

            data = new DataViewModel
            {
                CommandName = "Accelaration [C/S^2]",
                CommandId = "54",
                CommandSubId = "3",
                CommandValue = "",
                IsFloat = false,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(54, 3), data);
            DataCommandsListbySubGroup["MotionCommand List2"].Add(data);

            data = new DataViewModel
            {
                CommandName = "PTP Speed [C/S]",
                CommandId = "54",
                CommandSubId = "2",
                CommandValue = "",
                IsFloat = false,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(54, 2), data);
            DataCommandsListbySubGroup["MotionCommand List2"].Add(data);

            data = new DataViewModel
            {
                CommandName = "Max Tracking Err [C]",
                CommandId = "54",
                CommandSubId = "6",
                CommandValue = "",
                IsFloat = false,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(54, 6), data);
            DataCommandsListbySubGroup["MotionCommand List2"].Add(data);
            #endregion Commands1
            #region Commands2
            var ProfilerModeEnum = new List<string>
              {
                  "PID",
                  "Trapezoid"
              };
            Enums.Add("Profiler Mode", ProfilerModeEnum);

            var ProfilerModeCmd = new EnumViewModel
            {
                CommandName = "Profiler Mode",
                CommandId = "54",
                CommandSubId = "1",
                CommandList = Enums["Profiler Mode"],
                CommandValue = "1",//first enum in list
            };
            //DataViewCommandsList.Add(new Tuple<int, int>(54, 1), ProfilerModeCmd);
            EnumViewCommandsList.Add(new Tuple<int, int>(54, 1), ProfilerModeCmd);
            EnumCommandsListbySubGroup.Add("Profiler Mode", new ObservableCollection<object>
            {
              ProfilerModeCmd
            });
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

            var SignalgeneratorTypeCmd = new EnumViewModel
            {
                CommandName = "Type",
                CommandId = "7",
                CommandSubId = "1",
                CommandList = Enums["S.G.Type"],
                CommandValue = "1",//first enum in list start at 0
            };
            //DataViewCommandsList.Add(new Tuple<int, int>(7, 1), SignalgeneratorTypeCmd);
            EnumViewCommandsList.Add(new Tuple<int, int>(7, 1), SignalgeneratorTypeCmd);
            EnumCommandsListbySubGroup.Add("S.G.Type", new ObservableCollection<object>
            {
              SignalgeneratorTypeCmd
            });
            #endregion Commands3
            #region Commands4
            DataCommandsListbySubGroup.Add("S.G.List", new ObservableCollection<object>());

            data = new DataViewModel
            {
                CommandName = "Offset",
                CommandId = "7",
                CommandSubId = "5",
                CommandValue = "",
                IsFloat = false,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(7, 5), data);
            DataCommandsListbySubGroup["S.G.List"].Add(data);

            data = new DataViewModel
            {
                CommandName = "Frequency [Hz]",
                CommandId = "7",
                CommandSubId = "6",
                CommandValue = "",
                IsFloat = true,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(7, 6), data);
            DataCommandsListbySubGroup["S.G.List"].Add(data);


            #endregion Commands4
            #region Commands5
            DataCommandsListbySubGroup.Add("PowerOut List", new ObservableCollection<object>());
            data = new DataViewModel
            {
                CommandName = "PowerOut",
                CommandId = "12",
                CommandSubId = "1",
                CommandValue = "",
                IsFloat = false,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(12, 1), data);
            DataCommandsListbySubGroup["PowerOut List"].Add(data);
            #endregion Commands5
            #region Status_1
            DataCommandsListbySubGroup.Add("MotionStatus List", new ObservableCollection<object>());

            data = new DataViewModel
            {
                CommandName = "PWM %",
                CommandId = "30",
                CommandSubId = "2",
                CommandValue = "",
                IsFloat = true,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(30, 2), data);
            DataCommandsListbySubGroup["MotionStatus List"].Add(data);

            data = new DataViewModel
            {
                CommandName = "Speed Fdb",
                CommandId = "25",
                CommandSubId = "0",
                CommandValue = "",
                IsFloat = false,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(25, 0), data);
            DataCommandsListbySubGroup["MotionStatus List"].Add(data);

            data = new DataViewModel
            {
                CommandName = "RPM",
                CommandId = "25",
                CommandSubId = "10",
                CommandValue = "",
                IsFloat = false,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(25, 10), data);
            DataCommandsListbySubGroup["MotionStatus List"].Add(data);

            data = new DataViewModel
            {
                CommandName = "IQ Current [A]",
                CommandId = "30",
                CommandSubId = "0",
                CommandValue = "",
                IsFloat = true,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(30, 0), data);
            DataCommandsListbySubGroup["MotionStatus List"].Add(data);

            data = new DataViewModel
            {
                CommandName = "ID Current [A]",
                CommandId = "30",
                CommandSubId = "1",
                CommandValue = "",
                IsFloat = true,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(30, 1), data);
            DataCommandsListbySubGroup["MotionStatus List"].Add(data);

            data = new DataViewModel
            {
                CommandName = "Ia",
                CommandId = "30",
                CommandSubId = "10",
                CommandValue = "",
                IsFloat = true,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(30, 10), data);
            DataCommandsListbySubGroup["MotionStatus List"].Add(data);

            data = new DataViewModel
            {
                CommandName = "Ib",
                CommandId = "30",
                CommandSubId = "11",
                CommandValue = "",
                IsFloat = true,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(30, 11), data);
            DataCommandsListbySubGroup["MotionStatus List"].Add(data);

            data = new DataViewModel
            {
                CommandName = "Ic",
                CommandId = "30",
                CommandSubId = "12",
                CommandValue = "",
                IsFloat = true,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(30, 12), data);
            DataCommandsListbySubGroup["MotionStatus List"].Add(data);

            data = new DataViewModel
            {
                CommandName = "Temperature [C]",
                CommandId = "32",
                CommandSubId = "1",
                CommandValue = "",
                IsFloat = true,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(32, 1), data);
            DataCommandsListbySubGroup["MotionStatus List"].Add(data);
            #endregion Status_1
            #region Status2

            DigitalInputListbySubGroup.Add("Digital Input List", new ObservableCollection<object>());
            var names = new[]
            {
                "Input 1", "Input 2", "Input 3", "Input 4"
            };
            for(int i = 1; i < 5; i++)
            {
                var input = new BoolViewIndModel
                {
                    CommandName = names[i - 1],
                    CommandValue = 0,
                    CommandId = "29",
                    CommandSubId = i.ToString(),
                    IsFloat = false
                };
                DigitalInputList.Add(new Tuple<int, int>(29, i), input);
                DigitalInputListbySubGroup["Digital Input List"].Add(input);
            }
            #endregion Status2
            #region Status_3
            DataCommandsListbySubGroup.Add("Position counters List", new ObservableCollection<object>());
            data = new DataViewModel
            {
                CommandName = "Main",
                CommandId = "26",
                CommandSubId = "0",
                CommandValue = "",
                IsFloat = false,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(26, 0), data);
            DataCommandsListbySubGroup["Position counters List"].Add(data);

            data = new DataViewModel
            {
                CommandName = "Hall",
                CommandId = "26",
                CommandSubId = "1",
                CommandValue = "",
                IsFloat = false,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(26, 1), data);
            DataCommandsListbySubGroup["Position counters List"].Add(data);

            data = new DataViewModel
            {
                CommandName = "Motor",
                CommandId = "26",
                CommandSubId = "2",
                CommandValue = "",
                IsFloat = false,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(26, 2), data);
            DataCommandsListbySubGroup["Position counters List"].Add(data);

            data = new DataViewModel
            {
                CommandName = "External",
                CommandId = "26",
                CommandSubId = "3",
                CommandValue = "",
                IsFloat = false,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(26, 3), data);
            DataCommandsListbySubGroup["Position counters List"].Add(data);
            #endregion Status_3
        }
        public void GenerateLPCommands()
        {
            #region Commands1
            DataCommandsListbySubGroup.Add("LPCommands List", new ObservableCollection<object>());

            var data = new DataViewModel
            {
                CommandName = "SN",
                CommandId = "62",
                CommandSubId = "1",
                CommandValue = "",
                IsFloat = false,
            };
            DataViewCommandsListLP.Add(new Tuple<int, int>(62, 1), data);
            DataCommandsListbySubGroup["LPCommands List"].Add(data);

            data = new DataViewModel
            {
                CommandName = "HW Rev",
                CommandId = "62",
                CommandSubId = "2",
                CommandValue = "",
                IsFloat = true,
            };
            DataViewCommandsListLP.Add(new Tuple<int, int>(62, 2), data);
            DataCommandsListbySubGroup["LPCommands List"].Add(data);

            data = new DataViewModel
            {
                CommandName = "FW Rev",
                CommandId = "62",
                CommandSubId = "3",
                CommandValue = "",
                IsFloat = false,
            };
            DataViewCommandsListLP.Add(new Tuple<int, int>(62, 3), data);
            DataViewCommandsList.Add(new Tuple<int, int>(62, 3), data);
            DataCommandsListbySubGroup["LPCommands List"].Add(data);
            #endregion Commands1
            #region Commands2
            //var ProfilerModeEnum = new List<string>
            //  {
            //    "uRayon",
            //    "Rayon 30A",
            //    "uRayon SB",
            //    "Rayon HP",
            //    "Rayon MK6",
            //    "Rayon 70A"

            //  };
            //Enums.Add("Driver Type", ProfilerModeEnum);

            //var ProfilerModeCmd = new EnumViewModel
            //{
            //    CommandName = "Driver Type",
            //    CommandId = "62",
            //    CommandSubId = "0",
            //    CommandList = Enums["Driver Type"],
            //    CommandValue = "1",//first enum in list
            //};
            //DataViewCommandsListLP.Add(new Tuple<int, int>(62, 0), ProfilerModeCmd);
            //EnumViewCommandsList.Add(new Tuple<int, int>(62, 0), ProfilerModeCmd);
            //EnumCommandsListbySubGroup.Add("Driver Type", new ObservableCollection<object>
            //{
            //  ProfilerModeCmd
            //});
            #endregion Commands2
            #region Command3
            DataCommandsListbySubGroup.Add("DriverStatus List", new ObservableCollection<object>());
            data = new DataViewModel
            {
                CommandName = "Driver Status",
                CommandId = "33",
                CommandSubId = "1",
                CommandValue = "",
                IsFloat = false,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(33, 1), data);
            DataCommandsListbySubGroup["DriverStatus List"].Add(data);

            data = new DataViewModel
            {
                CommandName = "MotorStatus",
                CommandId = "1",
                CommandSubId = "0",
                CommandValue = "",
                IsFloat = false,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(1, 0), data);
            DataCommandsListbySubGroup["MotionStatus List"].Add(data);
            #endregion Command3
        }
        private void GenerateMotionTabCommands()
        {
            DataCommandsListbySubGroup.Add("CurrentLimit List", new ObservableCollection<object>());

            var names = new[]
            {
                "Continuous Current Limit [A]", "Peak Current Limit [A]", "Peak Time [sec]", "PWM limit [%]"
            };

            for(int i = 0; i < names.Length; i++)
            {
                var data = new DataViewModel
                {
                    CommandName = names[i],
                    CommandId = "52",
                    CommandSubId = (i + 1).ToString(CultureInfo.InvariantCulture),
                    CommandValue = "",
                    IsFloat = true,
                };

                DataViewCommandsList.Add(new Tuple<int, int>(52, i + 1), data);
                DataCommandsListbySubGroup["CurrentLimit List"].Add(data);
            }
        }
        private void GenerateIOTabCommands()
        {
            DataCommandsListbySubGroup.Add("AnalogCommand List", new ObservableCollection<object>());

            var names = new[]
            {
                "Ampere/Volt", "RPM/Volt", "Counts/Volt", "Offset", "Dead Zone", "Direction", "LPF Cut-Off"
            };
            for(int i = 0; i < names.Length; i++)
            {
                var data = new DataViewModel
                {
                    CommandName = names[i],
                    CommandId = "110",
                    CommandSubId = (i).ToString(CultureInfo.InvariantCulture),
                    CommandValue = "",
                    IsFloat = i == 5 ? false : true,
                };

                DataViewCommandsList.Add(new Tuple<int, int>(110, i), data);
                DataCommandsListbySubGroup["AnalogCommand List"].Add(data);
            }
        }
        private void GenerateBodeCommands()
        {
            DataCommandsListbySubGroup.Add("DataBodeList", new ObservableCollection<object>());
            DataCommandsListbySubGroup.Add("BodeListBackGround", new ObservableCollection<object>());

            string[] names = { "Control Loop", "Frequency Start", "Frequency End", "Amplitude", "PointsDec" };
            bool[] type = { false, true, true, true, false, false };

            for(int i = 1; i < names.Length; i++)
            {
                var data = new DataViewModel
                {
                    CommandName = names[i],
                    CommandId = "15",
                    CommandSubId = (i + 1).ToString(CultureInfo.InvariantCulture),
                    CommandValue = "",
                    IsFloat = type[i]
                };

                DataViewCommandsList.Add(new Tuple<int, int>(15, i + 1), data);
                DataCommandsListbySubGroup["DataBodeList"].Add(data);
            }

            var data1 = new DataViewModel
            {
                CommandName = "Status",
                CommandId = "6",
                CommandSubId = (15).ToString(CultureInfo.InvariantCulture),
                CommandValue = "",
                IsFloat = false
            };
            //DataViewCommandsList.Add(new Tuple<int, int>(6, 15), data1);
            DataCommandsListbySubGroup["BodeListBackGround"].Add(data1);

            
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

            var enum1 = new EnumViewModel
            {
                CommandName = "Control Loop",
                CommandId = "15",
                CommandSubId = "1",
                CommandList = Enums["Control Loop"],
                CommandValue = "1",//first enum in list
            };

            EnumViewCommandsList.Add(new Tuple<int, int>(15, 1), enum1);
            EnumCommandsListbySubGroup.Add("EnumBodeList", new ObservableCollection<object>{ enum1 });

            tmp1 = new List<string>
              {
                "Bode Current",
                "Bode Speed",
                "Bode Position"
            };
            Enums.Add("Bode Fdbck", tmp1);

            enum1 = new EnumViewModel
            {
                CommandName = "Bode Fdbck",
                CommandId = "15",
                CommandSubId = "6",
                CommandList = Enums["Bode Fdbck"],
                CommandValue = "0",//first enum in list
            };

            EnumViewCommandsList.Add(new Tuple<int, int>(15, 6), enum1);
            EnumCommandsListbySubGroup["EnumBodeList"].Add(enum1);
        }
        private void GenerateMaintenanceList()
        {
            var data = new DataViewModel
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
                data = new DataViewModel
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
            DataCommandsListbySubGroup.Add("UpperMainPan List", new ObservableCollection<object>());

            var data = new DataViewModel
            {
                CommandName = "CH1",
                CommandId = "60",
                CommandSubId = "1",
                CommandValue = "",
                IsFloat = false,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(60, 1), data);
            DataCommandsListbySubGroup["UpperMainPan List"].Add(data);

            data = new DataViewModel
            {
                CommandName = "CH2",
                CommandId = "60",
                CommandSubId = "2",
                CommandValue = "",
                IsFloat = false,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(60, 2), data);
            DataCommandsListbySubGroup["UpperMainPan List"].Add(data);

        }
        private void CalibrationCmd()
        {
            CalibartionCommandsListbySubGroup.Add("Calibration List", new ObservableCollection<object>());
            CalibartionCommandsListbySubGroup.Add("Calibration Result List", new ObservableCollection<object>());

            var names = new[]
            {
                "Current Offset", "PI Current Loop", "Hall Mapping", "Feedback Direction", "PI Speed Loop", "PI Position Loop", "Abs. Enc."
            };
            for(int i = 0; i < names.Length; i++) // Calibration Button
            {
                var data = new CalibrationButtonModel
                {
                    CommandName = names[i],
                    CommandId = "6",
                    CommandSubId = (i * 2 + 1).ToString(CultureInfo.InvariantCulture),
                    CommandValue = "",
                    IsFloat = false,
                };
                CalibartionCommandsList.Add(new Tuple<int, int>(6, Convert.ToInt16(i * 2 + 1)), data);
                CalibartionCommandsListbySubGroup["Calibration List"].Add(data);

                var TextBoxResult = new DataViewModel
                {
                    CommandName = names[i],
                    CommandId = "6",
                    CommandSubId = (i * 2 + 2).ToString(CultureInfo.InvariantCulture),
                    CommandValue = "",
                    IsFloat = false,
                };
                DataViewCommandsList.Add(new Tuple<int, int>(6, Convert.ToInt16(i * 2 + 2)), TextBoxResult);
                CalibartionCommandsListbySubGroup["Calibration Result List"].Add(TextBoxResult);
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
            DebugCommandsListbySubGroup.Add("Debug List", new ObservableCollection<object>());

            DataCommandsListbySubGroup.Add("InternalParam List", new ObservableCollection<object>());
            #region Operation
            var data_b = new DataViewModel
            {
                CommandName = "Checksum",
                CommandId = "62",
                CommandSubId = "10",
                CommandValue = "",
                IsFloat = false,
            };
            DataViewCommandsList.Add(new Tuple<int, int>(62, 10), data_b);
            DataCommandsListbySubGroup["InternalParam List"].Add(data_b);

            #endregion Operation
        }
    }
}

