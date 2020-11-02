//#define REFRESH_MANAGER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MotorController.Common;
using MotorController.ViewModels;
using MotorController.Views;
using MotorController.Helpers;
using System.Diagnostics;
using MotorController.Models.ParserBlock;
using MotorController.Models.DriverBlock;

namespace MotorController.Models
{
    public class RefreshManager
    {
        public static int tab = ParametarsWindowViewModel.TabControlIndex;
        private static readonly object Synlock = new object();
        static readonly object _refresh_lock = new object();

        private static RefreshManager _instance;

        public static Dictionary<Tuple<int, int>, object> BuildGenericCommandsList = new Dictionary<Tuple<int, int>, object>();

        public static int TempTab = 0;
        public static RefreshManager GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new RefreshManager();
                    return _instance;
                }
            }
        }

        public string[] GroupToExecute(int tabIndex)
        {
            string[] PanelElements = new string[] { "DriverStatus List", "Channel List", "MotionCommand List2", "MotionCommand List",
                                                    "Profiler Mode", "S.G.List", "S.G.Type", "BP_ToggleSwitch",
                                                    "MotionStatus List", "Digital Input List", "Position counters List",
                                                    "UpperMainPan List", "LPCommands List", "ChannelsList", "MotorControl" };
            string[] arr = new string[] { };
            if(DebugViewModel.GetInstance.EnRefresh)
            {
                switch(tabIndex)
                {
                    case (int)eTab.CONTROL:
                        arr = new string[] { "Control", "Motor", "Motion Limit", "CurrentLimit List" };
                        break;
                    case (int)eTab.FEED_BACKS:
                        arr = new string[] { "Hall", "FeedbackSync", "Feedback Sync", "Qep1", "Qep2", "SSI_Feedback", "Qep1Bis", "Qep2Bis" };
                        break;
                    case (int)eTab.PID:
                        arr = new string[] { "PIDCurrent", "PIDSpeed", "PIDPosition", "PID_speed_loop", "PID_current_loop", "PID_position_loop" };
                        break;
                    case (int)eTab.FILTER:
                        arr = new string[] { "FilterList", "Filter_Enable" };
                        break;
                    case (int)eTab.DEVICE:
                        arr = new string[] { "DeviceSerial", "BaudrateList" };
                        break;
                    case (int)eTab.I_O:
                        arr = new string[] { "AnalogCommand List" };
                        break;
                    case (int)eTab.CALIBRATION:
                        arr = new string[] { "Calibration Result List", "CalibrationList_ToggleSwitch" };
                        break;
                    case (int)eTab.BODE:
                        arr = new string[] { "DataBodeList", "BodeStart", "EnumBodeList" };
                        break;
                    case (int)eTab.MAINTENANCE:
                        arr = new string[] { "MaintenanceOperation"};
                        break;
                    default:
                        break;
                }
                arr = arr.Concat(PanelElements).ToArray();
            }
            if(DebugViewModel.GetInstance.DebugRefresh)
            {
                switch(tabIndex)
                {
                    case (int)eTab.DEBUG:
                        arr = arr.Concat(new string[] { "Debug List" }).ToArray();
                        break;
                    default:
                        break;
                }
            }
            if(/*!WizardWindowViewModel.GetInstance.StartEnable*/ WizardWindowViewModel.calibration_is_in_process && tabIndex != (int)eTab.CALIBRATION)
                arr = arr.Concat(new string[] { "Calibration Result List", "CalibrationList_ToggleSwitch" }).ToArray();
            if(LeftPanelViewModel.GetInstance._wizard_window != null)
            {
                if(LeftPanelViewModel.GetInstance._wizard_window.Visibility == System.Windows.Visibility.Visible)
                    arr = arr.Concat(new string[] { "WizardOperation" }).ToArray();
            }
            Debug.WriteLine("");
            Debug.Write(DateTime.Now.ToString());
            for(int i = 0; i < arr.Length; i++)
                Debug.Write(arr[i] + " - ");
            Debug.WriteLine("");

            return arr;
        }

        private int _iteratorRefresh = 0;
        public void BuildGenericCommandsList_Func()
        {
            lock(_refresh_lock)
            {
                _iteratorRefresh = -1;
                tab = ParametarsWindowViewModel.TabControlIndex;
                //if(ParametarsWindow.WindowsOpen == false)
                //    tab = -1;
                /*else*/
                //if(ParametarsWindowViewModel.TabControlIndex == -1)
                //    tab = 0;
                StackTrace _stck = new StackTrace();
                Debug.WriteLine("BuildGenericCommandsList_Func called - tab = " + tab + "TempTab = " + TempTab);

                if(tab != TempTab || DebugViewModel.updateList)
                {
                    BuildGenericCommandsList = new Dictionary<Tuple<int, int>, object>();
                    string[] _groups_to_execute = GroupToExecute(tab);
                    for(int i = 0; i < _groups_to_execute.Length; i++)
                    {
                        if(Commands.GetInstance.GenericCommandsGroup.ContainsKey(_groups_to_execute[i]))
                            foreach(var _data in Commands.GetInstance.GenericCommandsGroup[_groups_to_execute[i]])
                            {
                                switch(_data.GetType().Name)
                                {
                                    case "DataViewModel":
                                        if(!BuildGenericCommandsList.ContainsKey(new Tuple<int, int>(Convert.ToInt16(((DataViewModel)_data).CommandId), Convert.ToInt16(((DataViewModel)_data).CommandSubId))))
                                            BuildGenericCommandsList.Add(new Tuple<int, int>(Convert.ToInt16(((DataViewModel)_data).CommandId), Convert.ToInt16(((DataViewModel)_data).CommandSubId)), (DataViewModel)_data);
                                        break;
                                    case "EnumViewModel":
                                        if(!BuildGenericCommandsList.ContainsKey(new Tuple<int, int>(Convert.ToInt16(((EnumViewModel)_data).CommandId), Convert.ToInt16(((EnumViewModel)_data).CommandSubId))))
                                            BuildGenericCommandsList.Add(new Tuple<int, int>(Convert.ToInt16(((EnumViewModel)_data).CommandId), Convert.ToInt16(((EnumViewModel)_data).CommandSubId)), (EnumViewModel)_data);
                                        break;
                                    case "UC_ToggleSwitchViewModel":
                                        if(!BuildGenericCommandsList.ContainsKey(new Tuple<int, int>(Convert.ToInt16(((UC_ToggleSwitchViewModel)_data).CommandId), Convert.ToInt16(((UC_ToggleSwitchViewModel)_data).CommandSubId))))
                                            BuildGenericCommandsList.Add(new Tuple<int, int>(Convert.ToInt16(((UC_ToggleSwitchViewModel)_data).CommandId), Convert.ToInt16(((UC_ToggleSwitchViewModel)_data).CommandSubId)), (UC_ToggleSwitchViewModel)_data);
                                        break;
                                    case "CalibrationWizardViewModel":
                                        if(!BuildGenericCommandsList.ContainsKey(new Tuple<int, int>(Convert.ToInt16(((CalibrationWizardViewModel)_data).CommandId), Convert.ToInt16(((CalibrationWizardViewModel)_data).CommandSubId))))
                                            BuildGenericCommandsList.Add(new Tuple<int, int>(Convert.ToInt16(((CalibrationWizardViewModel)_data).CommandId), Convert.ToInt16(((CalibrationWizardViewModel)_data).CommandSubId)), (CalibrationWizardViewModel)_data);
                                        break;
                                    case "BoolViewIndModel":
                                        if(!BuildGenericCommandsList.ContainsKey(new Tuple<int, int>(Convert.ToInt16(((BoolViewIndModel)_data).CommandId), Convert.ToInt16(((BoolViewIndModel)_data).CommandSubId))))
                                            BuildGenericCommandsList.Add(new Tuple<int, int>(Convert.ToInt16(((BoolViewIndModel)_data).CommandId), Convert.ToInt16(((BoolViewIndModel)_data).CommandSubId)), (BoolViewIndModel)_data);
                                        break;
                                    case "UC_ChannelViewModel":
                                        if(!BuildGenericCommandsList.ContainsKey(new Tuple<int, int>(Convert.ToInt16(((UC_ChannelViewModel)_data).CommandId), Convert.ToInt16(((UC_ChannelViewModel)_data).CommandSubId))))
                                            BuildGenericCommandsList.Add(new Tuple<int, int>(Convert.ToInt16(((UC_ChannelViewModel)_data).CommandId), Convert.ToInt16(((UC_ChannelViewModel)_data).CommandSubId)), (UC_ChannelViewModel)_data);
                                        break;
                                }
                            }
                    }
                    TempTab = tab;
                    if(DebugViewModel.updateList)
                       DebugViewModel.updateList = false;
                }
                LeftPanelViewModel.GetInstance.cancelRefresh = new CancellationToken(false);
            }
        }
        public void StartRefresh()
        {
            if(LeftPanelViewModel.GetInstance.cancelRefresh.IsCancellationRequested)
                return;

            if(BuildGenericCommandsList.Count == 0)
            {
                if(DebugViewModel.GetInstance.DebugRefresh)
                    DebugViewModel.GetInstance.DebugRefresh = false;
                TempTab = -1;
                return;
            }

            lock(_refresh_lock)
            {
                if(_iteratorRefresh < 0)
                    _iteratorRefresh = BuildGenericCommandsList.Count - 1;

                int element = _iteratorRefresh--;
                if(element < BuildGenericCommandsList.Count && element > -1)
                {
                    try
                    {
                        switch(BuildGenericCommandsList.Values.ToList().ElementAt(element).GetType().Name)
                        {
                            case "DataViewModel":
                                if(!((DataViewModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).IsSelected)
                                {
                                    Rs232Interface.GetInstance.SendToParser(new PacketFields
                                    {
                                        ID = Convert.ToInt16(((DataViewModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).CommandId),
                                        SubID = Convert.ToInt16(((DataViewModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).CommandSubId),
                                        IsSet = false,
                                        IsFloat = ((DataViewModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).IsFloat
                                    });
                                    ((DataViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(
                                            Convert.ToInt16(((DataViewModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).CommandId),
                                            Convert.ToInt16(((DataViewModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).CommandSubId))]).GetCount++;
                                }
                                break;
                            case "EnumViewModel":
                                if(!((EnumViewModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).IsSelected)
                                {
                                    Rs232Interface.GetInstance.SendToParser(new PacketFields
                                    {
                                        ID = Convert.ToInt16(((EnumViewModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).CommandId),
                                        SubID = Convert.ToInt16(((EnumViewModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).CommandSubId),
                                        IsSet = false,
                                        IsFloat = false
                                    });
                                    ((EnumViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(
                                            Convert.ToInt16(((EnumViewModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).CommandId),
                                            Convert.ToInt16(((EnumViewModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).CommandSubId))]).GetCount++;
                                }
                                break;
                            case "UC_ToggleSwitchViewModel":
                                Rs232Interface.GetInstance.SendToParser(new PacketFields
                                {
                                    ID = Convert.ToInt16(((UC_ToggleSwitchViewModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).CommandId),
                                    SubID = Convert.ToInt16(((UC_ToggleSwitchViewModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).CommandSubId),
                                    IsSet = false,
                                    IsFloat = false
                                });
                                ((UC_ToggleSwitchViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(
                                        Convert.ToInt16(((UC_ToggleSwitchViewModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).CommandId),
                                        Convert.ToInt16(((UC_ToggleSwitchViewModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).CommandSubId))]).GetCount++;
                                break;
                            case "CalibrationWizardViewModel":
                                Rs232Interface.GetInstance.SendToParser(new PacketFields
                                {
                                    ID = Convert.ToInt16(((CalibrationWizardViewModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).CommandId),
                                    SubID = Convert.ToInt16(((CalibrationWizardViewModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).CommandSubId),
                                    IsSet = false,
                                    IsFloat = false
                                });
                                ((CalibrationWizardViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(
                                        Convert.ToInt16(((CalibrationWizardViewModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).CommandId),
                                        Convert.ToInt16(((CalibrationWizardViewModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).CommandSubId))]).GetCount++;
                                break;
                            case "BoolViewIndModel":
                                Rs232Interface.GetInstance.SendToParser(new PacketFields
                                {
                                    ID = Convert.ToInt16(((BoolViewIndModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).CommandId),
                                    SubID = Convert.ToInt16(((BoolViewIndModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).CommandSubId),
                                    IsSet = false,
                                    IsFloat = false
                                });
                                ((BoolViewIndModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(
                                        Convert.ToInt16(((BoolViewIndModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).CommandId),
                                        Convert.ToInt16(((BoolViewIndModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).CommandSubId))]).GetCount++;
                                break;
                            case "UC_ChannelViewModel":
                                Rs232Interface.GetInstance.SendToParser(new PacketFields
                                {
                                    ID = Convert.ToInt16(((UC_ChannelViewModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).CommandId),
                                    SubID = Convert.ToInt16(((UC_ChannelViewModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).CommandSubId),
                                    IsSet = false,
                                    IsFloat = false
                                });
                                ((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(
                                        Convert.ToInt16(((UC_ChannelViewModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).CommandId),
                                        Convert.ToInt16(((UC_ChannelViewModel)BuildGenericCommandsList.Values.ToList().ElementAt(element)).CommandSubId))]).GetCount++;
                                break;
                        }
                    }
                    catch { }
                }
            }
        }
        //        public void StartRefresh_old()
        //        {
        //            if(Rs232Interface._comPort != null)
        //            {
        //                if(Rs232Interface._comPort.IsOpen)
        //                {
        //                    tab = Views.ParametarsWindow.ParametersWindowTabSelected;
        //                    if(ParametarsWindow.WindowsOpen == false)
        //                        tab = -1;
        //#if REFRESH_MANAGER
        //                Debug.WriteLine("StartRefresh: " + DateTime.Now.ToString("h:mm:ss.fff"));
        //#endif
        //                    if(tab != TempTab || DebugViewModel.updateList)
        //                    {
        //                        BuildList = new Dictionary<Tuple<int, int>, DataViewModel>();
        //                        foreach(var list in BuildGroup)
        //                        {
        //                            if(GroupToExecute(tab).Contains(list.Key))
        //                            {
        //                                foreach(var sub_list in list.Value)
        //                                {
        //                                    var data = new DataViewModel
        //                                    {
        //                                        CommandName = ((DataViewModel)sub_list).CommandName,
        //                                        CommandId = ((DataViewModel)sub_list).CommandId,
        //                                        CommandSubId = ((DataViewModel)sub_list).CommandSubId,
        //                                        CommandValue = ((DataViewModel)sub_list).CommandValue,
        //                                        IsFloat = ((DataViewModel)sub_list).IsFloat,
        //                                        IsSelected = ((DataViewModel)sub_list).IsSelected,
        //                                    };
        //                                    if(!BuildList.ContainsKey(new Tuple<int, int>(Int32.Parse(data.CommandId), Int32.Parse(data.CommandSubId))))
        //                                    {
        //                                        BuildList.Add(new Tuple<int, int>(Int32.Parse(((DataViewModel)sub_list).CommandId), Int32.Parse(((DataViewModel)sub_list).CommandSubId)), data);
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        TempTab = tab;
        //                        if(DebugViewModel.updateList)
        //                            DebugViewModel.updateList = false;
        //#if REFRESH_MANAGER
        //                    Debug.WriteLine(" --- Tab --- ");
        //#endif
        //                    }
        //                    if(BuildList.Count == 0)
        //                    {
        //                        if(DebugViewModel.GetInstance.DebugRefresh)
        //                            DebugViewModel.GetInstance.DebugRefresh = false;
        //                        TempTab = -1;
        //                        return;
        //                    }

        //                    if(_iteratorRefresh < 0)
        //                        _iteratorRefresh = BuildList.Count - 1;

        //                    int element = _iteratorRefresh--;
        //                    if(element < BuildList.Count && element > -1)
        //                    {
        //                        if(!BuildList.ElementAt(element).Value.IsSelected)
        //                        {
        //                            Rs232Interface.GetInstance.SendToParser(new PacketFields
        //                            {
        //                                Data2Send = BuildList.ElementAt(element).Value.CommandValue,
        //                                ID = Convert.ToInt16(BuildList.ElementAt(element).Value.CommandId),
        //                                SubID = Convert.ToInt16(BuildList.ElementAt(element).Value.CommandSubId),
        //                                IsSet = false,
        //                                IsFloat = BuildList.ElementAt(element).Value.IsFloat
        //                            });
        //                            if(BuildList.Count > 0)
        //                            {
        //                                if(Commands.GetInstance.DataViewCommandsList.ContainsKey(
        //                                    new Tuple<int, int>(Convert.ToInt16(BuildList.ElementAt(element).Value.CommandId), Convert.ToInt16(BuildList.ElementAt(element).Value.CommandSubId))))
        //                                    Commands.GetInstance.DataViewCommandsList[
        //                                        new Tuple<int, int>(Convert.ToInt16(BuildList.ElementAt(element).Value.CommandId), Convert.ToInt16(BuildList.ElementAt(element).Value.CommandSubId))].GetCount++;
        //                            }
        //                        }
        //                    }

        //#if REFRESH_MANAGER
        //                Debug.WriteLine("EndRefresh: " + DateTime.Now.ToString("h:mm:ss.fff"));
        //#endif
        //                }
        //            }
        //        }
        public static int ConnectionCount = 0;
        public void VerifyConnection(object sender, EventArgs e)
        {
            if(Rs232Interface._comPort != null)
            {
                if(Rs232Interface._comPort.IsOpen)
                {
                    /* Sync command */
                    Rs232Interface.GetInstance.SendToParser(new PacketFields
                    {
                        Data2Send = "1",
                        ID = Convert.ToInt16(64),
                        SubID = Convert.ToInt16(0),
                        IsSet = true,
                        IsFloat = false
                    });

                    /* Motor Status */
                    Rs232Interface.GetInstance.SendToParser(new PacketFields
                    {
                        Data2Send = "",
                        ID = Convert.ToInt16(1),
                        SubID = Convert.ToInt16(0),
                        IsSet = false,
                        IsFloat = false
                    });
                    ConnectionCount++;
                    //if(ConnectionCount > 0)
                    //Debug.WriteLine("Send: " + ConnectionCount + " " + DateTime.Now.ToString("h:mm:ss.fff"));
                    if(ConnectionCount > 5 && ConnectionCount < 7)
                    {
                        EventRiser.Instance.RiseEevent(string.Format($"No communication with Driver"));
                        Rs232Interface.GetInstance.Disconnect(1);
                    }
                    else if(ConnectionCount >= 8 && ConnectionCount < 10)
                        Rs232Interface.GetInstance.Disconnect(1);
                }
                else if(Rs232Interface.GetInstance.IsSynced)
                {
                    EventRiser.Instance.RiseEevent(string.Format($"Serial cable disconnected from PC"));
                    Rs232Interface.GetInstance.Disconnect(0);
                }

                else
                {
                    EventRiser.Instance.RiseEevent(string.Format($"Communication Lost"));
                    Rs232Interface.GetInstance.Disconnect(0);
                }
            }
        }
        string CalibrationGetStatus(string returnedValue)
        {
            switch(Convert.ToInt16(returnedValue))
            {
                case 0:
                    return "Idle";
                case 1:
                    return "In Process";
                case 2:
                    return "Failure";
                case 3:
                    return "Success";
                default:
                    return "No Info(" + returnedValue + ")";
            }
        }
        int CalibStatus(string returnedValue)
        {
            int StateTemp = 0;
            switch(Convert.ToInt16(returnedValue))
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
            return StateTemp;
        }
        public string CalibrationGetError(string returnedValue)
        {
            switch(Convert.ToInt32(returnedValue))
            {
                case 0:
                    return "All OK !";
                case 1:
                    return "Hall Error";
                case 2:
                    return "Parameters Checksum Error";
                case 4:
                    return "Encoder/Hall Sync";
                case 8:
                    return "Over Temperature";
                case 16:
                    return "Over Voltage";
                case 32:
                    return "Under Voltage";
                case 128:
                    return "Position Tracking";
                case 256:
                    return "Driver Power Init";
                case 512:
                    return "Driver Power C/T";
                case 1024:
                    return "Driver Power Fault";
                case 2048:
                    return "Motor Stall";
                case 4096:
                    return "Gate Disable";
                case 8192:
                    return "Driver OSC";
                case 16384:
                    return "Driver ADC Offset";
                case 32768:
                    return "Driver Short Test";
                case 65536:
                    return "STO";
                case 131072:
                    return "SSI Clock Not Enabled";
                default:
                    return "no info(" + returnedValue + ")";
            }
        }

        public bool DisconnectedFlag = false;

        public int ch1, ch2;
        internal void UpdateModel(Tuple<int, int> commandidentifier, string newPropertyValue, bool IntFloat = false)
        {
            try
            {
                if(LeftPanelViewModel.GetInstance.StarterOperationFlag)
                {
                    Debug.WriteLine("Starter: " + commandidentifier.Item1.ToString() + ' ' + commandidentifier.Item2.ToString());

                    switch(commandidentifier.Item1)
                    {
                        case 1:
                            LeftPanelViewModel.GetInstance.StarterCount += 1;
                            EventRiser.Instance.RiseEevent(string.Format($"Read motor status"));
                            LeftPanelViewModel.GetInstance.ConnectTextBoxContent = "Connected";
                            LeftPanelViewModel.GetInstance.StarterOperationFlag = false;
                            return;
                        case 60:
                            switch(commandidentifier.Item2)
                            {
                                case 1:
                                    LeftPanelViewModel.GetInstance.StarterCount += 1;
                                    EventRiser.Instance.RiseEevent(string.Format($"Read Ch2"));
                                    break;
                                case 2:
                                    LeftPanelViewModel.GetInstance.StarterCount += 1;
                                    EventRiser.Instance.RiseEevent(string.Format($"Read Ch1"));
                                    break;
                            }
                            break;
                        case 62:
                            switch(commandidentifier.Item2)
                            {
                                case 1:
                                    LeftPanelViewModel.GetInstance.StarterCount += 1;
                                    EventRiser.Instance.RiseEevent(string.Format($"Read SN"));
                                    break;
                                case 2:
                                    LeftPanelViewModel.GetInstance.StarterCount += 1;
                                    EventRiser.Instance.RiseEevent(string.Format($"Read HW Rev"));
                                    break;
                                case 3:
                                    LeftPanelViewModel.GetInstance.StarterCount += 1;
                                    EventRiser.Instance.RiseEevent(string.Format($"Read FW Rev"));
                                    break;
                                case 10:
                                    LeftPanelViewModel.GetInstance.StarterCount += 1;
                                    EventRiser.Instance.RiseEevent(string.Format($"Read Checksum"));
                                    break;
                            }
                            break;
                        default:
                            return;
                    }
                }
                if(Commands.GetInstance.GenericCommandsList.ContainsKey(new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)))
                {
                    Type _type = Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].GetType();
                    switch(_type.Name)
                    {
                        case "DataViewModel":
                            ((DataViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)]).CommandValue = newPropertyValue;
                            break;
                        case "CalibrationWizardViewModel":
                            ((CalibrationWizardViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)]).CalibStatus = CalibStatus(newPropertyValue);
                            break;
                        case "UC_ToggleSwitchViewModel":
                            ((UC_ToggleSwitchViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)]).IsChecked = Convert.ToBoolean(Convert.ToInt64(newPropertyValue));
                            break;
                        case "EnumViewModel":
                            int index = Convert.ToInt32(newPropertyValue) - Convert.ToInt32(((EnumViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)]).CommandValue);
                            if(index < ((EnumViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)]).CommandList.Count && index >= 0)
                            {
                                ((EnumViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)]).SelectedItem =
                                ((EnumViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)]).CommandList[index];
                            }
                            break;
                        case "BoolViewIndModel":
                            ((BoolViewIndModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)]).CommandValue = Convert.ToInt16(newPropertyValue) == 1 ? 1 : 0;
                            break;
                        case "UC_ChannelViewModel":
                            ((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)]).ChSelectedIndex = Convert.ToInt16(newPropertyValue);
                            string temp = "", sub_temp = "";
                            for(int j = 0; j < Commands.GetInstance.GenericCommandsGroup["ChannelsList"].Count; j++)
                            {
                                sub_temp = ((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsList[
                                    new Tuple<int, int>(
                                        ((int)((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsGroup["ChannelsList"][j]).CommandId),
                                        ((int)((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsGroup["ChannelsList"][j]).CommandSubId))]).Y_Axis_Title;
                                if(!(j == 0 || String.IsNullOrEmpty(sub_temp)))
                                    temp += ", ";
                                if(!String.IsNullOrEmpty(sub_temp))
                                    temp += sub_temp;
                            }
                            //OscilloscopeViewModel.GetInstance.YAxisUnits = temp;
                            break;
                    }
                }
                if(WizardWindowViewModel.GetInstance.CalibrationWizardList.ContainsKey(new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)))
                    WizardWindowViewModel.GetInstance.updateCalibrationStatus(new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2), newPropertyValue);
                #region DebugTab
                if(Commands.GetInstance.GenericCommandsGroup.ContainsKey("Debug List"))
                {
                    if(Commands.GetInstance.GenericCommandsGroup["Debug List"].Count > 0 && DebugObjModel.DebugOperationPending)
                    {
                        var data1 = new DebugObjModel
                        {
                            ID = commandidentifier.Item1.ToString(),
                            Index = commandidentifier.Item2.ToString()
                        };
                        for(int i = 0; i < Commands.GetInstance.GenericCommandsGroup["Debug List"].Count; i++)
                        {
                            if(DebugViewModel.GetInstance.CompareDebugObj(data1, Commands.GetInstance.GenericCommandsGroup["Debug List"][i] as DebugObjModel))
                            {
                                ((DebugObjModel)Commands.GetInstance.GenericCommandsGroup["Debug List"].ElementAt(i)).GetData = newPropertyValue;
                                DebugViewModel.GetInstance.RxBuildOperation(ParserRayonM1.DebugData);
                                DebugObjModel.DebugOperationPending = false;
                            }
                        }
                    }
                }
                #endregion DebugTab
            }
            catch(Exception error)
            {
                Debug.WriteLine(error.Message + " - Op:" + commandidentifier.Item1.ToString() + "[" + commandidentifier.Item2.ToString() + "]");
            }
        }

        public void updateConnectionStatus(bool newPropertyValue)
        {
            if(LeftPanelViewModel.GetInstance.ConnectTextBoxContent == "Connected")
            {
                ConnectionCount = 0;
                LeftPanelViewModel.GetInstance.LedMotorStatus = Convert.ToInt16(newPropertyValue);
            }
            else if(LeftPanelViewModel.GetInstance.ConnectTextBoxContent == "Not Connected" && !LeftPanelViewModel.GetInstance.StarterOperationFlag)
            {
                LeftPanelViewModel.GetInstance.ConnectTextBoxContent = "Connected";
                LeftPanelViewModel.GetInstance.BlinkLedsTicks(LeftPanelViewModel.STOP);
                LeftPanelViewModel.GetInstance.VerifyConnectionTicks(LeftPanelViewModel.STOP);
                LeftPanelViewModel.GetInstance.RefreshParamsTick(LeftPanelViewModel.STOP);
                LeftPanelViewModel.GetInstance.StarterOperation(LeftPanelViewModel.STOP);
                LeftPanelViewModel.GetInstance.StarterOperation(LeftPanelViewModel.START);
            }
        }
    }
}
