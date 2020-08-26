using Abt.Controls.SciChart;
using Abt.Controls.SciChart.ChartModifiers;
using Abt.Controls.SciChart.Example.Common;
using Abt.Controls.SciChart.Example.Data;
using Abt.Controls.SciChart.Model.DataSeries;
using Abt.Controls.SciChart.Visuals.Axes;
using MotorController.CommandsDB;
using MotorController.Data;
using MotorController.Models.DriverBlock;
using MotorController.ViewModels;
using MotorController.Views;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using Timer = System.Timers.Timer;

namespace MotorController.ViewModels
{
    class BodeViewModel : ViewModelBase
    {
        private static readonly object Synlock = new object();
        public static readonly object PlotBodeListLock = new object();             //Singletone variable

        private static BodeViewModel _instance;
        public static BodeViewModel GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new BodeViewModel();
                    return _instance;
                }
            }
            set
            {
                _instance = value;
            }
        }
        private BodeViewModel()
        {
            ChartData = new XyDataSeries<float, float>();
            ChartData1 = new XyDataSeries<float, float>();
            InitializeAxes();
            //Load();
        }

        private bool _bodeStartStop = false;

        public bool BodeStartStop
        {
            get
            {
                return _bodeStartStop;
            }
            set
            {
                if(!LeftPanelViewModel._app_running)
                    return;
                //_bodeStartStop = value;
                // get call stack
                StackTrace stackTrace = new StackTrace();
                if(stackTrace.GetFrame(1).GetMethod().Name == "UpdateModel")
                {
                    _bodeStartStop = value;
                    OnPropertyChanged();
                }
                else if(stackTrace.GetFrame(1).GetMethod().Name != "UpdateModel")
                {
                    Rs232Interface.GetInstance.SendToParser(new PacketFields
                    {
                        Data2Send = value ? 1 : 0,
                        ID = Convert.ToInt16(6),
                        SubID = Convert.ToInt16(15),
                        IsSet = true,
                        IsFloat = false
                    });
                    if(value)
                    {
                        ChartData = new XyDataSeries<float, float>();
                        ChartData1 = new XyDataSeries<float, float>();
                        X_List.Clear();
                        Y1_List.Clear();
                        Y2_List.Clear();
                        //Task WaitSave = Task.Run((Action)GetInstance.Wait);
                        OnBodeStart();
                    }
                    else
                    {
                        //Task WaitSave = Task.Run((Action)GetInstance.Wait);
                        //OnBodeStop();
                    }
                }
            }
        }
        private void Wait()
        {
            Thread.Sleep(100);
            BodeStartStop = !_bodeStartStop;
        }
        private IAxis _xAxis1;
        public IAxis XAxis1
        {
            get { return _xAxis1; }
            set
            {
                _xAxis1 = value;
                OnPropertyChanged("XAxis1");
            }
        }
        private IAxis _xAxis2;
        public IAxis XAxis2
        {
            get { return _xAxis2; }
            set
            {
                _xAxis2 = value;
                OnPropertyChanged("XAxis2");
            }
        }
        private AxisCollection _chartYAxes = new AxisCollection();

        public AxisCollection ChartYAxes
        {
            get { return _chartYAxes; }
            set
            {
                _chartYAxes = value;
                OnPropertyChanged("ChartYAxes");
            }
        }

        private LogarithmicNumericAxis _xAxisLog;
        private static readonly object _lockAxes = new object();
        private DoubleRange _xAxisDoubleRange = new DoubleRange(0.6, 1000);
        // new DoubleRange(Convert.ToInt32(Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(15, 2)].CommandValue), Convert.ToInt32(Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(15, 3)].CommandValue));

        public DoubleRange XAxisDoubleRange
        {
            get { return _xAxisDoubleRange; }
            set
            {
                _xAxisDoubleRange = value;
                OnPropertyChanged("XAxisDoubleRange");
            }
        }
        private DoubleRange _magVisibleRange = new DoubleRange(-50, 20);
        public DoubleRange MagVisibleRange
        {
            get { return _magVisibleRange; }
            set
            {
                _magVisibleRange = value;
                OnPropertyChanged("MagVisibleRange");
            }
        }
        private DoubleRange _phaseVisibleRange = new DoubleRange(-200, 20);
        public DoubleRange PhaseVisibleRange
        {
            get { return _phaseVisibleRange; }
            set
            {
                _phaseVisibleRange = value;
                OnPropertyChanged("PhaseVisibleRange");
            }
        }
        [STAThread]
        private void InitializeAxes()
        {
            AutoRange _xAutorange = AutoRange.Never;
            //if(BodeStartStop)
            //    _xAutorange = AutoRange.Always;
            
            lock(_lockAxes)
            {
                _xAxisLog = new LogarithmicNumericAxis
                {
                    TextFormatting = "#0.00#",
                    ScientificNotation = ScientificNotation.Normalized,
                    VisibleRange = _xAxisDoubleRange,
                    GrowBy = new DoubleRange(0.1, 0.1),
                    DrawMajorBands = false,
                    AutoRange = _xAutorange,
                    //AxisTitle = "Time (ms)",
                    DrawMinorGridLines = true,
                    DrawMajorTicks = true,
                    DrawMinorTicks = true,
                    StrokeThickness = 1,
                    LogarithmicBase = 10.0,
                    FontSize = 5
                };
                XAxis1 = _xAxisLog;
                _xAxisLog = new LogarithmicNumericAxis
                {
                    TextFormatting = "#0.00#",
                    ScientificNotation = ScientificNotation.Normalized,
                    VisibleRange = _xAxisDoubleRange,
                    GrowBy = new DoubleRange(0.1, 0.1),
                    DrawMajorBands = false,
                    AutoRange = _xAutorange,
                    //AxisTitle = "Time (ms)",
                    DrawMinorGridLines = true,
                    DrawMajorTicks = true,
                    DrawMinorTicks = true,
                    StrokeThickness = 1,
                    LogarithmicBase = 10.0,
                    FontSize = 5
                };
                XAxis2 = _xAxisLog;
                MagVisibleRange = _magVisibleRange;
                PhaseVisibleRange = _phaseVisibleRange;
            }
            //Load();

            //_yAxisLog = new LogarithmicNumericAxis
            //{
            //    TextFormatting = "#.#E+0",
            //    ScientificNotation = ScientificNotation.LogarithmicBase,
            //    AxisAlignment = AxisAlignment.Left,
            //    GrowBy = new DoubleRange(0.1, 0.1),
            //    DrawMajorBands = false
            //};
#if Yaxes
                NumericAxis Y1 = new NumericAxis()
                {
                    
                    Id = "Y1_Magnitude",
                    TextFormatting = "0.00",
                    ScientificNotation = ScientificNotation.Normalized,
                    AxisAlignment = AxisAlignment.Left,
                    GrowBy = new DoubleRange(0.2, 0.2),
                    VisibleRange = new DoubleRange(-50, 20),
                    AnimatedVisibleRange = new DoubleRange(-50, 20),
                    AxisTitle = "Magnitude (dB)",
                    TickTextBrush = new SolidColorBrush(Colors.White),
                    DrawMajorBands = false,
                    DrawMinorGridLines = false,
                    DrawMajorTicks = false,
                    DrawMinorTicks = false,
                    StrokeThickness = 1

                };
                NumericAxis Y2 = new NumericAxis()
                {
                    Id = "Y2_Phase",
                    TextFormatting = "0.00",
                    ScientificNotation = ScientificNotation.Normalized,
                    AxisAlignment = AxisAlignment.Right,
                    GrowBy = new DoubleRange(0.2, 0.2),
                    VisibleRange = new DoubleRange(-200, 200),
                    AutoRange = AutoRange.Always,
                    AnimatedVisibleRange = new DoubleRange(-200, 200),
                    AxisTitle = "Phase (Degree)",
                    TickTextBrush = new SolidColorBrush(Colors.White),
                    DrawMajorBands = false,
                    DrawMinorGridLines = false,
                    DrawMajorTicks = false,
                    DrawMinorTicks = false,
                    StrokeThickness = 1
                };
                //ChartYAxes.Add(new NumericAxis() {
                    //Id = "DefaultAxisId",
                    //Visibility = Visibility.Hidden
                //});
                //ChartYAxes.Add(Y1);
                //ChartYAxes.Add(Y2);
#endif
        }
        private void Load()
        {
            // Create some DataSeries of type X=double, Y=double
            ChartData2 = new XyDataSeries<float, float>();

            var data1 = GetExponentialCurve(1.8, 25);

            ChartData2.Clear();
            // Append data to series.
            ChartData2.Append(data1.XData_f, data1.YData_f);
        }
        public DoubleSeries GetExponentialCurve(double power, int pointCount)
        {
            var doubleSeries = new DoubleSeries(pointCount);

            double x = 0.1;
            const double fudgeFactor = 1.5;
            for(int i = 0; i < pointCount; i++)
            {
                x *= fudgeFactor;
                double y = 1;// Math.Pow((double)i + 1, power);
                doubleSeries.Add(new XYPoint() { X_f = (float)x, Y_f = (float)y });
            }

            return doubleSeries;
        }
        private IXyDataSeries<float, float> _series0;
        private IXyDataSeries<float, float> _series1;
        private IXyDataSeries<float, float> _series2;

        public IXyDataSeries<float, float> ChartData
        {
            get { return _series0; }
            set
            {
                _series0 = value;
                OnPropertyChanged("ChartData");
            }
        }
        public IXyDataSeries<float, float> ChartData1
        {
            get { return _series1; }
            set
            {
                _series1 = value;
                OnPropertyChanged("ChartData1");
            }
        }
        public IXyDataSeries<float, float> ChartData2
        {
            get { return _series2; }
            set
            {
                _series2 = value;
                OnPropertyChanged("ChartData2");
            }
        }
        private string _ch1Gain = "1";
        public string Ch1Gain
        {
            get { return _ch1Gain; }
            set
            {
                if(value == _ch1Gain || value == "")
                    return;
                _ch1Gain = value;
                OnPropertyChanged("Ch1Gain");
            }
        }
        private string _ch2Gain = "1";
        public string Ch2Gain
        {
            get { return _ch2Gain; }
            set
            {
                if(value == _ch2Gain || value == "")
                    return;
                _ch2Gain = value;
                OnPropertyChanged("Ch2Gain");
            }
        }
        private ModifierType _chartModifier;
        public ModifierType ChartModifier
        {
            get { return _chartModifier; }
            set
            {
                _chartModifier = value;
                OnPropertyChanged("ChartModifier");
                OnPropertyChanged("IsRolloverSelected");
                OnPropertyChanged("IsCursorSelected");
            }
        }
        private DoubleRange _yVisibleRange;
        public DoubleRange YVisibleRange
        {
            get { return _yVisibleRange; }
            set
            {
                if(_yVisibleRange == value)
                    return;

                _yVisibleRange = value;
                OnPropertyChanged("YVisibleRange");
            }
        }

        private ObservableCollection<object> _dataBodeList;
        public ObservableCollection<object> DataBodeList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["DataBodeList"];
            }
            set
            {
                _dataBodeList = value;
                OnPropertyChanged();
            }

        }
        private ObservableCollection<object> _enumBodeList;
        public ObservableCollection<object> EnumBodeList
        {

            get
            {
                return Commands.GetInstance.EnumCommandsListbySubGroup["EnumBodeList"];
            }
            set
            {
                _enumBodeList = value;
                OnPropertyChanged();
            }

        }
        public ConcurrentQueue<float> FifoplotBodeListX = new ConcurrentQueue<float>();
        public ConcurrentQueue<float> FifoplotBodeListY1 = new ConcurrentQueue<float>();
        public ConcurrentQueue<float> FifoplotBodeListY2 = new ConcurrentQueue<float>();
        private List<float> X_List = new List<float>();
        private List<float> Y1_List = new List<float>();
        private List<float> Y2_List = new List<float>();
        private List<float> _excel_X_List = new List<float>();
        private List<float> _excel_Y1_List = new List<float>();
        private List<float> _excel_Y2_List = new List<float>();
        private float[] X_arr;
        private float[] Y1_arr;
        private float[] Y2_arr;
        private float iqFactor = (float)Math.Pow(2.0, 24);
        public void ParseBodePlot(List<byte[]> PlotList)
        {
            // In order to achive best performance using good old-fashioned for loop: twice faster! then "foreach (byte[] packet in PlotList)"
            //Debug.WriteLine("ParsePlot 1" + DateTime.Now.ToString("h:mm:ss.fff"));
            var element = 0;
            double newPropertyValuef = 0;

            for(int i = 0; i < PlotList.Count; i++)
            {
                lock(PlotBodeListLock)
                {
                    // X
                    //element = ((PlotList[i][2] << 24) | (PlotList[i][3] << 16) | (PlotList[i][4] << 8) | (PlotList[i][5]));
                    var dataAray = new byte[4];
                    for(int j = 3, k = 0; j >= 0; j--, k++)
                    {
                        dataAray[j] = PlotList[i][k + 2];
                    }
                    newPropertyValuef = System.BitConverter.ToSingle(dataAray, 0);
                    FifoplotBodeListX.Enqueue((float)newPropertyValuef);
                    Debug.WriteLine("X: " + (newPropertyValuef).ToString());
                    // Y1
                    element = ((PlotList[i][6] << 24) | (PlotList[i][7] << 16) | (PlotList[i][8] << 8) | (PlotList[i][9]));
                    dataAray = new byte[4];
                    for(int j = 3, k = 0; j >= 0; j--, k++)
                    {
                        dataAray[j] = PlotList[i][k + 6];
                    }
                    newPropertyValuef = 20 * Math.Log10(System.BitConverter.ToSingle(dataAray, 0));
                    newPropertyValuef = newPropertyValuef < -100 ? -100 : newPropertyValuef;
                    FifoplotBodeListY1.Enqueue((float)newPropertyValuef);
                    Debug.WriteLine("Y1: " + newPropertyValuef.ToString());

                    // Y2
                    element = ((PlotList[i][10] << 24) | (PlotList[i][11] << 16) | (PlotList[i][12] << 8) | (PlotList[i][13]));
                    FifoplotBodeListY2.Enqueue((element / iqFactor) * 360);
                    Debug.WriteLine("Y2: " + ((element / iqFactor) * 360).ToString());

                }
            }
            plotSubFunction();

            //PlotList.Clear();
            //Debug.WriteLine("ParsePlot 2" + DateTime.Now.ToString("h:mm:ss.fff"));
        }
        private Timer _timer;
        private const double TimerIntervalMs = 100;
        public static bool _chartRunning = false;
        public void OnBodeStop()
        {
            /*
            lock(this)
            {
                if(_timer != null)
                {
                    lock(_timer)
                    {
                        _timer.Stop();
                        _timer.Elapsed -= OnTick;
                        _chartRunning = false;
                        _timer = null;
                        Thread.Sleep(10);
                    }
                }
            }
            */
        }
        // Setup start condition when the example enters
        public void OnBodeStart()
        {
            _excel_X_List.Clear();
            _excel_Y1_List.Clear();
            _excel_Y2_List.Clear();
            ResetZoom();
            /*
            if(_timer == null)
            {
                Task.Factory.StartNew(action: () =>
                {
                    Thread.Sleep(100);
                    _timer = new Timer(TimerIntervalMs) { AutoReset = true };
                    _timer.Elapsed += OnTick;
                    _chartRunning = true;
                    _timer.Start();
                });
            }
            */
        }
        private void OnTick(object sender, EventArgs e)
        {
            //Rs232Interface.GetInstance.SendToParser(new PacketFields
            //{
            //    Data2Send = "0",
            //    ID = 6,
            //    SubID = 15,
            //    IsSet = false,
            //    IsFloat = false
            //});

            plotSubFunction();
        }
        private void plotSubFunction()
        {
            float item;
            while(FifoplotBodeListX.TryDequeue(out item))
            {
                X_List.Add(item);
                _excel_X_List.Add(item);
                FifoplotBodeListY1.TryDequeue(out item);
                Y1_List.Add(item);
                _excel_Y1_List.Add(item);
                FifoplotBodeListY2.TryDequeue(out item);
                Y2_List.Add(item);
                _excel_Y2_List.Add(item);
            }
            if(X_List.Count > 0)
            {
                X_arr = X_List.ToArray();
                Y1_arr = Y1_List.ToArray();
                Y2_arr = Y2_List.ToArray();

                using(this.ChartData.SuspendUpdates())
                {
                    using(this.ChartData1.SuspendUpdates())
                    {
                        ChartData.Append(X_arr, Y1_arr);
                        ChartData1.Append(X_arr, Y2_arr);
                    }
                }

                X_List.Clear();
                Y1_List.Clear();
                Y2_List.Clear();
            }
        }
        private bool _startBodeEnable = true;
        private bool _stopBodeEnable = false;
        private string filePath;
        string delimiter = ",";

        public bool StartBodeEnable
        {
            get { return _startBodeEnable; }
            set { _startBodeEnable = value; OnPropertyChanged(); }
        }
        public bool StopBodeEnable
        {
            get { return _stopBodeEnable; }
            set { _stopBodeEnable = value; OnPropertyChanged(); }
        }
        public ActionCommand ResetZoomBode
        {
            get { return new ActionCommand(ResetZoom); }
        }
        public void ResetZoom()
        {
            try
            {
                _magVisibleRange = new DoubleRange(-50, 20);
                _phaseVisibleRange = new DoubleRange(-200, 20);
                _xAxisDoubleRange = new DoubleRange(0.5, 2000);

                if(!String.IsNullOrEmpty(Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(15, 2)].CommandValue) &&
                    !String.IsNullOrEmpty(Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(15, 3)].CommandValue))
                    _xAxisDoubleRange = new DoubleRange(Convert.ToSingle(Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(15, 2)].CommandValue) - 
                        0.1 * Convert.ToSingle(Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(15, 2)].CommandValue), 
                        Convert.ToSingle(Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(15, 3)].CommandValue) + 
                        0.1 * Convert.ToSingle(Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(15, 3)].CommandValue));

                InitializeAxes();
            }
            catch { }
        }

        public ActionCommand AutoScaleBode
        {
            get { return new ActionCommand(AutoScale); }
        }

        public void AutoScale()
        {
            try
            {
                if(_excel_X_List.Count > 1 && _excel_Y2_List.Count > 1 && _excel_Y1_List.Count > 1)
                {
                    //if(!String.IsNullOrEmpty(Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(15, 2)].CommandValue) &&
                    //    !String.IsNullOrEmpty(Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(15, 3)].CommandValue))
                    //    _xAxisDoubleRange = new DoubleRange(Convert.ToInt32(Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(15, 2)].CommandValue), Convert.ToInt32(Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(15, 3)].CommandValue));

                    _xAxisDoubleRange = new DoubleRange(/*Math.Abs*/(_excel_X_List.Min() - 0.2 * (_excel_X_List.Min())), _excel_X_List.Max() + 0.2 * (_excel_X_List.Max()));
                    //_phaseVisibleRange = new DoubleRange(_excel_Y2_List.Min() - 0.2 * (_excel_Y2_List.Max() - _excel_Y2_List.Min()), _excel_Y2_List.Max() + 0.2 * (_excel_Y2_List.Max() - _excel_Y2_List.Min()));
                    if(_excel_Y2_List.Min() != _excel_Y2_List.Max())
                        _phaseVisibleRange = new DoubleRange(_excel_Y2_List.Min() - 0.2 * (_excel_Y2_List.Max() - _excel_Y2_List.Min()), _excel_Y2_List.Max() + 0.2 * (_excel_Y2_List.Max() - _excel_Y2_List.Min()));
                    else if(_excel_Y2_List.Min() == _excel_Y2_List.Max() && _excel_Y2_List.Max() == 0)
                    {
                        _phaseVisibleRange = new DoubleRange(-1.5, + 1.5);
                    }
                    else
                    {
                        if(_excel_Y2_List.Min() < 0 && _excel_Y2_List.Max() < 0)
                            _phaseVisibleRange = new DoubleRange(_excel_Y2_List.Min() + 0.2 * (_excel_Y2_List.Min()), _excel_Y2_List.Max() - 0.2 * (_excel_Y2_List.Max()));
                        else
                            _phaseVisibleRange = new DoubleRange(_excel_Y2_List.Min() - 0.2 * (_excel_Y2_List.Min()), _excel_Y2_List.Max() + 0.2 * (_excel_Y2_List.Max()));
                    }
                    if(_excel_Y1_List.Min() != _excel_Y1_List.Max())
                        _magVisibleRange = new DoubleRange(_excel_Y1_List.Min() - 0.2 * (_excel_Y1_List.Max() - _excel_Y1_List.Min()), _excel_Y1_List.Max() + 0.2 * (_excel_Y1_List.Max() - _excel_Y1_List.Min()));
                    else if(_excel_Y1_List.Min() == _excel_Y1_List.Max() && _excel_Y1_List.Max() == 0)
                    {
                        _magVisibleRange = new DoubleRange(-1.5, +1.5);
                    }
                    else
                    {
                        if(_excel_Y1_List.Min() < 0 && _excel_Y1_List.Max() < 0)
                            _magVisibleRange = new DoubleRange(_excel_Y1_List.Min() + 0.2 * (_excel_Y1_List.Min()), _excel_Y1_List.Max() - 0.2 * (_excel_Y1_List.Max()));
                        else
                            _magVisibleRange = new DoubleRange(_excel_Y1_List.Min() - 0.2 * (_excel_Y1_List.Min()), _excel_Y1_List.Max() + 0.2 * (_excel_Y1_List.Max()));
                    }
                    InitializeAxes();
                }
            }
            catch { }
        }
        public ActionCommand SaveBodeToExcel
        {
            get { return new ActionCommand(SaveBode); }
        }
        public void SaveBode()
        {
            string Date = OscilloscopeViewModel.Day(DateTime.Now.Day) + ' ' + OscilloscopeViewModel.MonthTrans(DateTime.Now.Month) + ' ' + DateTime.Now.Year.ToString();
            string path = "\\MotorController\\Charts\\" + Date + ' ' + DateTime.Now.ToString("HH:mm:ss");
            path = (path.Replace('-', ' ')).Replace(':', '_');
            path += ".csv";
            path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + path;
            filePath = path;

            Thread.Sleep(100);
            StringBuilder sb = new StringBuilder();

            float[] yxls = _excel_Y1_List.ToArray();
            float[] yxls2 = _excel_Y2_List.ToArray();

            string[] xstring = new string[_excel_X_List.Count + 1];

            string[] ystring = new string[_excel_Y1_List.Count + 1];

            xstring = new string[_excel_X_List.Count + 1];
            ystring[0] = "Y1 - Magnitude (dB)";


            string[] ystring2 = new string[_excel_Y2_List.Count + 1];

            xstring = new string[_excel_X_List.Count + 1];
            ystring2[0] = "Y2 - Phase (Degree)";

            xstring[0] = "Time";


            for(int i = 1; i < _excel_X_List.Count; i++)
            {
                xstring[i] = (_excel_X_List.ElementAt(i)).ToString(CultureInfo.CurrentCulture);
                ystring[i] = yxls[i - 1].ToString(CultureInfo.CurrentCulture);
                ystring2[i] = yxls2[i - 1].ToString(CultureInfo.CurrentCulture);
                sb.AppendLine(string.Join(delimiter, xstring[i - 1], ystring[i - 1], ystring2[i - 1]));
            }
            System.Windows.Forms.SaveFileDialog saveFile = new System.Windows.Forms.SaveFileDialog();
            saveFile.Filter = "Excel (*.xlsx)|*.csv";
            saveFile.FileName = path;
            var t = new Thread((ThreadStart)(() =>
            {
                if(saveFile.ShowDialog() == DialogResult.OK)
                {
                    filePath = saveFile.FileName;
                }
                else
                    return;
                File.AppendAllText(filePath, sb.ToString());
                sb.Clear();
            }));
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

    }
}