//#define REFRESH_MANAGER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MotorController.CommandsDB;
using MotorController.ViewModels;
using System.Collections.ObjectModel;
using MotorController.Views;
using System.Collections;
using MotorController.Helpers;
using System.Diagnostics;
using System.Windows.Threading; // For Dispatcher.
using System.Windows;
using System.Windows.Media;
using MotorController.Models.ParserBlock;

namespace MotorController.Models.DriverBlock
{

    public class RefreshManger
    {
        public static int tab = ParametarsWindowViewModel.TabControlIndex;
        private static readonly object Synlock = new object();
        static readonly object _refresh_lock = new object();

        private static RefreshManger _instance;

        public static Dictionary<string, ObservableCollection<object>> BuildGroup = new Dictionary<string, ObservableCollection<object>>();
        public static Dictionary<Tuple<int, int>, DataViewModel> BuildList = new Dictionary<Tuple<int, int>, DataViewModel>();
        public static Dictionary<Tuple<int, int>, object> BuildGenericCommandsList = new Dictionary<Tuple<int, int>, object>();

        public static int TempTab = 0;
        public static RefreshManger GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new RefreshManger();
                    buildGroup();
                    return _instance;
                }
            }
        }

        public static void buildGroup()
        {
            BuildGroup = new Dictionary<string, ObservableCollection<object>>();
            BuildList = new Dictionary<Tuple<int, int>, DataViewModel>();
            //var DataViewlist = CommandsDB.Commands.GetInstance.DataViewCommandsList;
            //var EnumViewlist = CommandsDB.Commands.GetInstance.EnumViewCommandsList;
            var AllDataList = Commands.GetInstance.DataCommandsListbySubGroup;
            var AllEnumList = Commands.GetInstance.EnumCommandsListbySubGroup;
            var AllCalList = Commands.GetInstance.CalibartionCommandsListbySubGroup;
            var AllBoolList = Commands.GetInstance.DigitalInputListbySubGroup;
            var AllDebugList = Commands.GetInstance.DebugCommandsListbySubGroup;
            var AllToggleSwitchList = Commands.GetInstance.ToggleSwitchList;

            foreach(var list in AllEnumList)
            {
                BuildGroup.Add(list.Key, new ObservableCollection<object>());
                foreach(var sub_list in list.Value)
                {
                    var data = new DataViewModel
                    {
                        CommandName = ((DataViewModel)sub_list).CommandName,
                        CommandId = ((DataViewModel)sub_list).CommandId,
                        CommandSubId = ((DataViewModel)sub_list).CommandSubId,
                        CommandValue = ((DataViewModel)sub_list).CommandValue,
                        IsFloat = ((DataViewModel)sub_list).IsFloat,
                    };
                    BuildGroup[list.Key].Add(data);
                }
            }
            foreach(var list in AllDataList)
            {
                BuildGroup.Add(list.Key, new ObservableCollection<object>());
                foreach(var sub_list in list.Value)
                {
                    var data = new DataViewModel
                    {
                        CommandName = ((DataViewModel)sub_list).CommandName,
                        CommandId = ((DataViewModel)sub_list).CommandId,
                        CommandSubId = ((DataViewModel)sub_list).CommandSubId,
                        CommandValue = ((DataViewModel)sub_list).CommandValue,
                        IsFloat = ((DataViewModel)sub_list).IsFloat,
                    };
                    BuildGroup[list.Key].Add(data);
                }
            }
            foreach(var list in AllCalList)
            {
                BuildGroup.Add(list.Key, new ObservableCollection<object>());
                foreach(var sub_list in list.Value)
                {
                    if(list.Key == "Calibration List")
                    {
                        CalibrationButtonModel temp = sub_list as CalibrationButtonModel;
                        string CommandName = temp.CommandName;
                        string CommandId = temp.CommandId;
                        string CommandSubId = temp.CommandSubId;
                        bool IsFloat = temp.IsFloat;
                        var data = new DataViewModel
                        {
                            CommandName = CommandName,
                            CommandId = CommandId,
                            CommandSubId = CommandSubId,
                            IsFloat = IsFloat,
                        };
                        BuildGroup[list.Key].Add(data);
                    }
                    else if(list.Key == "Calibration Result List")
                    {
                        CalibrationWizardViewModel temp = sub_list as CalibrationWizardViewModel;
                        string CommandName = temp.CalibrationName;
                        string CommandId = temp.CommandId;
                        string CommandSubId = temp.CommandSubId;
                        var data = new DataViewModel
                        {
                            CommandName = CommandName,
                            CommandId = CommandId,
                            CommandSubId = CommandSubId
                        };
                        BuildGroup[list.Key].Add(data);
                    }
                    else
                    {
                        var data = new DataViewModel
                        {
                            CommandName = ((DataViewModel)sub_list).CommandName,
                            CommandId = ((DataViewModel)sub_list).CommandId,
                            CommandSubId = ((DataViewModel)sub_list).CommandSubId,
                            CommandValue = ((DataViewModel)sub_list).CommandValue,
                            IsFloat = ((DataViewModel)sub_list).IsFloat,
                        };
                        BuildGroup[list.Key].Add(data);
                    }
                }
            }
            foreach(var list in AllBoolList)
            {
                BuildGroup.Add(list.Key, new ObservableCollection<object>());
                foreach(var sub_list in list.Value)
                {
                    if(list.Key == "Digital Input List")
                    {
                        BoolViewIndModel temp = sub_list as BoolViewIndModel;
                        string CommandName = temp.CommandName;
                        string CommandId = temp.CommandId;
                        string CommandSubId = temp.CommandSubId;
                        bool IsFloat = temp.IsFloat;
                        var data = new DataViewModel
                        {
                            CommandName = CommandName,
                            CommandId = CommandId,
                            CommandSubId = CommandSubId,
                            IsFloat = IsFloat,
                        };
                        BuildGroup[list.Key].Add(data);
                    }
                    else
                    {
                        var data = new DataViewModel
                        {
                            CommandName = ((DataViewModel)sub_list).CommandName,
                            CommandId = ((DataViewModel)sub_list).CommandId,
                            CommandSubId = ((DataViewModel)sub_list).CommandSubId,
                            CommandValue = ((DataViewModel)sub_list).CommandValue,
                            IsFloat = ((DataViewModel)sub_list).IsFloat,
                        };
                        BuildGroup[list.Key].Add(data);
                    }
                }
            }
            foreach(var list in AllDebugList)
            {
                BuildGroup.Add(list.Key, new ObservableCollection<object>());
                foreach(var sub_list in list.Value)
                {
                    DebugObjModel temp = sub_list as DebugObjModel;
                    string CommandName = "";
                    string CommandId = temp.ID;
                    string CommandSubId = temp.Index;
                    bool IsFloat = temp.IntFloat;
                    var data = new DataViewModel
                    {
                        CommandName = CommandName,
                        CommandId = CommandId,
                        CommandSubId = CommandSubId,
                        IsFloat = !IsFloat,
                    };
                    BuildGroup[list.Key].Add(data);
                }
            }
            foreach(var list in AllToggleSwitchList)
            {
                BuildGroup.Add(list.Key, new ObservableCollection<object>());
                foreach(var sub_list in list.Value)
                {
                    UC_ToggleSwitchViewModel temp = sub_list as UC_ToggleSwitchViewModel;
                    string CommandName = temp.Label;
                    string CommandId = temp.CommandId.ToString();
                    string CommandSubId = temp.CommandSubId.ToString();
                    var data = new DataViewModel
                    {
                        CommandName = CommandName,
                        CommandId = CommandId,
                        CommandSubId = CommandSubId
                    };
                    BuildGroup[list.Key].Add(data);
                }
            }
        }
        public string[] GroupToExecute(int tabIndex)//
        {
            string[] PanelElements = new string[] { "DriverStatus List", "Channel List", "MotionCommand List2", "MotionCommand List",
                                                    "Profiler Mode", "S.G.List", "S.G.Type", "PowerOut List",
                                                    "MotionStatus List", "Digital Input List", "Position counters List",
                                                    "UpperMainPan List", "LPCommands List", "ChannelsList" };// , "Driver Type" ,
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
                        arr = new string[] { "PIDCurrent", "PIDSpeed", "PIDPosition", "PID_speed_loop", "PID_current_loop", "PID_position_loop"/*, "PIDListBackGround"*/ };
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
                        if(WizardWindowViewModel.GetInstance.StartEnable)
                            arr = new string[] { "Calibration Result List", "Calibration List", "CalibrationList_ToggleSwitch" };
                        break;
                    case (int)eTab.BODE:
                        arr = new string[] { "DataBodeList", "BodeStart", "EnumBodeList" };

                        break;
                    default:
                        return PanelElements;
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
                                        if(!BuildGenericCommandsList.ContainsKey(new Tuple<int, int>(Convert.ToInt16(((DataViewModel)_data).CommandId), Convert.ToInt16(((DataViewModel)_data).CommandSubId))))
                                            BuildGenericCommandsList.Add(new Tuple<int, int>(Convert.ToInt16(((DataViewModel)_data).CommandId), Convert.ToInt16(((DataViewModel)_data).CommandSubId)), (EnumViewModel)_data);
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
                    //TempTab = tab;
                    //if(DebugViewModel.updateList)
                    //   DebugViewModel.updateList = false;
                    LeftPanelViewModel.GetInstance.cancelRefresh = new CancellationToken(false);
                }
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
                    catch(Exception e) { }
                }
            }
        }
        public void StartRefresh_old()
        {
            if(Rs232Interface._comPort != null)
            {
                if(Rs232Interface._comPort.IsOpen)
                {
                    //tab = Views.ParametarsWindow.ParametersWindowTabSelected;
                    //if(ParametarsWindow.WindowsOpen == false)
                    //    tab = -1;
#if REFRESH_MANAGER
                Debug.WriteLine("StartRefresh: " + DateTime.Now.ToString("h:mm:ss.fff"));
#endif
                    if(tab != TempTab || DebugViewModel.updateList)
                    {
                        BuildList = new Dictionary<Tuple<int, int>, DataViewModel>();
                        foreach(var list in BuildGroup)
                        {
                            if(GroupToExecute(tab).Contains(list.Key))
                            {
                                foreach(var sub_list in list.Value)
                                {
                                    var data = new DataViewModel
                                    {
                                        CommandName = ((DataViewModel)sub_list).CommandName,
                                        CommandId = ((DataViewModel)sub_list).CommandId,
                                        CommandSubId = ((DataViewModel)sub_list).CommandSubId,
                                        CommandValue = ((DataViewModel)sub_list).CommandValue,
                                        IsFloat = ((DataViewModel)sub_list).IsFloat,
                                        IsSelected = ((DataViewModel)sub_list).IsSelected,
                                    };
                                    if(!BuildList.ContainsKey(new Tuple<int, int>(Int32.Parse(data.CommandId), Int32.Parse(data.CommandSubId))))
                                    {
                                        BuildList.Add(new Tuple<int, int>(Int32.Parse(((DataViewModel)sub_list).CommandId), Int32.Parse(((DataViewModel)sub_list).CommandSubId)), data);
                                    }
                                }
                            }
                        }
                        TempTab = tab;
                        if(DebugViewModel.updateList)
                            DebugViewModel.updateList = false;
#if REFRESH_MANAGER
                    Debug.WriteLine(" --- Tab --- ");
#endif
                    }
                    if(BuildList.Count == 0)
                    {
                        if(DebugViewModel.GetInstance.DebugRefresh)
                            DebugViewModel.GetInstance.DebugRefresh = false;
                        TempTab = -1;
                        return;
                    }

                    if(_iteratorRefresh < 0)
                        _iteratorRefresh = BuildList.Count - 1;

                    int element = _iteratorRefresh--;
                    if(element < BuildList.Count && element > -1)
                    {
                        if(!BuildList.ElementAt(element).Value.IsSelected)
                        {
                            Rs232Interface.GetInstance.SendToParser(new PacketFields
                            {
                                Data2Send = BuildList.ElementAt(element).Value.CommandValue,
                                ID = Convert.ToInt16(BuildList.ElementAt(element).Value.CommandId),
                                SubID = Convert.ToInt16(BuildList.ElementAt(element).Value.CommandSubId),
                                IsSet = false,
                                IsFloat = BuildList.ElementAt(element).Value.IsFloat
                            });
                            if(BuildList.Count > 0)
                            {
                                if(Commands.GetInstance.DataViewCommandsList.ContainsKey(
                                    new Tuple<int, int>(Convert.ToInt16(BuildList.ElementAt(element).Value.CommandId), Convert.ToInt16(BuildList.ElementAt(element).Value.CommandSubId))))
                                    Commands.GetInstance.DataViewCommandsList[
                                        new Tuple<int, int>(Convert.ToInt16(BuildList.ElementAt(element).Value.CommandId), Convert.ToInt16(BuildList.ElementAt(element).Value.CommandSubId))].GetCount++;
                            }
                        }
                    }

#if REFRESH_MANAGER
                Debug.WriteLine("EndRefresh: " + DateTime.Now.ToString("h:mm:ss.fff"));
#endif
                }
            }
        }
        public static int ConnectionCount = 0;
        //public bool _oneSelected = false;
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
                    else if(ConnectionCount > 7)
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
        //private void MouseLeaveCommandFunc()
        //{
        //    RefreshManger.DataPressed = false;
        //    foreach(var list in Commands.GetInstance.DataViewCommandsList)
        //    {
        //        try
        //        {
        //            Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(Convert.ToInt16(list.Value.CommandId), Convert.ToInt16(list.Value.CommandSubId))].IsSelected = false;
        //            Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(Convert.ToInt16(list.Value.CommandId), Convert.ToInt16(list.Value.CommandSubId))].Background2 = new SolidColorBrush(Colors.White);
        //            Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(Convert.ToInt16(list.Value.CommandId), Convert.ToInt16(list.Value.CommandSubId))].Background = new SolidColorBrush(Colors.Gray);
        //        }
        //        catch(Exception e)
        //        {

        //        }
        //    }

        //    MotorController.Models.DriverBlock.RefreshManger.GetInstance._oneSelected = false;
        //}
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
        string CalibrationGetError(string returnedValue)
        {
            switch(Convert.ToInt32(returnedValue))
            {
                //uint16_t hallFeedlErr:1;         // 0x01 - 1
                //uint16_t encPhaseErr:1;          // 0x02 - 2
                //uint16_t encoderHallMismach:1;  // 0x04  - 4
                //uint16_t overTemperature:1;     // 0x08  - 8
                //uint16_t overVoltage:1;         // 0x010 - 16
                //uint16_t underVoltage:1;        //0x20   - 32
                //uint16_t speedRangeErr:1;       //0x40   - 64
                //uint16_t positionErr:1;         //0x80   - 128
                //uint16_t gateDriverFault:1;     //0x0100 - 256
                //uint16_t nOCTW:1;               //0x0200 - 512
                //uint16_t gateDriverInit:1;      //0x0400 - 1024
                //uint16_t motorStall:1;           //0x0800 - 2048
                //uint16_t Reserved3:1;           //0x1000 - 4096
                //uint16_t Reserved4:1;           //0x2000 - 8192
                //uint16_t ADCoffset:1;           //0x4000 - 16384
                //uint16_t FetShort:1;            //0x8000 - 32768
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

        string YAxisUnits(string channel, string returnedValue)
        {
            string value = "";
            switch(Convert.ToInt32(returnedValue))
            {
                case 0:
                    value = "[A]";
                    break;
                case 1:
                    value = "[V]";
                    break;
                case 5:
                    value = "[Elec_Angle]";
                    break;
                case 6:
                    value = "[Mech_Angle]";
                    break;
                case 10:
                    value = "[RPM/V]";
                    break;
                case 11:
                    value = "[Count/Sec]";
                    break;
                case 12:
                    value = "[Round/Min]";
                    break;
                case 13:
                    value = "[Counts]";
                    break;
                default:
                    value = "N/A";
                    break;
            }
            return channel + ": " + value;
        }
        //public static int CalibrationTimeOut = 10;
        //private static int PrecedentIdx = 0;
        public bool DisconnectedFlag = false;
        public void YAxisLegend(int Sel, int identifier)
        {
            if(LeftPanelViewModel.GetInstance.StarterOperationFlag)
            {
                if(Sel <= OscilloscopeViewModel.GetInstance.Channel1SourceItems.Count && Sel <= OscilloscopeViewModel.GetInstance.Channel2SourceItems.Count)
                {
                    try
                    {
                        OscilloscopeViewModel.GetInstance.YAxisUnits = "CH" + identifier.ToString() + ": " + OscilloscopeViewModel.GetInstance.ChannelYtitles.First(x => x.Key == OscilloscopeViewModel.GetInstance.SelectedCh1DataSource).Value;
                    }
                    catch(Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
            }
            else if(Sel < OscilloscopeViewModel.GetInstance.ChannelYtitles.Count)
                OscilloscopeViewModel.GetInstance.YAxisUnits = "CH" + identifier.ToString() + ": " + OscilloscopeViewModel.GetInstance.ChannelYtitles.ElementAt(Sel).Value;
        }
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
                //LeftPanelViewModel.GetInstance.ValueChange = false;
                if(/*DebugViewModel.GetInstance.EnRefresh || LeftPanelViewModel.GetInstance.StarterOperationFlag ||*/ true)
                {
                    #region Calibration
                    if(commandidentifier.Item1 == 6666)
                    {
                        //if(Commands.GetInstance.CalibrationWizardCommands.ContainsKey(new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)))
                        //    Commands.GetInstance.CalibrationWizardCommands[new Tuple<int, int>(6, commandidentifier.Item2)].CalibStatus = CalibStatus(newPropertyValue);

                        //int ind = Commands.GetInstance.CalibartionCommandsListbySubGroup["Calibration Result List"].IndexOf(new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2));

                        //if(Commands.GetInstance.CalibartionCommandsList.ContainsKey(new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)))
                        //{
                        //    if(Convert.ToInt16(newPropertyValue) == 0)
                        //        Commands.GetInstance.CalibartionCommandsList[new Tuple<int, int>(6, commandidentifier.Item2)].ButtonContent = "Run";
                        //    else if(Convert.ToInt16(newPropertyValue) == 1)
                        //        Commands.GetInstance.CalibartionCommandsList[new Tuple<int, int>(6, commandidentifier.Item2)].ButtonContent = "Running";
                        //}
                        //else 
                        //if(Commands.GetInstance.DataViewCommandsList.ContainsKey(new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)))
                        //    Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandValue = CalibrationGetStatus(newPropertyValue);

                        //else 
                        if(commandidentifier.Item1 == 6 && commandidentifier.Item2 == 15) // StartStop Bode
                        {
                            if(newPropertyValue == "1")
                                BodeViewModel.GetInstance.BodeStartStop = true;
                            else
                                BodeViewModel.GetInstance.BodeStartStop = false;
                        }

                        if(WizardWindowViewModel.GetInstance.CalibrationWizardList.ContainsKey(new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)))
                            WizardWindowViewModel.GetInstance.updateCalibrationStatus(new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2), newPropertyValue);
                    }
                    else if(Commands.GetInstance.GenericCommandsList.ContainsKey(new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)))
                    {
                        //((DataViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)]).CommandValue = newPropertyValue;
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
                                ((UC_ToggleSwitchViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)]).IsChecked = Convert.ToBoolean(Convert.ToInt16(newPropertyValue));
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
                                //OscilloscopeViewModel.GetInstance.YAxisUnits = "";
                                string temp = "";
                                for(int j = 0; j < Commands.GetInstance.GenericCommandsGroup["ChannelsList"].Count; j++)
                                {
                                    temp += ((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsList[
                                        new Tuple<int, int>(
                                            ((int)((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsGroup["ChannelsList"][j]).CommandId),
                                            ((int)((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsGroup["ChannelsList"][j]).CommandSubId))]).Y_Axis_Title;
                                    if(j != Commands.GetInstance.GenericCommandsGroup["ChannelsList"].Count - 1)
                                        temp +=  ", ";
                                }
                                OscilloscopeViewModel.GetInstance.YAxisUnits = temp;
                                break;
                        }
                    }
                    if(commandidentifier.Item1 == 1 && commandidentifier.Item2 == 0 && !DebugViewModel.GetInstance._forceConnectMode) // MotorStatus
                    {
                        updateConnectionStatus(commandidentifier, newPropertyValue);
                    }
                    else if(commandidentifier.Item1 == 33)
                    {
                        LeftPanelViewModel.GetInstance.DriverStat = CalibrationGetError(newPropertyValue);
                    }
                    else if(WizardWindowViewModel.GetInstance.CalibrationWizardList.ContainsKey(new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)))
                        WizardWindowViewModel.GetInstance.updateCalibrationStatus(new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2), newPropertyValue);
                    #region DebugTab
                    if(DebugViewModel.GetInstance.DebugRefresh || DebugObjModel.DebugOperationPending)
                    {
                        if(Commands.GetInstance.DebugCommandsList.ContainsKey(new Tuple<int, int, bool>(commandidentifier.Item1, commandidentifier.Item2, IntFloat))) // Debug Panel
                        {
                            try
                            {
                                Commands.GetInstance.DebugCommandsList[new Tuple<int, int, bool>(commandidentifier.Item1, commandidentifier.Item2, IntFloat)].GetData = newPropertyValue;
                                DebugViewModel.GetInstance.RxBuildOperation(ParserRayonM1.DebugData);
                                DebugObjModel.DebugOperationPending = false;
                            }
                            catch { }
                        }
                    }
                    #endregion DebugTab
                    return;
                    #endregion Calibration
                    #region Plot_Channels
                    /*else*/
                    if(commandidentifier.Item1 == 60 && commandidentifier.Item2 <= 2)
                    {
                        int Sel = 0;
                        if(Int32.Parse(newPropertyValue) >= 0)
                        {
                            Sel = Int32.Parse(newPropertyValue);

                            /*if(LeftPanelViewModel.GetInstance.StarterOperationFlag && !DisconnectedFlag) // At first application run
                            {
                                if(Sel <= OscilloscopeViewModel.GetInstance.Channel1SourceItems.Count && Sel <= OscilloscopeViewModel.GetInstance.Channel2SourceItems.Count && Sel >= 0)
                                {
                                    if(commandidentifier.Item2 == 1)
                                    {
                                        OscilloscopeViewModel.GetInstance.Ch1SelectedIndex = Sel;
                                        OscilloscopeViewModel.GetInstance.SelectedCh1DataSource = OscilloscopeViewModel.GetInstance.Channel1SourceItems.ElementAt(Sel);
                                        YAxisLegend(Sel, commandidentifier.Item2);
                                    }
                                    else if(commandidentifier.Item2 == 2)
                                    {
                                        OscilloscopeViewModel.GetInstance.Ch2SelectedIndex = Sel;
                                        OscilloscopeViewModel.GetInstance.SelectedCh2DataSource = OscilloscopeViewModel.GetInstance.Channel2SourceItems.ElementAt(Sel);
                                        YAxisLegend(Sel, commandidentifier.Item2);
                                    }
                                }
                            }
                            elseif(DisconnectedFlag) // When user click on disconnect button or conncetion lost
                            {
                                if(commandidentifier.Item2 == 1)
                                {
                                    OscilloscopeViewModel.GetInstance.Ch1SelectedIndex = Sel;
                                    if(OscilloscopeViewModel.GetInstance.Channel1SourceItems.Count > 0)
                                        OscilloscopeViewModel.GetInstance.SelectedCh1DataSource = OscilloscopeViewModel.GetInstance.Channel1SourceItems.ElementAt(Sel);
                                }
                                else if(commandidentifier.Item2 == 2)
                                {
                                    OscilloscopeViewModel.GetInstance.Ch2SelectedIndex = Sel;
                                    if(OscilloscopeViewModel.GetInstance.Channel2SourceItems.Count > 0)
                                        OscilloscopeViewModel.GetInstance.SelectedCh2DataSource = OscilloscopeViewModel.GetInstance.Channel2SourceItems.ElementAt(Sel);
                                }
                            }
                            else*/
                            {
                                if(Sel <= OscilloscopeViewModel.GetInstance.ChannelYtitles.Count && OscilloscopeViewModel.GetInstance.ChannelYtitles.Count > 0)
                                {
                                    if(commandidentifier.Item2 == 1)
                                    {
                                        OscilloscopeViewModel.GetInstance.Ch1SelectedIndex = Sel;
                                        OscilloscopeViewModel.GetInstance.SelectedCh1DataSource = OscilloscopeViewModel.GetInstance.Channel1SourceItems.ElementAt(Sel);
                                        if(!LeftPanelViewModel.GetInstance.StarterOperationFlag)
                                            ch1 = Sel;
                                    }
                                    else if(commandidentifier.Item2 == 2)
                                    {
                                        OscilloscopeViewModel.GetInstance.Ch2SelectedIndex = Sel;
                                        OscilloscopeViewModel.GetInstance.SelectedCh2DataSource = OscilloscopeViewModel.GetInstance.Channel2SourceItems.ElementAt(Sel);
                                        if(!LeftPanelViewModel.GetInstance.StarterOperationFlag)
                                            ch2 = Sel;
                                    }
                                    YAxisLegend(Sel, commandidentifier.Item2);
                                }
                            }
                        }
                    }
                    #endregion Plot_Channels
                    #region DataView_EnumView
                    else if(commandidentifier.Item1 == 33)
                    {
                        LeftPanelViewModel.GetInstance.DriverStat = CalibrationGetError(newPropertyValue);
                    }
                    else if(commandidentifier.Item1 == 12 && commandidentifier.Item2 == 1) // Power Output Command
                    {
                        if(newPropertyValue == "1")
                            BottomPanelViewModel.GetInstance.PowerOutputChecked = true;
                        else
                            BottomPanelViewModel.GetInstance.PowerOutputChecked = false;

                    }
                    else if(commandidentifier.Item1 == 1 && commandidentifier.Item2 == 0 && !DebugViewModel.GetInstance._forceConnectMode) // MotorStatus
                    {
                        updateConnectionStatus(commandidentifier, newPropertyValue);
                    }
                    else if(Commands.GetInstance.DataViewCommandsList.ContainsKey(new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2))
                        && Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].IsSelected == false)
                    {
                        Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandValue = newPropertyValue;
                        if(commandidentifier.Item1 == 62 && commandidentifier.Item2 < 3)
                            Commands.GetInstance.DataViewCommandsListLP[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandValue = newPropertyValue;
                        else if(commandidentifier.Item1 == 62 && commandidentifier.Item2 == 10)
                            Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandValue = "0x" + int.Parse(newPropertyValue).ToString("X8");
                    }
                    else if(Commands.GetInstance.EnumViewCommandsList.ContainsKey(new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)))
                    {
                        try
                        {
                            int index = Convert.ToInt32(newPropertyValue) - Convert.ToInt32(Commands.GetInstance.EnumViewCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandValue);
                            if(index < Commands.GetInstance.EnumViewCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandList.Count && index >= 0)
                            {
                                Commands.GetInstance.EnumViewCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].SelectedItem =
                                Commands.GetInstance.EnumViewCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandList[index];
                            }
                        }
                        catch
                        {
                        }
                    }
                    #endregion DataView_EnumView
                    #region DigitalInput
                    else if(Commands.GetInstance.DigitalInputList.ContainsKey(new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)))
                    {
                        Commands.GetInstance.DigitalInputList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandValue = Convert.ToInt16(newPropertyValue) == 1 ? 1 : 0;
                    }
                    #endregion DigitalInput
                    #region PID
                    //else if((commandidentifier.Item1 == 81 || commandidentifier.Item1 == 82 || commandidentifier.Item1 == 83) && commandidentifier.Item2 == 10) // PID
                    //{
                    //    switch(commandidentifier.Item1)
                    //    {
                    //        case 81:
                    //            PIDViewModel.GetInstance.PID_current_loop = newPropertyValue == "1" ? true : false;
                    //            break;
                    //        case 82:
                    //            PIDViewModel.GetInstance.PID_speed_loop = newPropertyValue == "1" ? true : false;
                    //            break;
                    //        case 83:
                    //            PIDViewModel.GetInstance.PID_position_loop = newPropertyValue == "1" ? true : false;
                    //            break;
                    //    }
                    //}
                    //else if(commandidentifier.Item1 == 78 && commandidentifier.Item2 == 1)
                    //{
                    //    FeedBackViewModel.GetInstance.External_interpolation = newPropertyValue == "1" ? true : false;
                    //}
                    //else if(commandidentifier.Item1 == 101 && commandidentifier.Item2 == 0) // Filter Enable
                    //{
                    //    FilterViewModel.GetInstance.Filter_Enable = newPropertyValue == "1" ? true : false;
                    //}
                    #endregion PID
                }
                #region ToggleSwitch
                if(Commands.GetInstance.ToggleSwitchCommands.ContainsKey(new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)))
                {
                    try
                    {
                        Commands.GetInstance.ToggleSwitchCommands[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].IsChecked = Convert.ToBoolean(Convert.ToInt16(newPropertyValue));
                    }
                    catch
                    {
                    }
                }
                #endregion ToggleSwitch
                //else if(commandidentifier.Item1 == 1 && commandidentifier.Item2 == 0 && !DebugViewModel.GetInstance._forceConnectMode) // MotorStatus
                //{
                //    updateConnectionStatus(commandidentifier, newPropertyValue);
                //}
                /*
                #region StartUpAPP
                else if(commandidentifier.Item1 == 62)
                {
                    switch(commandidentifier.Item2)
                    {
                        case 0:
                            Commands.GetInstance.DataViewCommandsListLP[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandValue = newPropertyValue;
                            break;
                        case 1:
                            Commands.GetInstance.DataViewCommandsListLP[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandValue = newPropertyValue;
                            break;
                        case 2:
                            Commands.GetInstance.DataViewCommandsListLP[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandValue = newPropertyValue;
                            break;
                        case 3:
                            Commands.GetInstance.DataViewCommandsListLP[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandValue = newPropertyValue;
                            break;
                        case 10:
                            Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandValue = "0x" + int.Parse(newPropertyValue).ToString("X8");
                            break;
                    }
                }
                #endregion StartUpAPP
                */
                //}
                #region DebugTab
                if(DebugViewModel.GetInstance.DebugRefresh || DebugObjModel.DebugOperationPending)
                {
                    if(Commands.GetInstance.DebugCommandsList.ContainsKey(new Tuple<int, int, bool>(commandidentifier.Item1, commandidentifier.Item2, IntFloat))) // Debug Panel
                    {
                        try
                        {
                            Commands.GetInstance.DebugCommandsList[new Tuple<int, int, bool>(commandidentifier.Item1, commandidentifier.Item2, IntFloat)].GetData = newPropertyValue;
                            DebugViewModel.GetInstance.RxBuildOperation(ParserRayonM1.DebugData);
                            DebugObjModel.DebugOperationPending = false;
                        }
                        catch { }
                    }
                }
                #endregion DebugTab
            }
            catch(Exception error)
            {
                Debug.WriteLine(error.Message);
            }
        }

        public void updateConnectionStatus(Tuple<int, int> commandidentifier, string newPropertyValue)
        {
            if(LeftPanelViewModel.GetInstance.ConnectTextBoxContent == "Connected")
            {
                if(newPropertyValue == "1" || newPropertyValue == "0")
                {
                    ConnectionCount = 0;
                    LeftPanelViewModel.GetInstance.LedMotorStatus = Convert.ToInt16(newPropertyValue);
                    //Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandValue = newPropertyValue;
                    ((UC_ToggleSwitchViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)]).IsChecked = Convert.ToBoolean(Convert.ToInt16(newPropertyValue));

                    //Debug.WriteLine("ConnectionCount: " + ConnectionCount + " " + DateTime.Now.ToString("h:mm:ss.fff"));

                }
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
