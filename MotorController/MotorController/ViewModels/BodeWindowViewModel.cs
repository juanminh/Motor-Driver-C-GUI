using Abt.Controls.SciChart;
using Abt.Controls.SciChart.Example.Common;
using Abt.Controls.SciChart.Example.Data;
using Abt.Controls.SciChart.Model.DataSeries;
using Abt.Controls.SciChart.Visuals.Axes;
using MotorController.Common;
using MotorController.Data;
using MotorController.Models;
using MotorController.Views;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;

namespace MotorController.ViewModels
{
    public partial class BodeWindowViewModel : BaseViewModel //INotifyPropertyChanged
    {
        //public event PropertyChangedEventHandler PropertyChanged = delegate { };
        //public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;

        //    if(handler != null)
        //        handler(this, new PropertyChangedEventArgs(propertyName));
        //}
        //public void OnPropertyChanged(PropertyChangedEventArgs e)
        //{
        //    if(PropertyChanged != null)
        //    {
        //        PropertyChanged(this, e);
        //    }
        //}
        private static readonly object Synlock = new object();
        public static readonly object PlotBodeListLock = new object();             //Singletone variable

        private static BodeWindowViewModel _instance;
        public static BodeWindowViewModel GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new BodeWindowViewModel();
                    return _instance;
                }
            }
            set
            {
                _instance = value;
            }
        }
        public BodeWindowViewModel()
        {
            InitializeAxes();
        }
        public ObservableCollection<object> BodeStart
        {
            get
            {
                return Commands.GetInstance.GenericCommandsGroup["BodeStart"];
            }
        }
        private System.Threading.Timer _timer;
        public async void update_bode_indicator(bool val)
        {
            await Task.Run(() => BodeStartStop = val);
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                BodeStartStop = val;
                GetInstance.BodeStartStop = val;
            });
            BodeStartStop = val;
            GetInstance.BodeStartStop = val;
            
            _timer = new System.Threading.Timer((x) => System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => BodeStartStop = val)), null, 100, 0);
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
                //if(!LeftPanelViewModel._app_running || _bodeStartStop == value)
                //    return;

                _bodeStartStop = value;
                OnPropertyChanged();
                return;
                if(value)
                {
                    ChartData = new XyDataSeries<float, float>();
                    ChartData1 = new XyDataSeries<float, float>();
                    X_List.Clear();
                    Y1_List.Clear();
                    Y2_List.Clear();
                    OnBodeStart();
                }
            }
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
        private LogarithmicNumericAxis _xAxisLog;
        private static readonly object _lockAxes = new object();
        private DoubleRange _xAxisDoubleRange = new DoubleRange(0.6, 1000);

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
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    AutoRange _xAutorange = AutoRange.Never;
                    lock(_lockAxes)
                    {

                        _xAxisLog = new LogarithmicNumericAxis
                        {
                            TextFormatting = "#0#",
                            ScientificNotation = ScientificNotation.Normalized,
                            VisibleRange = _xAxisDoubleRange,
                            GrowBy = new DoubleRange(0.1, 0.1),
                            DrawMajorBands = false,
                            AutoRange = _xAutorange,
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
                            TextFormatting = "#0#",
                            ScientificNotation = ScientificNotation.Normalized,
                            VisibleRange = _xAxisDoubleRange,
                            GrowBy = new DoubleRange(0.1, 0.1),
                            DrawMajorBands = false,
                            AutoRange = _xAutorange,
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
                });
            }
            catch { }
        }

        private IXyDataSeries<float, float> _series0 = new XyDataSeries<float, float>();
        private IXyDataSeries<float, float> _series1 = new XyDataSeries<float, float>();
        private IXyDataSeries<float, float> _series2 = new XyDataSeries<float, float>();

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
        public ObservableCollection<object> DataBodeList
        {
            get
            {
                return Commands.GetInstance.GenericCommandsGroup["DataBodeList"];
            }
        }
        public ObservableCollection<object> EnumBodeList
        {

            get
            {
                return Commands.GetInstance.GenericCommandsGroup["EnumBodeList"];
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
        //private Timer _timer;
        private const double TimerIntervalMs = 100;
        public static bool _chartRunning = false;

        // Setup start condition when the example enters
        public void OnBodeStart()
        {
            _excel_X_List.Clear();
            _excel_Y1_List.Clear();
            _excel_Y2_List.Clear();
            ResetZoom();
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

        private string filePath;
        string delimiter = ",";

        public ActionCommand ResetZoomBode
        {
            get { return new ActionCommand(ResetZoom); }
        }
        public void ResetZoom()
        {
            try
            {
                GetInstance.update_bode_indicator(true);
                return;
                _magVisibleRange = new DoubleRange(-50, 20);
                _phaseVisibleRange = new DoubleRange(-200, 20);
                _xAxisDoubleRange = new DoubleRange(0.5, 2000);

                if(!String.IsNullOrEmpty(((DataViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(15, 2)]).CommandValue) &&
                    !String.IsNullOrEmpty(((DataViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(15, 3)]).CommandValue))
                    _xAxisDoubleRange = new DoubleRange(Convert.ToSingle(((DataViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(15, 2)]).CommandValue) -
                        0.1 * Convert.ToSingle(((DataViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(15, 2)]).CommandValue),
                        Convert.ToSingle(((DataViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(15, 3)]).CommandValue) +
                        0.1 * Convert.ToSingle(((DataViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(15, 3)]).CommandValue));

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
                        _phaseVisibleRange = new DoubleRange(-1.5, +1.5);
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

        public virtual ICommand BodeWindowLoaded
        {
            get
            {
                return new RelayCommand(BodeWindowLoaded_Func);
            }
        }
        public ICommand BodeWindowClosed
        {
            get
            {
                return new RelayCommand(BodeWindowClosed_Func);
            }
        }
        public static bool _is_bode_window_opened = false;
        private void BodeWindowLoaded_Func()
        {
            DebugViewModel.updateList = true;
            LeftPanelViewModel.GetInstance.cancelRefresh = new CancellationToken(true);
            RefreshManager.GetInstance.BuildGenericCommandsList_Func();
        }
        private void BodeWindowClosed_Func()
        {
            LeftPanelViewModel.GetInstance._bode_window.Visibility = Visibility.Hidden;
            LeftPanelViewModel.GetInstance.cancelRefresh = new CancellationToken(true);
            DebugViewModel.updateList = true;
            RefreshManager.GetInstance.BuildGenericCommandsList_Func();
        }
    }
}
