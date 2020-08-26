#define RELEASE_MODE

using System;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Abt.Controls.SciChart;
using MotorController.CommandsDB;
using MotorController.Models.DriverBlock;
using MotorController.Views;
using MotorController.Helpers;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Timer = System.Timers.Timer;
using System.Collections.Generic;
using System.Windows.Threading;

namespace MotorController.ViewModels
{

    public partial class LeftPanelViewModel : ViewModelBase
    {
        #region members
        public PacketFields RxPacket;
        private static readonly object TableLock = new object();
        private readonly object ConnectLock = new object();
        private readonly object pidLock = new object();
        private ComboBox _comboBox;

        private Thread _xlThread = new Thread(ThreadProc);
        #endregion
        #region Actions
        public ActionCommand SetAutoConnectActionCommandCommand
        {
            get { return new ActionCommand(AutoConnectCommand); }
        }
        public ActionCommand ForceStop { get { return new ActionCommand(FStop); } }
        public ActionCommand Showwindow { get { return new ActionCommand(ShowParametersWindow); } }
        public ActionCommand ShowWizard { get { return new ActionCommand(ShowWizardWindow); } }
        public ActionCommand ClearLogCommand
        {
            get { return new ActionCommand(ClearLog); }
        }
        private static LeftPanelViewModel _instance;
        private static readonly object Synlock = new object(); //Single tone variabl
                                                               //   public ActionCommand GetPidCurr { get { return new ActionCommand(GPC); } }
                                                               //   public ActionCommand SetPidCurr { get { return new ActionCommand(SPC); } }
        #endregion

        #region Props
        public static LeftPanelViewModel GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new LeftPanelViewModel();
                    return _instance;
                }
            }
        }
        public bool ValueChange = false;
        //WizardWindowViewModel _wizardWindow = WizardWindowViewModel.GetInstance;
        public LeftPanelViewModel()
        {
            ComboBoxCOM = ComboBox.GetInstance;
        }
        public ComboBox ComboBoxCOM
        {
            get { return _comboBox; }
            set { _comboBox = value; }
        }
        #region Connect_Button
        private String _connetButtonContent;
        public String ConnectButtonContent
        {
            get { return _connetButtonContent; }
            set
            {
                if(value == "Disconnect")
                {
                    ComboBox.GetInstance.ComPortComboboxEn = false;
                }
                else
                {
                    ComboBox.GetInstance.ComPortComboboxEn = true;
                    LeftPanelViewModel._app_running = false;
                    ConnectTextBoxContent = "Not Connected";
                }
                if(_connetButtonContent == value)
                    return;
                _connetButtonContent = value;
                OnPropertyChanged("ConnectButtonContent");

            }

        }
        private bool _connectButtonEnable = true;
        public bool ConnectButtonEnable
        {
            get { return _connectButtonEnable; }
            set
            {
                if(_connectButtonEnable == value)
                    return;
                _connectButtonEnable = value;
                OnPropertyChanged("ConnectButtonEnable");
            }
        }
        public bool StarterOperationFlag = false;
        public bool StarterPlotFlag = false;
        public int StarterCount = 0;
        public bool plotList()
        {
            bool plotResult = false;
            StarterPlotFlag = true;
            OscilloscopeViewModel.GetInstance.ChComboEn = false;
            Thread.Sleep(10);

            Rs232Interface.GetInstance.SendToParser(new PacketFields
            {
                Data2Send = "",
                ID = Convert.ToInt16(34),
                SubID = Convert.ToInt16(1), // Start Plot list
                IsSet = false,
                IsFloat = false
            });

            int timeOutPlot = 0;
            do
            {
                Thread.Sleep(100);
                timeOutPlot++;
            } while(OscilloscopeParameters.plotCount_temp != 0 && timeOutPlot <= 50);

            Debug.WriteLine("TimeOutPlot: " + timeOutPlot);
            if(OscilloscopeParameters.plotCount_temp == 0)
            {
                EventRiser.Instance.RiseEevent(string.Format($"Success"));
                plotResult = true;
            }
            else
            {
                EventRiser.Instance.RiseEevent(string.Format($"Failed"));
                OscilloscopeParameters.InitList();
                plotResult = false;
            }

            OscilloscopeViewModel.GetInstance.ChComboEn = true;
            //StarterPlotFlag = false;
            return plotResult;

        }
        private void StarterOperationTicks(object sender, EventArgs e)
        {
            #region Operations
            StarterOperationFlag = true;
            StarterCount = 0;
            RefreshManger.ConnectionCount = 0;

            if(!plotList())
                plotList();

            short[] ID = { 60, 60,/* 62,*/ 62, 62, 62, 1 };
            short[] subID = { 1, 2, /*10,*/ 1, 2, 3, 0 };
            string[] param = { "Read Ch1", "Read Ch2", /*"Read Checksum",*/ "Read SN", "Read HW Rev", "Read FW Rev", "Read motor status" };

            EventRiser.Instance.RiseEevent(string.Format($"Reading param..."));

            for(int i = 0; i < param.Length; i++)
            {
                //EventRiser.Instance.RiseEevent(string.Format(param[i]));
                Rs232Interface.GetInstance.SendToParser(new PacketFields
                {
                    Data2Send = "",
                    ID = ID[i],
                    SubID = subID[i],
                    IsSet = false,
                    IsFloat = false
                });
                Thread.Sleep(50);
            }
            
            #endregion  Operations

            int timeOutReadParam = 0;
            do
            {
                Thread.Sleep(100);
                timeOutReadParam++;
            } while(StarterOperationFlag && timeOutReadParam <= 20);
            int _freqCount = 0;
            if(!String.IsNullOrEmpty(Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(62, 3)].CommandValue))
                if(Convert.ToInt32(Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(62, 3)].CommandValue) > 20257)
                {
                    StarterOperationFlag = true;
                    // Get Frequency 
                    Rs232Interface.GetInstance.SendToParser(new PacketFields 
                    {
                        Data2Send = "",
                        ID = 34,
                        SubID = 2,
                        IsSet = false,
                        IsFloat = false
                    });
                    EventRiser.Instance.RiseEevent(string.Format($"Read Plot Freq..."));
                    _freqCount = 1;
                }
            Thread.Sleep(100);
            if(StarterCount == param.Length + _freqCount)
                EventRiser.Instance.RiseEevent(string.Format($"Connected successfully with unit"));
            else
            {
                EventRiser.Instance.RiseEevent(string.Format($"Failed reading params"));
                EventRiser.Instance.RiseEevent(string.Format($"Reconnect again..."));

#if ReadAgaingParam
                StarterCount = 0;
                for(int i = 0; i < param.Length; i++)
                {
                    //EventRiser.Instance.RiseEevent(string.Format(param[i]));
                    Rs232Interface.GetInstance.SendToParser(new PacketFields
                    {
                        Data2Send = "",
                        ID = ID[i],
                        SubID = subID[i],
                        IsSet = false,
                        IsFloat = false
                    });
                    Thread.Sleep(50);
                }
                Thread.Sleep(50);
                if(StarterCount == param.Length)
                    EventRiser.Instance.RiseEevent(string.Format($"Connected successfully with unit"));
                else
                    EventRiser.Instance.RiseEevent(string.Format($"Failed reading params"));
#endif
            }

#if !DEBUG || RELEASE_MODE
            VerifyConnectionTicks(STOP);
            VerifyConnectionTicks(START);
#endif
            if(RefreshManger.GetInstance.DisconnectedFlag)
            {
                //Thread.Sleep(10);
                //Rs232Interface.GetInstance.SendToParser(new PacketFields
                //{
                //    Data2Send = RefreshManger.GetInstance.ch1.ToString(),
                //    ID = Convert.ToInt16(60),
                //    SubID = Convert.ToInt16(1),
                //    IsSet = true,
                //    IsFloat = false
                //});
                //Thread.Sleep(10);

                //Rs232Interface.GetInstance.SendToParser(new PacketFields
                //{
                //    Data2Send = RefreshManger.GetInstance.ch2.ToString(),
                //    ID = Convert.ToInt16(60),
                //    SubID = Convert.ToInt16(2),
                //    IsSet = true,
                //    IsFloat = false
                //});
                //Thread.Sleep(10);
                //Rs232Interface.GetInstance.SendToParser(new PacketFields
                //{
                //    Data2Send = "1",
                //    ID = Convert.ToInt16(64),
                //    SubID = Convert.ToInt16(0),
                //    IsSet = true,
                //    IsFloat = false
                //});
            }
            Thread.Sleep(10);
            RefreshManger.GetInstance.DisconnectedFlag = false;

            BlinkLedsTicks(STOP);
            BlinkLedsTicks(START);

            RefreshParamsTick(STOP);
            if(DebugViewModel.GetInstance.EnRefresh)
                RefreshParamsTick(START);

            LeftPanelViewModel._app_running = true;
            StarterOperation(STOP);
            LeftPanelViewModel.GetInstance.ConnectButtonEnable = true;

            //OscilloscopeViewModel.GetInstance.ResetZoom();
        }
        private String _connectTextBoxContent;
        public String ConnectTextBoxContent
        {
            get { return _connectTextBoxContent; }
            set
            {
                if(_connectTextBoxContent == value)
                    return;
                _connectTextBoxContent = value;
                OnPropertyChanged("ConnectTextBoxContent");
            }
        }

        private ObservableCollection<object> _lpCommandsList;
        public ObservableCollection<object> LPCommandsList
        {
            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["LPCommands List"];
            }
            set
            {
                _lpCommandsList = value;
                OnPropertyChanged();
            }
        }
#endregion

        private float _setCurrentPid;
        public float SetCurrentPid
        {
            get { return _setCurrentPid; }
            set
            {
                if(_setCurrentPid == value)
                    return;
                _setCurrentPid = value;
                OnPropertyChanged("SetCurrentPid");


            }
        }
        private float _currentPid;
        public float CurrentPid
        {
            get { return _currentPid; }
            set
            {
                if(_currentPid == value)
                    return;

                _currentPid = value;
                OnPropertyChanged("CurrentPid");
            }
        }

#region Send_Button
        private String _sendButtonContent;
        public String SendButtonContent
        {

            get { return _sendButtonContent; }
            set
            {
                if(_sendButtonContent == value)
                    return;
                _sendButtonContent = value;
                OnPropertyChanged("SendButtonContent");
            }
        }
#endregion

#region Stop_Button
        private String _stopButtonContent;
        public String StopButtonContent
        {
            get { return _stopButtonContent; }
            set
            {
                if(_stopButtonContent == value)
                    return;
                _stopButtonContent = value;
                OnPropertyChanged("StopButtonContent");
            }
        }
#endregion

#region MotorON_Switch
        public static bool MotorOnOff_flag = false;
        private bool _motorOnToggleChecked = false;
        private int _ledMotorStatus;

        public bool MotorOnToggleChecked
        {
            get
            {
                return _motorOnToggleChecked;
            }
            set
            {
                if(_connetButtonContent == "Disconnect")
                {
                    _motorOnToggleChecked = value;
                    //Sent
                    if(!MotorOnOff_flag)
                    {
                        Rs232Interface.GetInstance.SendToParser(new PacketFields
                        {
                            Data2Send = _motorOnToggleChecked ? 1 : 0,
                            ID = Convert.ToInt16(1),
                            SubID = Convert.ToInt16(0),
                            IsSet = true,
                            IsFloat = false
                        });
                        Rs232Interface.GetInstance.SendToParser(new PacketFields
                        {
                            Data2Send = "",
                            ID = Convert.ToInt16(1),
                            SubID = Convert.ToInt16(0),
                            IsSet = false,
                            IsFloat = false
                        });
                    }
                    MotorOnOff_flag = false;
                    OnPropertyChanged("MotorONToggleChecked");
                }
            }
        }
        public int LedMotorStatus
        {
            get
            {
                return _ledMotorStatus;
            }
            set
            {
                if(value == 0)
                {
                    MotorOnOff_flag = true;
                    MotorOnToggleChecked = false;
                }
                else if(value == 1)
                {
                    MotorOnOff_flag = true;
                    MotorOnToggleChecked = true;
                }
                _ledMotorStatus = value;
                RaisePropertyChanged("LedMotorStatus");
            }
        }

        private ObservableCollection<object> _driverStatusList;
        public ObservableCollection<object> DriverStatusList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["DriverStatus List"];
            }
            set
            {
                _driverStatusList = value;
                OnPropertyChanged();
            }


        }

#endregion

#endregion

#region TXRXLed
        private int _led_statusTx;
        public int LedStatusTx
        {
            get
            {
                return _led_statusTx;
            }
            set
            {
                _led_statusTx = value;
                RaisePropertyChanged("LedStatusTx");
            }
        }

        private int _led_statusRx;

        public int LedStatusRx
        {
            get
            {
                return _led_statusRx;
            }
            set
            {
                _led_statusRx = value;
                RaisePropertyChanged("LedStatusRx");
            }
        }
#endregion TXRXLed

#region Send_Button

        public ActionCommand SendActionCommand { get { return new ActionCommand(() => SendXLS()); } }


        public void SendXLS()
        {
            if(_xlThread.ThreadState == System.Threading.ThreadState.Running)
            {
                System.Windows.MessageBox.Show(" Wait until the end of XLS!! thread running)))");
                return;
            }

            if(_xlThread.ThreadState == System.Threading.ThreadState.WaitSleepJoin)
            {
                System.Windows.MessageBox.Show(" Wait until the end of XLS!! thread Sleeping)))");
                return;
            }

            if(_xlThread.ThreadState == System.Threading.ThreadState.Unstarted)
            {

                _xlThread.Start();
            }

            if(_xlThread.ThreadState == System.Threading.ThreadState.Stopped)
            {
                _xlThread = new Thread(ThreadProc);
                _xlThread.Start();
            }

        }

        public static bool Stop = false;
        public static ManualResetEvent mre = new ManualResetEvent(false);
        public static string Exelsrootpath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())) + @"\Exels";
        public static string Recordsrootpath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())) + @"\Records\";
        static public string excelPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())) + @"\Exels\const_acc_N.xlsx";
        public static string name;
        private static DataSet output = new System.Data.DataSet();
        static private double mmperRev = 2.5;
        static private double countesPerRev = 1200;
        static private double offset = 0;
        public static Int32 ChankLen = 0;
        private static UInt32 DebugCount = 0;

        private static void ThreadProc()
        {
            byte[] poRefCmd;
            DataTable table;


            poRefCmd = new byte[11];

            using(OleDbConnection connection = new OleDbConnection())
            {
                connection.ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + excelPath +
                                                ";Extended Properties=\"Excel 12.0 Xml;HDR=YES\"";
                /* The connection string is used to specify how the connection would be performed. */
                connection.Open();
                OleDbCommand command = new OleDbCommand
                    ("SELECT POS " + "FROM [Feuil1$]", connection);
                OleDbDataAdapter adapter = new OleDbDataAdapter(command);

                // Stopwatch sw = new Stopwatch();
                adapter.Fill(output);
                table = output.Tables[0];
            }


            // ProtocolParser.GetInstance.BuildPacketToSend("1", "213" /*CommandId*/, "0" /* subid*/, true /*IsSet*/);
            if(Rs232Interface.GetInstance.IsSynced)
            {
                //Send command to the target 
                PacketFields RxPacket;
                RxPacket.ID = 213;
                RxPacket.IsFloat = false;
                RxPacket.IsSet = true;
                RxPacket.SubID = 0;
                RxPacket.Data2Send = 1;
                //rise event
                Rs232Interface.GetInstance.SendToParser(RxPacket);

            }

            mre.WaitOne();

            Stopwatch sw = new Stopwatch();

            foreach(DataRow row in table.Rows)
            {
                double temp = (double)row[0];
                object item = (double)((((double)row[0] * countesPerRev) / mmperRev) + offset);
                float var = (float)Convert.ToSingle(item);
                int var_i = (int)Convert.ToInt32(item);
                string datatosend = var_i.ToString();

                if(ChankLen > 0)
                {
                    ChankLen--;
                    //  ProtocolParser.GetInstance.BuildPacketToSend(datatosend, "403" /*CommandId*/, "0" /* subid*/, true /*IsSet*/);
                    if(Rs232Interface.GetInstance.IsSynced)
                    {
                        //Send command to the target 
                        PacketFields RxPacket;
                        RxPacket.ID = 403;
                        RxPacket.IsFloat = false;
                        RxPacket.IsSet = false;
                        RxPacket.SubID = 0;
                        RxPacket.Data2Send = var_i;
                        //rise event
                        Rs232Interface.GetInstance.SendToParser(RxPacket);
                    }
                    DebugCount++;
#region SW
                    sw.Start();
                    while(sw.ElapsedTicks < 50)
                    {
                    }
                    sw.Reset();
#endregion
                }
                else
                {
                    sw.Start();
                    while(sw.ElapsedTicks < 50)
                    {
                    }
                    sw.Reset();
                    ChankLen = 0;
                    //   ProtocolParser.GetInstance.BuildPacketToSend(datatosend, "403" /*CommandId*/, "0" /* subid*/, true /*IsSet*/);
                    if(Rs232Interface.GetInstance.IsSynced)
                    {
                        //Send command to the target 
                        PacketFields RxPacket;
                        RxPacket.ID = 403;
                        RxPacket.IsFloat = false;
                        RxPacket.IsSet = true;
                        RxPacket.SubID = 0;
                        RxPacket.Data2Send = var_i;
                        //rise event
                        Rs232Interface.GetInstance.SendToParser(RxPacket);
                    }
                    DebugCount++;
                    mre.Reset();//Suspend
                    mre.WaitOne();
                }
                lock(TableLock)
                {
                    if(Stop == true)
                    {
                        break;
                    }
                }


            }

            sw.Start();
            while(sw.ElapsedTicks < 10000)
            {
            }
            sw.Reset();
            // DebugCount = 0;
            ChankLen = 0;
            table.Dispose();
        }


#endregion

#region Action methods
        public static bool busy = false;
        public void AutoConnectCommand()
        {
            ConnectButtonEnable = false;
            if(Rs232Interface.GetInstance.IsSynced == false && busy == false)
            {
                busy = true;
                lock(ConnectLock)
                {
                    // Erase textboxs content, reset all default.
                    foreach(var element in Commands.GetInstance.DataViewCommandsList)
                    {
                        element.Value.CommandValue = "";
                    }
                    foreach(var element in Commands.GetInstance.CalibartionCommandsList)
                    {
                        element.Value.ButtonContent = "Run";
                        element.Value.CommandValue = "";
                    }
                    LogText = "";
                    Task task = new Task(Rs232Interface.GetInstance.AutoConnect);
                    task.Start();
                }
            }
            else if(busy == false)
            {
                busy = true;
                lock(ConnectLock)
                {
                    if(Rs232Interface._comPort != null)
                    {
                        Debug.WriteLine("IsOpen When Disc: " + Rs232Interface._comPort.IsOpen.ToString());
                    }
                    else
                        Debug.WriteLine("IsOpen When Disc: null");
                    Rs232Interface.GetInstance.Disconnect();
                }
            }
        }

        private string _logText;

        //int logCounter = 0;
        public void Instance_LoggerEvent(object sender, EventArgs e)
        {
            string temp = ((CustomEventArgs)e).Msg + Environment.NewLine + LogText;
            if(!LogText.Contains(((CustomEventArgs)e).Msg))
                LogText = temp;
        }
        public string LogText
        {
            get { return _logText; }

            set
            {
                _logText = value;
                RaisePropertyChanged("LogText");
            }
        }

        private string _comToolTipText;
        public string ComToolTipText
        {
            get { return _comToolTipText; }

            set
            {
                _comToolTipText = value;
                RaisePropertyChanged("ComToolTipText");
            }
        }
        public void FStop()
        {
            lock(TableLock)
            {
                Stop = true;
            }
            Thread.Sleep(1000);

            if(_xlThread.ThreadState == System.Threading.ThreadState.WaitSleepJoin)
            {
                _xlThread.Abort();
            }
        }
        public static bool _app_running = false; // Indicate the application is running and connected to a driver

        public static ParametarsWindow win;
        private void ShowParametersWindow()
        {
            if(ParametarsWindow.WindowsOpen != true)
            {
                win = ParametarsWindow.GetInstance;
                if(win.ActualHeight != 0)
                {
                    win.Activate();
                }
                else
                {
                    win.Show();
                }
            }
            else if(win.WindowState == System.Windows.WindowState.Minimized)
                win.WindowState = System.Windows.WindowState.Normal;
            win.Activate();
            win.Topmost = true;  // important
            win.Topmost = false; // important
            win.Focus();         // important
        }

        public static Wizard WizardWindow;
        private void ShowWizardWindow()
        {
            if(Wizard.WindowsOpen != true)
            {
                WizardWindow = Wizard.GetInstance;
                if(WizardWindow.ActualHeight != 0)
                    WizardWindow.Activate();
                else
                {
                    //WizardWindow.Show();
                    //Thread thread = new Thread((ThreadStart)(() =>
                    //{
                    //    WizardWindow.Show();

                    //    System.Windows.Threading.Dispatcher.Run();
                    //}));

                    //thread.SetApartmentState(ApartmentState.STA);
                    //thread.Start();

                    WizardWindow.Dispatcher.Invoke(DispatcherPriority.Normal,
                        new Action(delegate ()
                        {
                            WizardWindow.Show();
                        }
                        ));
                }
            }
            else if(WizardWindow.WindowState == System.Windows.WindowState.Minimized)
                WizardWindow.WindowState = System.Windows.WindowState.Normal;
            WizardWindow.Activate();
            WizardWindow.Topmost = true;  // important
            WizardWindow.Topmost = false; // important
            WizardWindow.Focus();         // important
        }
        public void Close_parmeterWindow()
        {
            win.Close();
        }

        private void ClearLog()
        {
            LogText = "";
        }

        private Timer _RefreshParamsTickTimer;
        const double _RefreshParamsTickInterval = 5;
        public const int START = 1;
        public const int STOP = 0;
        public const int TX_LED = 1;
        public const int RX_LED = 0;
        public void RefreshParamsTick(int _mode)
        {
            switch(_mode)
            {
                case STOP:
                    lock(this)
                    {
                        if(_RefreshParamsTickTimer != null)
                        {
                            lock(_RefreshParamsTickTimer)
                            {
                                _RefreshParamsTickTimer.Stop();
                                _RefreshParamsTickTimer.Elapsed -= RefreshParams;
                                _RefreshParamsTickTimer = null;
                                Thread.Sleep(10);
                            }
                        }
                    }
                    break;
                case START:
                    if(_RefreshParamsTickTimer == null)
                    {
                        Task.Factory.StartNew(action: () =>
                        {
                            Thread.Sleep(100);
                            _RefreshParamsTickTimer = new Timer(_RefreshParamsTickInterval) { AutoReset = true };
                            _RefreshParamsTickTimer.Elapsed += RefreshParams;
                            _RefreshParamsTickTimer.Start();
                        });
                    }
                    break;
            }
        }
        public void RefreshParams(object sender, EventArgs e)
        {
            if((_app_running && DebugViewModel.GetInstance.EnRefresh) || (_app_running && DebugViewModel.GetInstance.DebugRefresh))
            {
                RefreshManger.GetInstance.StartRefresh();
            }
        }

        private Timer _VerifyConnectionTimer;
        const double _VerifyConnectionInterval = 500;
        public void VerifyConnectionTicks(int _mode)
        {
            switch(_mode)
            {
                case STOP:
                    lock(this)
                    {
                        if(_VerifyConnectionTimer != null)
                        {
                            lock(_VerifyConnectionTimer)
                            {
                                _VerifyConnectionTimer.Stop();
                                _VerifyConnectionTimer.Elapsed -= RefreshManger.GetInstance.VerifyConnection;
                                _VerifyConnectionTimer = null;
                                Thread.Sleep(10);
                            }
                        }
                    }
                    break;
                case START:
                    if(_VerifyConnectionTimer == null)
                    {
                        Task.Factory.StartNew(action: () =>
                        {
                            Thread.Sleep(100);
                            _VerifyConnectionTimer = new Timer(_VerifyConnectionInterval) { AutoReset = true };
                            _VerifyConnectionTimer.Elapsed += RefreshManger.GetInstance.VerifyConnection;
                            _VerifyConnectionTimer.Start();
                        });
                    }
                    break;
            }
        }

        private Timer _BlinkLedsTimer;
        const double _BlinkLedsInterval = 1;
        public void BlinkLedsTicks(int _mode)
        {
            switch(_mode)
            {
                case STOP:
                    lock(this)
                    {
                        if(_BlinkLedsTimer != null)
                        {
                            lock(_BlinkLedsTimer)
                            {
                                _BlinkLedsTimer.Stop();
                                _BlinkLedsTimer.Elapsed -= BlinkLeds;
                                _BlinkLedsTimer = null;
                                Thread.Sleep(10);
                            }
                        }
                    }
                    break;
                case START:
                    if(_BlinkLedsTimer == null)
                    {
                        Task.Factory.StartNew(action: () =>
                        {
                            Thread.Sleep(10);
                            _BlinkLedsTimer = new Timer(_BlinkLedsInterval) { AutoReset = true };
                            _BlinkLedsTimer.Elapsed += BlinkLeds;
                            _BlinkLedsTimer.Start();
                        });
                    }
                    break;
            }
        }
        public int led = -1;
        public void BlinkLeds(object sender, EventArgs e)
        {
            if(_app_running)
            {
                lock(Synlock)
                {
                    if(led == TX_LED)
                    {
                        LedStatusTx = 1;
                        Thread.Sleep(3);
                    }
                    if(led == RX_LED)
                    {
                        LedStatusRx = 1;
                        Thread.Sleep(1);
                    }
                    LedStatusTx = 0;
                    LedStatusRx = 0;
                    led = -1;
                }
            }
        }

        private Timer _StarterOperationTimer = null;
        const double _StarterOperationInterval = 1;
        public void StarterOperation(int _mode)
        {
            switch(_mode)
            {
                case STOP:
                    lock(this)
                    {
                        if(_StarterOperationTimer != null)
                        {
                            lock(_StarterOperationTimer)
                            {
                                _StarterOperationTimer.Stop();
                                _StarterOperationTimer.Elapsed -= StarterOperationTicks;
                                _StarterOperationTimer = null;
                                Thread.Sleep(10);
                            }
                        }
                    }
                    break;
                case START:
                    if(_StarterOperationTimer == null)
                    {
                        Task.Factory.StartNew(action: () =>
                        {
                            Thread.Sleep(10);
                            _StarterOperationTimer = new Timer(_StarterOperationInterval) { AutoReset = false };
                            _StarterOperationTimer.Elapsed += StarterOperationTicks;
                            _StarterOperationTimer.Start();
                        });
                    }
                    break;
            }
        }

        private string _driverStat;

        public string DriverStat
        {
            get { return _driverStat; }
            set
            {
                _driverStat = value;
                OnPropertyChanged("DriverStat");
            }
        }
#endregion
    }
}
