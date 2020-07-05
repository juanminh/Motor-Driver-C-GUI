using Abt.Controls.SciChart;
using Abt.Controls.SciChart.Example.Common;
using Abt.Controls.SciChart.Example.Data;
using Abt.Controls.SciChart.Model.DataSeries;
using Abt.Controls.SciChart.Visuals.Axes;
using SuperButton.CommandsDB;
using SuperButton.Data;
using SuperButton.Models.DriverBlock;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Timer = System.Timers.Timer;

namespace SuperButton.ViewModels
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
            InitializeAxes();
            Load();
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
                _bodeStartStop = value;

                // get call stack
                StackTrace stackTrace = new StackTrace();

                if(stackTrace.GetFrame(1).GetMethod().Name != "UpdateModel")
                {
                    Rs232Interface.GetInstance.SendToParser(new PacketFields
                    {
                        Data2Send = _bodeStartStop ? 1 : 0,
                        ID = Convert.ToInt16(6),
                        SubID = Convert.ToInt16(15),
                        IsSet = true,
                        IsFloat = false
                    });
                    if(value)
                    {
                        ChartData = new XyDataSeries<float, float>();
                        ChartData1 = new XyDataSeries<float, float>();
                        if(!String.IsNullOrEmpty(Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(15, 2)].CommandValue) &&
                            !String.IsNullOrEmpty(Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(15, 3)].CommandValue))
                            XAxisDoubleRange = new DoubleRange(Convert.ToInt32(Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(15, 2)].CommandValue), Convert.ToInt32(Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(15, 3)].CommandValue));
                        OnBodeStart();
                    }
                    else
                        OnBodeStop();
                }

                OnPropertyChanged();

            }
        }
        private IAxis _xAxis;
        public IAxis XAxis
        {
            get { return _xAxis; }
            set
            {
                _xAxis = value;
                OnPropertyChanged("XAxis");
            }
        }
        private AxisCollection _chartYAxes = new AxisCollection() {
            (new NumericAxis() {
            Id = "DefaultAxisId",
            Visibility = Visibility.Hidden
        })
        };

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
        private DoubleRange _xAxisDoubleRange = new DoubleRange(0, 1000);
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
        [STAThread]
        private void InitializeAxes()
        {
            lock(_lockAxes)
            {
                ChartData = new XyDataSeries<float, float>();
                ChartData1 = new XyDataSeries<float, float>();

                _xAxisLog = new LogarithmicNumericAxis
                {
                    TextFormatting = "#.#E+0",
                    ScientificNotation = ScientificNotation.LogarithmicBase,
                    VisibleRange = XAxisDoubleRange,
                    GrowBy = new DoubleRange(0.2, 0.2),
                    DrawMajorBands = false,
                    //AxisTitle = "Time (ms)",
                    DrawMinorGridLines = true,
                    DrawMajorTicks = true,
                    DrawMinorTicks = true,
                    StrokeThickness = 1,
                    LogarithmicBase = 10.0
                };
                XAxis = _xAxisLog;

                //Load();

                //_yAxisLog = new LogarithmicNumericAxis
                //{
                //    TextFormatting = "#.#E+0",
                //    ScientificNotation = ScientificNotation.LogarithmicBase,
                //    AxisAlignment = AxisAlignment.Left,
                //    GrowBy = new DoubleRange(0.1, 0.1),
                //    DrawMajorBands = false
                //};

                IAxis Y1 = new NumericAxis()
                {
                    Id = "Y1_Magnitude",
                    TextFormatting = "#",
                    ScientificNotation = ScientificNotation.Normalized,
                    AxisAlignment = AxisAlignment.Left,
                    GrowBy = new DoubleRange(0.2, 0.2),
                    VisibleRange = new DoubleRange(-50, 20),
                    AxisTitle = "Magnitude (dB)",
                    TickTextBrush = new SolidColorBrush(Colors.White),
                    DrawMajorBands = false,
                    DrawMinorGridLines = false,
                    DrawMajorTicks = false,
                    DrawMinorTicks = false

                };
                IAxis Y2 = new NumericAxis()
                {
                    Id = "Y2_Phase",
                    TextFormatting = "#",
                    ScientificNotation = ScientificNotation.Normalized,
                    AxisAlignment = AxisAlignment.Right,
                    GrowBy = new DoubleRange(0.2, 0.2),
                    VisibleRange = new DoubleRange(-200, 200),
                    AxisTitle = "Phase (Degree)",
                    TickTextBrush = new SolidColorBrush(Colors.White),
                    DrawMajorBands = false,
                    DrawMinorGridLines = false,
                    DrawMajorTicks = false,
                    DrawMinorTicks = false
                };
                ChartYAxes.Add(Y1);
                ChartYAxes.Add(Y2);
            }

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

        private ObservableCollection<object> _bodeList;
        public ObservableCollection<object> BodeList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["BodeList"];
            }
            set
            {
                _bodeList = value;
                OnPropertyChanged();
            }

        }
        public ConcurrentQueue<float> FifoplotBodeListX = new ConcurrentQueue<float>();
        public ConcurrentQueue<float> FifoplotBodeListY1 = new ConcurrentQueue<float>();
        public ConcurrentQueue<float> FifoplotBodeListY2 = new ConcurrentQueue<float>();
        private List<float> X_List = new List<float>();
        private List<float> Y1_List = new List<float>();
        private List<float> Y2_List = new List<float>();
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
                    newPropertyValuef = newPropertyValuef < -50 ? -50 : newPropertyValuef;
                    FifoplotBodeListY1.Enqueue((float)newPropertyValuef);
                    Debug.WriteLine("Y1: " + newPropertyValuef.ToString());

                    // Y2
                    element = ((PlotList[i][10] << 24) | (PlotList[i][11] << 16) | (PlotList[i][12] << 8) | (PlotList[i][13]));
                    FifoplotBodeListY2.Enqueue((element / iqFactor) * 360);
                    Debug.WriteLine("Y2: " + ((element / iqFactor) * 360).ToString());

                }
            }
            //PlotList.Clear();
            //Debug.WriteLine("ParsePlot 2" + DateTime.Now.ToString("h:mm:ss.fff"));
        }
        private Timer _timer;
        private const double TimerIntervalMs = 100;
        public static bool _chartRunning = false;
        public void OnBodeStop()
        {
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
        }
        // Setup start condition when the example enters
        public void OnBodeStart()
        {
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
        }
        private void OnTick(object sender, EventArgs e)
        {
            Rs232Interface.GetInstance.SendToParser(new PacketFields
            {
                Data2Send = "0",
                ID = 6,
                SubID = 15,
                IsSet = false,
                IsFloat = false
            });

            float item;
            while(FifoplotBodeListX.TryDequeue(out item))
            {
                X_List.Add(item);
                FifoplotBodeListY1.TryDequeue(out item);
                Y1_List.Add(item);
                FifoplotBodeListY2.TryDequeue(out item);
                Y2_List.Add(item);
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
                        //_series0.Clear();
                        //_series1.Clear();
                        _series0.Append(X_arr, Y1_arr);
                        _series1.Append(X_arr, Y2_arr);
                    }
                }

                X_List.Clear();
                Y1_List.Clear();
                Y2_List.Clear();
            }
        }
        private bool _startBodeEnable = true;
        private bool _stopBodeEnable = false;
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
    }
}