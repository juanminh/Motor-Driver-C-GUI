#define RELEASE_MODE
using Abt.Controls.SciChart;
using MotorController.Models.DriverBlock;
using MotorController.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Collections.ObjectModel;
using MotorController.CommandsDB;
using System.Threading;
using System.Collections.Specialized;
using System.ComponentModel;
using MotorController.Models.ParserBlock;
using MotorController.Views;
using Timer = System.Timers.Timer;

namespace MotorController.ViewModels
{
    internal class DebugViewModel : ViewModelBase
    {
        #region FIELDS
        private static readonly object Synlock = new object();
        private static DebugViewModel _instance;
        #endregion FIELDS

        public static DebugViewModel GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new DebugViewModel();
                    return _instance;
                }
            }
            set
            {
                _instance = value;
            }
        }
        private DebugViewModel()
        {
            initSim();
        }

        private ObservableCollection<object> _debugList;
        public ObservableCollection<object> DebugList
        {
            get
            {
                return Commands.GetInstance.DebugCommandsListbySubGroup["Debug List"];
            }
            set
            {
                _debugList = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<object> _nternalParamList;
        public ObservableCollection<object> InternalParamList
        {
            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["InternalParam List"];
            }
            set
            {
                _nternalParamList = value;
                OnPropertyChanged();
            }
        }

        private string _debugId = "";
        public string DebugID
        {
            get { return _debugId; }
            set { _debugId = value; OnPropertyChanged("DebugID"); }
        }
        private string _debugIndex = "";
        public string DebugIndex
        {
            get { return _debugIndex; }
            set { _debugIndex = value; OnPropertyChanged("DebugIndex"); }
        }
        private bool _debugIntFloat = true;
        public bool DebugIntFloat
        {
            get
            {
                return _debugIntFloat;
            }
            set
            {
                _debugIntFloat = value;
                OnPropertyChanged("DebugIntFloat");
            }
        }
#if !DEBUG || RELEASE_MODE
        private bool _enRefresh = true;
        private bool _debugRefresh = false;
        private bool _enPing = true;
#else
        private bool _enRefresh = false;
        private bool _debugRefresh = false;
        private bool _enPing = false;
#endif
        public bool EnRefresh
        {
            get
            {
                return _enRefresh;
            }
            set
            {
                _enRefresh = value;
                OnPropertyChanged("EnRefresh");
                if(value && LeftPanelViewModel._app_running)
                {
                    if(!DebugRefresh)
                    {
                        ///Thread bkgnd = new Thread(LeftPanelViewModel.GetInstance.BackGroundFunc);
                        ///bkgnd.Start();

                        LeftPanelViewModel.GetInstance.RefreshParamsTick(LeftPanelViewModel.START);
                    }
                    else
                        updateList = true;

                }
                else if(!value)
                {
                    foreach(var list in Commands.GetInstance.DataViewCommandsList)
                    {
                        try
                        {
                            Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(Convert.ToInt16(list.Value.CommandId), Convert.ToInt16(list.Value.CommandSubId))].IsSelected = false;
                            Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(Convert.ToInt16(list.Value.CommandId), Convert.ToInt16(list.Value.CommandSubId))].BackgroundStd = new SolidColorBrush(Colors.White);
                            Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(Convert.ToInt16(list.Value.CommandId), Convert.ToInt16(list.Value.CommandSubId))].BackgroundSmallFont = new SolidColorBrush(Colors.Gray);
                        }
                        catch(Exception)
                        {
                        }
                    }
                    if(DebugRefresh)
                        updateList = true;
                    else
                        LeftPanelViewModel.GetInstance.RefreshParamsTick(LeftPanelViewModel.STOP);
                }
            }
        }
        public bool DebugRefresh
        {
            get
            {
                return _debugRefresh;
            }
            set
            {
                _debugRefresh = value;
                OnPropertyChanged("DebugRefresh");
                if(value && LeftPanelViewModel._app_running)
                {
                    if(!EnRefresh)
                    {
                        ///Thread bkgnd = new Thread(LeftPanelViewModel.GetInstance.BackGroundFunc);
                        ///bkgnd.Start();

                        LeftPanelViewModel.GetInstance.RefreshParamsTick(LeftPanelViewModel.START);

                    }
                    else
                        updateList = true;
                }
                else if(!value)
                    if(EnRefresh)
                        updateList = true;
                    else
                        LeftPanelViewModel.GetInstance.RefreshParamsTick(LeftPanelViewModel.STOP);

            }
        }
        public bool EnPing
        {
            get
            {
                return _enPing;
            }
            set
            {
                _enPing = value;
                OnPropertyChanged("EnPing");
                if(value && LeftPanelViewModel._app_running)
                    LeftPanelViewModel.GetInstance.VerifyConnectionTicks(LeftPanelViewModel.START);
                else if(!value)
                    LeftPanelViewModel.GetInstance.VerifyConnectionTicks(LeftPanelViewModel.STOP);
            }
        }

        public static bool updateList = false;
        public ActionCommand addDebugOperation { get { return new ActionCommand(addDebugOperationCmd); } }
        private void addDebugOperationCmd()
        {

            if(udID.Data != "" && udIndex.Data != "")
            {
                if(!Commands.GetInstance.DebugCommandsList.ContainsKey(new Tuple<int, int, bool>(Convert.ToInt16(udID.Data), Convert.ToInt16(udIndex.Data), DebugIntFloat)))
                {
                    var data = new DebugObjModel
                    {
                        ID = udID.Data,
                        Index = udIndex.Data,
                        IntFloat = DebugIntFloat,
                        GetData = "",
                        SetData = "",
                    };
                    Commands.GetInstance.DebugCommandsList.Add(new Tuple<int, int, bool>(Convert.ToInt16(udID.Data), Convert.ToInt16(udIndex.Data), DebugIntFloat), data);
                    Commands.GetInstance.DebugCommandsListbySubGroup["Debug List"].Add(data);
                    RefreshManger.buildGroup();
                }
            }
        }
        public ActionCommand removeDebugOperation { get { return new ActionCommand(removeDebugOperationCmd); } }
        private bool CompareDebugObj(DebugObjModel first, DebugObjModel second)
        {
            if(first.ID == second.ID)
                if(first.Index == second.Index)
                    return true;
            return false;
        }
        private void removeDebugOperationCmd()
        {
            if(udID.Data != "" && udIndex.Data != "")
            {
                if(Commands.GetInstance.DebugCommandsList.ContainsKey(new Tuple<int, int, bool>(Convert.ToInt16(udID.Data), Convert.ToInt16(udIndex.Data), DebugIntFloat)))
                {
                    Commands.GetInstance.DebugCommandsList.Remove(new Tuple<int, int, bool>(Convert.ToInt16(udID.Data), Convert.ToInt16(udIndex.Data), DebugIntFloat));
                    var data1 = new DebugObjModel
                    {
                        ID = udID.Data,
                        Index = udIndex.Data,
                        IntFloat = DebugIntFloat,
                        GetData = "",
                        SetData = "",
                    };

                    for(int i = 0; i < Commands.GetInstance.DebugCommandsListbySubGroup["Debug List"].Count; i++)
                    {
                        if(CompareDebugObj(data1, Commands.GetInstance.DebugCommandsListbySubGroup["Debug List"].ElementAt(i) as DebugObjModel))
                        {
                            Commands.GetInstance.DebugCommandsListbySubGroup["Debug List"].RemoveAt(i);
                            RefreshManger.buildGroup();
                        }
                    }
                }
            }
        }
        public ActionCommand GetCS { get { return new ActionCommand(GetCSCmd); } }
        private void GetCSCmd()
        {
            Rs232Interface.GetInstance.SendToParser(new PacketFields
            {
                Data2Send = "",
                ID = Convert.ToInt16(62),
                SubID = Convert.ToInt16(10),
                IsSet = false,
                IsFloat = false
            });

        }
        public ActionCommand ClearDebugOp { get { return new ActionCommand(ClearDebugOpCmd); } }
        private void ClearDebugOpCmd()
        {
            Commands.GetInstance.DebugCommandsList.Clear();
            Commands.GetInstance.DebugCommandsListbySubGroup["Debug List"].Clear();
            RefreshManger.buildGroup();
        }

        #region In_Output_Parse
        public static CrcEventhandlerCalcHostFrameCrc CrcInputCalc = CrcBase.CalcHostFrameCrc;
        //Send data to controller
        public void TxBuildOperation(object Data2Send, Int16 Id, Int16 SubId, bool IsSet, bool IsFloat)
        {
            #region building
            byte[] temp;
            if(IsSet == false)
                temp = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
            else
                temp = new byte[11] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            char tempChar = (char)0;

            temp[0] = 0x49;           //PreambleLSByte
            temp[1] = 0x5d;           //PreambleMsbyte
            temp[2] = (byte)(Id);     // ID msb

            tempChar = (char)(tempChar | ((char)(((Id >> 8)) & 0x3F)) | (((char)(SubId & 0x3)) << 6));
            temp[3] = (byte)tempChar;
            tempChar = (char)0;
            tempChar = (char)(tempChar | (char)(SubId >> 2));
            temp[4] = (byte)tempChar;
            if(IsSet == false)        //Set//Get
                temp[4] |= (1 << 4);

            if(IsFloat)        //Float//Int
                temp[4] |= (1 << 5);

            // 1<<6  -- Color 1
            // 1<<7  -- Color 2

            if(IsSet == false)
            {
                //Risng up delegate , to call static function from CRC class
                ushort TempGetCrc = CrcInputCalc(temp.Take(5), 2);
                temp[5] = (byte)(TempGetCrc & 0xFF);
                temp[6] = (byte)((TempGetCrc >> 8) & 0xFF);
            }
            else
            {
                if(Data2Send is double)                                           //Data float
                {
                    var datvaluevalue = BitConverter.GetBytes((float)(Data2Send is double ? (double)Data2Send : 0));
                    temp[5] = (byte)(datvaluevalue[0]);
                    temp[6] = (byte)(datvaluevalue[1]);
                    temp[7] = (byte)(datvaluevalue[2]);
                    temp[8] = (byte)(datvaluevalue[3]);
                }
                else if(Data2Send is int)//Data int
                {
                    //Int32 transit =(Int32) Data2Send;
                    temp[5] = (byte)(((int)Data2Send & 0xFF));
                    temp[6] = (byte)(((int)Data2Send >> 8) & 0xFF);
                    temp[7] = (byte)(((int)Data2Send >> 16) & 0xFF);
                    temp[8] = (byte)(((int)Data2Send >> 24) & 0xFF);
                }
                else if(IsFloat)                                           //Data float
                {
                    var datvaluevalue = BitConverter.GetBytes((float)(float.Parse((string)Data2Send)));
                    float newPropertyValuef = System.BitConverter.ToSingle(datvaluevalue, 0);
                    temp[5] = (byte)(datvaluevalue[0]);
                    temp[6] = (byte)(datvaluevalue[1]);
                    temp[7] = (byte)(datvaluevalue[2]);
                    temp[8] = (byte)(datvaluevalue[3]);
                }
                else // String Value
                {
                    if(Data2Send.ToString().Length != 0)
                    {
                        if(Data2Send.ToString().IndexOf(".") != -1)
                            Data2Send = Data2Send.ToString().Substring(0, Data2Send.ToString().IndexOf("."));
                        try
                        {
                            var datvaluevalue = 0;
                            Int32.TryParse(Data2Send.ToString(), out datvaluevalue);
                            temp[5] = (byte)(((int)datvaluevalue & 0xFF));
                            temp[6] = (byte)(((int)datvaluevalue >> 8) & 0xFF);
                            temp[7] = (byte)(((int)datvaluevalue >> 16) & 0xFF);
                            temp[8] = (byte)(((int)datvaluevalue >> 24) & 0xFF);
                        }
                        catch
                        {
                            var datvaluevalue = UInt32.Parse(Data2Send.ToString());
                            temp[5] = (byte)(((int)datvaluevalue & 0xFF));
                            temp[6] = (byte)(((int)datvaluevalue >> 8) & 0xFF);
                            temp[7] = (byte)(((int)datvaluevalue >> 16) & 0xFF);
                            temp[8] = (byte)(((int)datvaluevalue >> 24) & 0xFF);
                        }
                    }
                }
                //Risng up delegate , to call static function from CRC class
                ushort TempCrc = CrcInputCalc(temp.Take(9), 2);   // Delegate won  

                temp[9] = (byte)(TempCrc & 0xFF);
                temp[10] = (byte)((TempCrc >> 8) & 0xFF);
            }

            #endregion building

            StringBuilder hex = new StringBuilder(temp.Length * 2);
            foreach(byte b in temp)
                hex.AppendFormat("{0:X2} ", b);

            string operation = "Tx: 0x";
            operation += hex.ToString();
            DebugTx = operation;
        }

        public void RxBuildOperation(byte[] data)
        {
            StringBuilder hex = new StringBuilder(data.Length * 2);
            foreach(byte b in data)
                hex.AppendFormat("{0:X2} ", b);

            string operation = "Rx: 0x8B 3C ";
            operation += hex.ToString();
            DebugRx = operation;
        }

        #endregion

        private string _debugTx = "";
        private string _debugRx = "";
        public string DebugTx
        {
            get { return _debugTx; }
            set { _debugTx = value; OnPropertyChanged("DebugTx"); }
        }
        public string DebugRx
        {
            get { return _debugRx; }
            set { _debugRx = value; OnPropertyChanged("DebugRx"); }
        }

        private UpDownControlModel _udID;
        public UpDownControlModel udID
        {
            get { return _udID; }
            set
            {
                if(value == _udID)
                    return;
                _udID = value;
                OnPropertyChanged("udID");
            }
        }
        private UpDownControlModel _udIndex;
        public UpDownControlModel udIndex
        {
            get { return _udIndex; }
            set
            {
                if(value == _udIndex)
                    return;
                _udIndex = value;
                OnPropertyChanged("udIndex");
            }
        }

        #region Simulation
        void initSim()
        {
            _udID = new UpDownControlModel();
            _udIndex = new UpDownControlModel();
            _udIDSim = new UpDownControlModel();
            _udIndexSim = new UpDownControlModel();
        }
        private bool _simulation = false;

        public bool Simulation
        {
            get
            {
                return _simulation;
            }
            set
            {
                if(!value)
                {
                    SimulationTicks(LeftPanelViewModel.STOP);
                    _simulation = false;
                }
                else if(value && LeftPanelViewModel._app_running)
                {
                    if(udIDSim.Data != null && udIndexSim.Data != null)
                    {
                        //if(Convert.ToDouble(SimData) >= 0 && Convert.ToUInt32(SimCount) >= 0)
                        {
                            //if(Convert.ToUInt32(SimStep) > 0)
                            {
                                SimulationTicks(LeftPanelViewModel.START);
                                _simulation = true;
                            }
                            //else
                                //_simulation = false;
                        }
                        //else
                            //_simulation = false;
                    }
                    else
                        _simulation = false;
                }
                OnPropertyChanged("Simulation");
            }
        }
        
        private UpDownControlModel _udIDSim;
        public UpDownControlModel udIDSim
        {
            get { return _udIDSim; }
            set
            {
                if(value == _udIDSim)
                    return;
                _udIDSim = value;
                OnPropertyChanged("udIDSim");
            }
        }
        private UpDownControlModel _udIndexSim;
        public UpDownControlModel udIndexSim
        {
            get { return _udIndexSim; }
            set
            {
                if(value == _udIndexSim)
                    return;
                _udIndexSim = value;
                OnPropertyChanged("udIndexSim");
            }
        }
        private bool _simIntFloat = false;
        public bool SimIntFloat
        {
            get
            {
                return _simIntFloat;
            }
            set
            {
                _simIntFloat = value;
                OnPropertyChanged("SimIntFloat");
            }
        }
        private string _simData = "0";
        public string SimData
        {
            get { return _simData; }
            set
            {
                if(value == _simData)
                    return;
                _simData = value;
                OnPropertyChanged("SimData");
            }
        }
        private string _simCount = "100";
        public string SimCount
        {
            get { return _simCount; }
            set
            {
                if(value == _simCount)
                    return;
                _simCount = value;
                OnPropertyChanged("SimCount");
            }
        }
        private bool _simCountEn = true;
        public bool SimCountEn
        {
            get
            {
                return _simCountEn;
            }
            set
            {
                _simCountEn = value;
                OnPropertyChanged("SimCountEn");
            }
        }
        private string _simStep = "1";
        public string SimStep
        {
            get { return _simStep; }
            set
            {
                if(value == _simStep)
                    return;
                _simStep = value;
                OnPropertyChanged("SimStep");
            }
        }
        private string _simDeltaT = "1";
        public string SimDeltaT
        {
            get { return _simDeltaT; }
            set
            {
                if(value == _simDeltaT)
                    return;
                _simDeltaT = value;
                OnPropertyChanged("SimDeltaT");
            }
        }
        private string _iterator = "1";
        public string iterator
        {
            get { return _iterator; }
            set
            {
                if(value == _iterator)
                    return;
                _iterator = value;
                OnPropertyChanged("iterator");
            }
        }

        private Timer _SimulationTimer;
        //const double _SimulationInterval = 500;
        public void SimulationTicks(int _mode)
        {
            switch(_mode)
            {
                case LeftPanelViewModel.STOP:
                    lock(this)
                    {
                        if(_SimulationTimer != null)
                        {
                            lock(_SimulationTimer)
                            {
                                _SimulationTimer.Stop();
                                _SimulationTimer.Elapsed -= SimulationFunc;
                                _SimulationTimer = null;
                                Thread.Sleep(10);
                            }
                            SimCountEn = true;
                        }
                    }
                    break;
                case LeftPanelViewModel.START:
                    if(_SimulationTimer == null)
                    {
                        Task.Factory.StartNew(action: () =>
                        {
                            Thread.Sleep(100);
                            SimCountEn = false;
                            simDataTemp = Convert.ToUInt32(SimData);
                            i = 0;
                            _SimulationTimer = new Timer(Convert.ToUInt32(SimDeltaT)) { AutoReset = true };
                            _SimulationTimer.Elapsed += SimulationFunc;
                            _SimulationTimer.Start();
                        });
                    }
                    break;
            }
        }
        UInt32 simDataTemp = 0;
        int i = 0;
        public void SimulationFunc(object sender, EventArgs e)
        {
            if(Rs232Interface._comPort.IsOpen)
            {
                if(i < Convert.ToUInt32(SimCount))
                {
                    Rs232Interface.GetInstance.SendToParser(new PacketFields
                    {
                        Data2Send = simDataTemp.ToString(),
                        ID = Convert.ToInt16(udIDSim.Data),
                        SubID = Convert.ToInt16(udIndexSim.Data),
                        IsSet = true,
                        IsFloat = SimIntFloat
                    });
                    simDataTemp += Convert.ToUInt32(SimStep);
                    i++;
                    iterator = (Convert.ToUInt32(SimCount) - i).ToString();

                    //if(i == Convert.ToUInt32(SimCount))
                    //    Simulation = false;
                }
                else
                    Simulation = false;
            }
        }
        #endregion Simulation

        #region ForceConnect
        public bool _forceConnectMode = false;
        public ObservableCollection<string> FlashBaudrateList
        {

            get
            {
                return MaintenanceViewModel.GetInstance.FlashBaudrateList;
            }
        }
        private string _flashBaudRate = "230400";
        public string FlashBaudRate
        {
            get
            {
                return _flashBaudRate;
            }
            set
            {
                _flashBaudRate = value;
                OnPropertyChanged();
            }
        }
        public ActionCommand ForceConnect { get { return new ActionCommand(ForceConnectCmd); } }
        private void ForceConnectCmd()
        {
            PortChat.GetInstance.Main(LeftPanelViewModel.GetInstance.ComboBoxCOM.ComString, Convert.ToInt32(FlashBaudRate));
            PortChat.GetInstance._serialPort.DataReceived -= Rs232Interface.GetInstance.DataReceived;
            PortChat.GetInstance._serialPort.DataReceived += Rs232Interface.GetInstance.DataReceived;

            ParserRayonM1.GetInstanceofParser.Parser2Send -= Rs232Interface.GetInstance.SendDataHendler;
            ParserRayonM1.GetInstanceofParser.Parser2Send += Rs232Interface.GetInstance.SendDataHendler;
            Rs232Interface._comPort = PortChat.GetInstance._serialPort;

            ForceConnectEnable = false;
            EnPing = false;
            EnRefresh = false;
            DebugRefresh = false;
            _forceConnectMode = true;
        }
        public ActionCommand Disconnect { get { return new ActionCommand(DisconnectCmd); } }
        private void DisconnectCmd()
        {
            PortChat.GetInstance.CloseComunication();
            ForceConnectEnable = true;
            _forceConnectMode = false;
        }
        private bool _forceConnectEnable = true;
        public bool ForceConnectEnable
        {
            get
            {
                return _forceConnectEnable;
            }
            set
            {
                _forceConnectEnable = value;
                OnPropertyChanged();
            }
        }

        #endregion ForceConnect
    }
}






