﻿//#define DEBUG_PLOT
//#define PLOT_CHUNKED
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Abt.Controls.SciChart;
using Abt.Controls.SciChart.Example.Common;
using Abt.Controls.SciChart.Model.DataSeries;
using Abt.Controls.SciChart.Numerics;
using MotorController.Common;
using MotorController.Models.DriverBlock;
using MotorController.ViewModels;
using Timer = System.Timers.Timer;
using MotorController.Models.ParserBlock;
using System.Diagnostics;
using MotorController.Helpers;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MotorController.Models;
using System.Windows.Data;
using Abt.Controls.SciChart.Visuals.Axes;
using MotorController.Data;
using Abt.Controls.SciChart.Example.Data;
using System.Windows.Media;

//Cntl+M and Control+O for close regions
namespace MotorController.Views
{

    public class OscilloscopeViewModel : BaseViewModel
    {
        #region members
        public Dictionary<string, string> ChannelYtitles = new Dictionary<string, string>();

        //CH1 ComboBox
        int ch1;
        public ObservableCollection<string> _channel1SourceItems = new ObservableCollection<string>();
        public ObservableCollection<string> Channel1SourceItems
        {
            get
            {
                if(_channel1SourceItems == null)
                    _channel1SourceItems = new ObservableCollection<string>();
                return _channel1SourceItems;
            }
            set
            {
                _channel1SourceItems = value;
                OnPropertyChanged("Channel1SourceItems");
            }
        }
        public object _lock = new object();          //Single tone variable

        private string _selectedCh1DataSource;

        //CH2 ComboBox
        int ch2;
        private string _selectedCh2DataSource;

        private UInt16 plotActivationstate;

        private bool _isFull = false;
        private float[] xData;
        private int pivot = 0;

        readonly List<float> AllYData = new List<float>(500000);//500000
        readonly List<float> AllYData2 = new List<float>(500000);

        private static float _duration = 5.0F; //0.01F; // 10 ms

        private DoubleRange _xVisibleRange = new DoubleRange(0, (_duration * 1000));
        private DoubleRange _yVisibleRange;

        private ModifierType _chartModifier;

        public Timer _timer;

        //private ResamplingMode _resamplingMode;

        private int POintstoPlot = 33000; //5 sec min


        public List<float> RecList = new List<float>();
        public List<float> RecList2 = new List<float>();


        private int _undesample = 1;
        private uint _undesampleCounter = 0;

        private const double TimerIntervalMs = 100;

        private int ucarry;
        private int State = 0;

        List<float> utemp3L = new List<float>();
        float[] utemp3;


        #endregion
        #region XYAxis
        private NumericAxis _xAxisNum;

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

        private static readonly object _lockAxes = new object();
        [STAThread]
        private void InitializeAxes()
        {
            lock(_lockAxes)
            {
                _xAxisNum = new NumericAxis
                {
                    ScientificNotation = ScientificNotation.Normalized,
                    AnimatedVisibleRange = _xVisibleRange,
                    VisibleRangeLimit = _xVisibleRange,
                    AxisTitle = "Time (ms)",
                    DrawMajorBands = false,
                    DrawMinorGridLines = false,
                    DrawMajorTicks = true,
                    DrawMinorTicks = true,
                    StrokeThickness = 1
                };
                XAxis = _xAxisNum;
            }
        }
        #endregion
        #region Yzoom
        public double _yzoom = 0;
        public ActionCommand YPlus
        {
            get { return new ActionCommand(YDirPlus); }
        }
        public ActionCommand YMinus
        {
            get { return new ActionCommand(YDirMinus); }
        }
        public void YDirMinus()
        {
            //Debug.WriteLine("YDirMinus");
            if(_timer == null)
            {
                EventRiser.Instance.RiseEevent(string.Format($"No plot detected !"));
                return;
            }

            if(_yzoom > 0.002)
            {
                _yzoom = _yzoom / 2;
                YLimit = new DoubleRange(-_yzoom, _yzoom); //ubdate visible limits        
                YVisibleRange = YLimit;
            }
        }
        public void YDirPlus()
        {
            //Debug.WriteLine("YDirPlus");
            if(_timer == null)
            {
                EventRiser.Instance.RiseEevent(string.Format($"No plot detected !"));
                return;
            }
            _yzoom = _yzoom * 2;
            YLimit = new DoubleRange(-_yzoom, _yzoom); //ubdate visible limits        
            YVisibleRange = YLimit;
        }
        #endregion
        #region Duration
        public ActionCommand DirectionPlus
        {
            get { return new ActionCommand(DirPlus); }
        }
        public ActionCommand DirectionMinus
        {
            get { return new ActionCommand(DirMinus); }
        }
        public void DirPlus() // - minus button
        {
            if(_timer != null)
            {
                lock(/*_timer*/Synlock)
                {
                    _duration *= 2;
                    POintstoPlot = (int)(OscilloscopeParameters.ChanelFreq * _duration);  //update resolution
                }
            }
            else
            {
                _duration *= 2;
                POintstoPlot = (int)(OscilloscopeParameters.ChanelFreq * _duration);  //update resolution
                _yFloats = new float[0];
                _yFloats2 = new float[0];
            }

            float _temp_duration = _duration;
            _temp_duration *= 1000;
            _xVisibleRange = new DoubleRange(0, _temp_duration); //ubdate visible limits        

            _isFull = false;
            InitializeAxes();
        }
        public void DirMinus() // + plus button
        {
            if(_duration <= 0.01)
                return;
            if(_timer != null)
            {
                lock(/*_timer*/Synlock)
                {
                    _duration /= 2;
                    POintstoPlot = (int)(OscilloscopeParameters.ChanelFreq * _duration);  //update resolution
                }
            }
            else
            {
                _duration /= 2;
                POintstoPlot = (int)(OscilloscopeParameters.ChanelFreq * _duration);  //update resolution
                _yFloats = new float[0];
                _yFloats2 = new float[0];
            }

            float _temp_duration = _duration;
            _temp_duration *= 1000;
            _xVisibleRange = new DoubleRange(0, _temp_duration); //ubdate visible limits        
            _undesample = 1;
            pivot = (int)0; //update pivote and move to initial state
            _isFull = false;
            InitializeAxes();
        }

        #endregion
        #region Constractor
        private static OscilloscopeViewModel _instance;
        private static readonly object Synlock = new object(); //Single tone variable

        public static OscilloscopeViewModel GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new OscilloscopeViewModel();
                    return _instance;
                }
            }
        }

        public OscilloscopeViewModel()
        {
            InitializeAxes();
            IsFreeze = RecFlag = false;
            //Initial frame duration is 5 seconds
            POintstoPlot = (int)(OscilloscopeParameters.SingleChanelFreqC * _duration/*0.01*/);//20Khz/3*5Seconds -> 5000 

            xData = new float[0];
            _yFloats = new float[0];
            _yFloats2 = new float[0];

            BindingOperations.EnableCollectionSynchronization(_channel1SourceItems, _lock);

            Thread.Sleep(1);
            //ResetZoom();
        }
        public void FillDictionary()
        {
            Channel1SourceItems.Clear();

            Channel1SourceItems.Add("Pause");
            _channel1SourceItems.Add("IqFeedback");
            _channel1SourceItems.Add("I_PhaseA");
            _channel1SourceItems.Add("I_PhaseB");
            _channel1SourceItems.Add("I_PhaseC");
            _channel1SourceItems.Add("VDC_Motor");
            _channel1SourceItems.Add("BEMF_PhaseA");
            _channel1SourceItems.Add("BEMF_PhaseB");
            _channel1SourceItems.Add("BEMF_PhaseC");
            _channel1SourceItems.Add("HALL_LPF_Speed");
            _channel1SourceItems.Add("HALL_Elect_Angle");
            _channel1SourceItems.Add("QEP1_LPF_Speed");
            _channel1SourceItems.Add("QEP1_Elect_Angle");
            _channel1SourceItems.Add("QEP2_LPF_Speed");
            _channel1SourceItems.Add("QEP2_Elect_Angle");
            _channel1SourceItems.Add("SSI_LPF_Speed");
            _channel1SourceItems.Add("SSI_Elect_Angle");
            _channel1SourceItems.Add("SL_Elect_Angle");
            _channel1SourceItems.Add("IRms");
            _channel1SourceItems.Add("IRms(Filtered)");
            _channel1SourceItems.Add("SL_LPF_Speed");
            _channel1SourceItems.Add("CommutationAngle");
            _channel1SourceItems.Add("SpeedFdbRPM");
            _channel1SourceItems.Add("SpeedRefRPM");
            _channel1SourceItems.Add("Test_Signal");
            _channel1SourceItems.Add("Cla_filt0");
            _channel1SourceItems.Add("Cmd_Ref");
            _channel1SourceItems.Add("Cmd_Ref_filt");
            _channel1SourceItems.Add("SinEnc");
            _channel1SourceItems.Add("CosEnc");
            _channel1SourceItems.Add("InterAngle");
            _channel1SourceItems.Add("SpeedRefPI");
            _channel1SourceItems.Add("SpeedFdb");
            _channel1SourceItems.Add("CurrentRefPI");

            ChannelYtitles.Clear();
            ChannelYtitles.Add("Pause", "");
            ChannelYtitles.Add("IqFeedback", "Current [A]");
            ChannelYtitles.Add("I_PhaseA", "Current [A]");
            ChannelYtitles.Add("I_PhaseB", "Current [A]");
            ChannelYtitles.Add("I_PhaseC", "Voltage [V]");
            ChannelYtitles.Add("VDC_Motor", "Voltage [V]");
            ChannelYtitles.Add("BEMF_PhaseA", "Voltage [V]");
            ChannelYtitles.Add("BEMF_PhaseB", "Voltage [V]");
            ChannelYtitles.Add("BEMF_PhaseC", "BEMF_PhaseC");
            ChannelYtitles.Add("HALL_LPF_Speed", "Velocity [KRPM]");
            ChannelYtitles.Add("HALL_Elect_Angle", "Angle [Deg]");
            ChannelYtitles.Add("QEP1_LPF_Speed", "Velocity [KRPM]");
            ChannelYtitles.Add("QEP2_Elect_Angle", "Angle [Deg]");
            ChannelYtitles.Add("QEP2_LPF_Speed", "Velocity [KRPM]");
            ChannelYtitles.Add("SSI_LPF_Speed", "Velocity [KRPM]");
            ChannelYtitles.Add("SSI_Elect_Angle", "Angle [Deg]");
            ChannelYtitles.Add("SL_Elect_Angle", "Angle [Deg]");
            ChannelYtitles.Add("IRms", "Current [A]");
            ChannelYtitles.Add("SL_LPF_Speed", "Velocity [KRPM]");
            ChannelYtitles.Add("CommutationAngle", "Angle [Deg]");
            ChannelYtitles.Add("PositionFdb", "Position [Counts]");
            ChannelYtitles.Add("PositionRef", "Position [Counts]");
            ChannelYtitles.Add("Test_Signal", "");
            ChannelYtitles.Add("Cla_filt0", "");
            ChannelYtitles.Add("Cmd_Ref", "");
            ChannelYtitles.Add("Cmd_Ref_filt", "");//28
            ChannelYtitles.Add("SinEnc", ""); //31
            ChannelYtitles.Add("CosEnc", ""); //32
            ChannelYtitles.Add("InterAngle", ""); //33
            ChannelYtitles.Add("SpeedRefPI", ""); //34
            ChannelYtitles.Add("SpeedFdb", ""); //35
            ChannelYtitles.Add("CurrentRefPI", ""); //36

        }
        #endregion
        #region ActionCommnds
        public ActionCommand SetRolloverModifierCommand
        {
            get { return new ActionCommand(() => SetModifier(ModifierType.Rollover)); }
        }
        public ActionCommand SetCursorModifierCommand
        {
            get { return new ActionCommand(() => SetModifier(ModifierType.CrosshairsCursor)); }
        }
        public ActionCommand SetRubberBandZoomModifierCommand
        {
            get { return new ActionCommand(() => SetModifier(ModifierType.RubberBandZoom)); }
        }
        public ActionCommand SetZoomPanModifierCommand
        {
            get { return new ActionCommand(() => SetModifier(ModifierType.ZoomPan)); }
        }
        public ActionCommand SetNullModifierCommand
        {
            get { return new ActionCommand(() => SetModifier(ModifierType.Null)); }
        }
        public ActionCommand ResetZoomCommand
        {
            get { return new ActionCommand(ResetZoom); }
        }
        public ActionCommand ClearGraphCommand
        {
            get { return new ActionCommand(ClearGraph); }
        }
        #endregion
        private int ActChenCount = 0;

        #region Channels
        private int _chan1Counter = 0;
        private int _chan2Counter = 0;

        private IXyDataSeries<float, float> _series0;
        private IXyDataSeries<float, float> _series1;

        public void ChannelsplotActivationMerge()
        {
            //Activate plot
            if(OscilloscopeParameters.ChanTotalCounter > 0 && plotActivationstate == 0)
            {
                ChartData = new XyDataSeries<float, float>();
                ChartData1 = new XyDataSeries<float, float>();

                OnExampleEnter();
                plotActivationstate = 1;
            }
            else if(OscilloscopeParameters.ChanTotalCounter == 0 && plotActivationstate == 1)
            {
                plotActivationstate = 0;
                OnExampleExit();
            }
        }
        public void _update_channel_count(int ch, int comboBox)
        {
            if(comboBox == 1)
                ch1 = ch;
            else
                ch2 = ch;

            if(ch >= 0 && OscilloscopeParameters.ScaleAndGainList.Count != 0)
            {
                //Channel One
                switch(comboBox)
                {
                    case (1):
                        _chan1Counter = (ch == 0) ? 0 : 1;
                        OscilloscopeParameters.Gain = OscilloscopeParameters.ScaleAndGainList.ElementAt(ch).Item1;
                        OscilloscopeParameters.FullScale = OscilloscopeParameters.ScaleAndGainList.ElementAt(ch).Item2;
                        break;
                    case (2):
                        _chan2Counter = (ch == 0) ? 0 : 1;
                        OscilloscopeParameters.Gain2 = OscilloscopeParameters.ScaleAndGainList.ElementAt(ch).Item1;
                        OscilloscopeParameters.FullScale2 = OscilloscopeParameters.ScaleAndGainList.ElementAt(ch).Item2;
                        break;
                }
            }
        }
        public void _update_pivot_buffer()
        {
            OscilloscopeParameters.ChanelFreq = OscilloscopeParameters.ChanTotalCounter == 0 ? OscilloscopeParameters.SingleChanelFreqC : OscilloscopeParameters.SingleChanelFreqC / OscilloscopeParameters.ChanTotalCounter;
            int ActualPOintstoPlot = POintstoPlot;//Actual points to plot transit value
            POintstoPlot = (int)(OscilloscopeParameters.ChanelFreq * _duration);
            if(ActualPOintstoPlot != POintstoPlot)//Reset
            {
                ActChenCount = 1;
                _isFull = false;
                pivot = 0;
                if(ChartData == null)
                    ChartData = new XyDataSeries<float, float>();
                if(ChartData1 == null)
                    ChartData1 = new XyDataSeries<float, float>();

                using(ChartData.SuspendUpdates())
                {
                    using(ChartData1.SuspendUpdates())
                    {
                        ChartData.Clear();
                        ChartData1.Clear();
                    }
                }
                _yFloats = new float[0];
                _yFloats2 = new float[0];
                AllYData.Clear();
            }
        }
        public void ChannelsYaxeMerge(int ch, int comboBox)
        {
            _update_channel_count(ch, comboBox);

            OscilloscopeParameters.ChanTotalCounter = _chan1Counter + _chan2Counter;

            if(OscilloscopeParameters.ChanTotalCounter == 1)
            {
                YVisibleRange = ch1 != 0
                    ? new DoubleRange(min: -OscilloscopeParameters.FullScale,
                        max: OscilloscopeParameters.FullScale)
                    : new DoubleRange(min: -OscilloscopeParameters.FullScale2,
                        max: OscilloscopeParameters.FullScale2);
                _yzoom = YVisibleRange.Max;
            }
            else if(OscilloscopeParameters.ChanTotalCounter == 2) //both of channels active
            {
                if(!RefreshManager.GetInstance.DisconnectedFlag)
                {
                    YVisibleRange = OscilloscopeParameters.FullScale > OscilloscopeParameters.FullScale2
                    ? new DoubleRange(min: -OscilloscopeParameters.FullScale,
                        max: OscilloscopeParameters.FullScale)
                    : new DoubleRange(min: -OscilloscopeParameters.FullScale2,
                    max: OscilloscopeParameters.FullScale2);

                    _yzoom = YVisibleRange.Max;
                    _xVisibleRange = new DoubleRange(0, _duration * 1000); //ubdate visible limits        
                    _undesample = 1;
                    //InitializeAxes();

                    pivot = (int)0; //update pivote and move to initial state
                    _isFull = false;

                    _yFloats = new float[0];
                    _yFloats2 = new float[0];

                    while(ParserRayonM1.GetInstanceofParser.FifoplotList.IsEmpty == false)
                    {
                        float dummy;
                        ParserRayonM1.GetInstanceofParser.FifoplotList.TryDequeue(out dummy);
                    }

                    while(ParserRayonM1.GetInstanceofParser.FifoplotListCh2.IsEmpty == false)
                    {
                        float dummy;
                        ParserRayonM1.GetInstanceofParser.FifoplotListCh2.TryDequeue(out dummy);
                    }

                    if(AllYData.Count > 0)
                        AllYData.Clear();
                }
            }

            _update_pivot_buffer();
        }
        public void StepRecalcMerge()
        {
            if(_timer != null)
                lock(/*_timer*/Synlock)
                {
                    OscilloscopeParameters.Step = OscilloscopeParameters.ChanTotalCounter > 0
                        ? OscilloscopeParameters.ChanTotalCounter * 1000 / OscilloscopeParameters.SingleChanelFreqC
                        : 1000 / OscilloscopeParameters.SingleChanelFreqC;
                    OscilloscopeParameters.ChanelFreq =
                        (float)
                            (OscilloscopeParameters.SingleChanelFreqC * (1.0 / OscilloscopeParameters.ChanTotalCounter));

                }
            else
            {
                OscilloscopeParameters.Step = OscilloscopeParameters.ChanTotalCounter > 0
                    ? OscilloscopeParameters.ChanTotalCounter * 1000 / OscilloscopeParameters.SingleChanelFreqC
                    : 1000 / OscilloscopeParameters.SingleChanelFreqC;
                OscilloscopeParameters.ChanelFreq =
                    (float)(OscilloscopeParameters.SingleChanelFreqC * (1.0 / OscilloscopeParameters.ChanTotalCounter));
            }
        }
        #endregion Channels

        private string _graphTheme = Consts._project == Consts.eProject.REDLER ? "Oscilloscope" : "ExpressionDark";/*Oscilloscope*/
        public string GraphTheme
        {
            get { return _graphTheme; }
            set
            {
                _graphTheme = value;
                OnPropertyChanged("GraphTheme");
            }
        }
        private Color _colorChart1 = ((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsGroup["ChannelsList"][0]).ChBackground;
        private Color _colorChart2 = ((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsGroup["ChannelsList"][1]).ChBackground;

        public Color ColorChart1
        {
            get { return _colorChart1; }
            set
            {
                _colorChart1 = value;
                OnPropertyChanged("ColorChart1");
            }
        }
        public Color ColorChart2
        {
            get { return _colorChart2; }
            set
            {
                _colorChart2 = value;
                OnPropertyChanged("ColorChart2");
            }
        }

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
        public void ResetZoom()
        {
            try
            {
                if(ChartData == null || ChartData1 == null)
                    return;
                float maxval = 0, minval = 0;
                

                minval = ((float)ChartData.YMin > (float)ChartData1.YMin) ? (float)ChartData1.YMin : (float)ChartData.YMin;
                maxval = ((float)ChartData.YMax > (float)ChartData1.YMax) ? (float)ChartData.YMax : (float)ChartData1.YMax;

                minval *= (minval > 0 ? (float)0.5 : (float)1.1);
                if(minval == maxval)
                    return;
                _xVisibleRange = new DoubleRange(0, _duration * 1000);
                YLimit = new DoubleRange(minval, 1.1 * maxval);
                _yzoom = OscilloscopeParameters.FullScale;
                //XVisibleRange = XLimit;
                YVisibleRange = YLimit;
                InitializeAxes();
            }
            catch { }
        }

        private void ClearGraph()
        {
            lock(this)
            {
                try
                {
                    if(ChartData == null || ChartData1 == null)
                        return;
                    using(this.ChartData.SuspendUpdates())
                    {
                        using(this.ChartData1.SuspendUpdates())
                        {
                            _yFloats = new float[0];
                            _yFloats2 = new float[0];
                            pivot = 0;
                            ChartData.Clear();
                            ChartData1.Clear();
                            _isFull = false;
                        }
                    }
                }
                catch(Exception) { }
            }
        }
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
        private void SetModifier(ModifierType modifierType)
        {
            ChartModifier = modifierType;
        }

        // Reset state when example exits
        public void OnExampleExit()
        {
            lock(this)
            {
                if(_timer != null)
                {
                    lock(/*_timer*/Synlock)
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
        public void OnExampleEnter()
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

        private float[] _yFloats;
        private float[] _yFloats2;

        private float[] temp3;
        private float[] temp4;
        private int carry;
        private int carry2;
        private float[] yDataTemp;
        private float[] yDataTemp2;
        public static bool _chartRunning = false;
        /* On tick function */

        List<float> ytemp = new List<float>();
        List<float> ytemp2 = new List<float>();

        public static List<float> Ytemp = new List<float>();

        float step_temp = 0;
        private float iqFactor = (float)Math.Pow(2.0, -15);
        private int IntegerFactor = 1;
        float calcFactor(float dataSample, int ChNo)
        {
            switch(((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsList[
                    new Tuple<int, int>(
                        ((int)((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsGroup["ChannelsList"][ChNo - 1]).CommandId),
                        ((int)((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsGroup["ChannelsList"][ChNo - 1]).CommandSubId))]).PlotType)
            {
                case "Integer":
                    return dataSample * IntegerFactor;
                case "Int32":
                    return dataSample;
                case "Float":
                case "Iq24":
                case "Iq15":
                default:
                    return dataSample * iqFactor;
            }
        }
        float SubGain1 = 1, SubGain2 = 1;
        private void OnTick(object sender, EventArgs e)
        {
            try
            {
                SubGain1 = Convert.ToSingle(((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsList[
                    new Tuple<int, int>(
                        ((int)((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsGroup["ChannelsList"][0]).CommandId),
                        ((int)((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsGroup["ChannelsList"][0]).CommandSubId))]).Gain);
                SubGain2 = Convert.ToSingle(((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsList[
                        new Tuple<int, int>(
                            ((int)((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsGroup["ChannelsList"][1]).CommandId),
                            ((int)((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsGroup["ChannelsList"][1]).CommandSubId))]).Gain);
                if(!IsFreeze)
                {
                    lock(/*_timer*/Synlock)
                    {
                        State = 0;
                        if(step_temp != OscilloscopeParameters.Step)
                            step_temp = OscilloscopeParameters.Step;

                        if(OscilloscopeParameters.ChanTotalCounter == 1)
                        {
                            //Debug.WriteLine("1: OscilloscopeParameters.Gain - OscilloscopeParameters.FullScale - SubGain1: " + OscilloscopeParameters.Gain.ToString() + " - " + OscilloscopeParameters.FullScale.ToString() + " - " + SubGain1.ToString());
                            #region SingleChan
                            if(ParserRayonM1.GetInstanceofParser.FifoplotList.IsEmpty)
                            {
                                if(AllYData.Count > 1 && _isFull)
                                    State = 4;
                                else
                                    return;
                            }
                            else if(ActChenCount == 1)//First throw
                            {
                                float item;
                                while(ParserRayonM1.GetInstanceofParser.FifoplotList.TryDequeue(out item))
                                { }
                                ActChenCount = 0;
                            }
                            else //Collect whole the Data to the single grand list
                            {
                                Ytemp = new List<float>();
                                float item;

                                #region RecordAray
                                if(RecFlag)
                                {
                                    if(ch1 != 0)
                                    {
                                        while(ParserRayonM1.GetInstanceofParser.FifoplotList.TryDequeue(out item))
                                        {
                                            Ytemp.Add(calcFactor((item * OscilloscopeParameters.Gain * OscilloscopeParameters.FullScale * SubGain1), 1));
                                            RecList.Add(calcFactor((item * OscilloscopeParameters.Gain * OscilloscopeParameters.FullScale * SubGain1), 1));
                                        }
                                    }
                                    else
                                    {
                                        while(ParserRayonM1.GetInstanceofParser.FifoplotList.TryDequeue(out item))
                                        {
                                            Ytemp.Add(calcFactor((item * OscilloscopeParameters.Gain2 * OscilloscopeParameters.FullScale2 * SubGain2), 2));
                                            RecList2.Add(calcFactor((item * OscilloscopeParameters.Gain2 * OscilloscopeParameters.FullScale2 * SubGain2), 2));
                                        }
                                    }
                                }
                                #endregion RecordAray
                                else
                                {
                                    if(ch1 != 0)
                                    {
                                        while(ParserRayonM1.GetInstanceofParser.FifoplotList.TryDequeue(out item))
                                            Ytemp.Add(calcFactor((item * OscilloscopeParameters.Gain * OscilloscopeParameters.FullScale * SubGain1), 1));
                                    }
                                    else
                                    {
                                        while(ParserRayonM1.GetInstanceofParser.FifoplotList.TryDequeue(out item))
                                            Ytemp.Add(calcFactor((item * OscilloscopeParameters.Gain2 * OscilloscopeParameters.FullScale2 * SubGain2), 2));
                                    }
                                }

                                AllYData.AddRange(Ytemp);


                                if(_isFull)
                                    State = 4;
                                else if(POintstoPlot > pivot && _isFull == false)//fills buffer
                                    State = 2;
                                else if(POintstoPlot == pivot && _isFull == false) //buffer is full
                                {
                                    _isFull = true;
                                    State = 4;
                                }
                                else
                                    return;
                                #region Switch
                                switch(State)
                                {
                                    case (2): //Fills y buffer
                                        float[] temp;
                                        if((POintstoPlot - pivot) > 0)
                                            temp = AllYData.Take(POintstoPlot - pivot).ToArray();
                                        else
                                            return;
                                        if(_undesample == 1)
                                        {
                                            if(_yFloats.Length == 0) //Start fills
                                            {
                                                _yFloats = new float[temp.Length];
                                                xData = new float[temp.Length];

                                                //X fills
                                                for(int i = 0; i < temp.Length; i++)
                                                    xData[i] = i * OscilloscopeParameters.Step;

                                                Array.Copy(temp, 0, _yFloats, 0, temp.Length);
                                                pivot = temp.Length; // pivot
                                            } //Follow
                                            else
                                            {
                                                Array.Resize(ref xData, temp.Length + pivot);
                                                Array.Resize(ref _yFloats, temp.Length + pivot);

                                                for(int i = 0; i < pivot + temp.Length; i++)
                                                    xData[i] = i * OscilloscopeParameters.Step;

                                                Array.Copy(temp, 0, _yFloats, pivot, temp.Length);
                                                pivot = pivot + temp.Length;
                                            }
                                        }
                                        else // under sampled
                                        {
                                            if(_yFloats.Length == 0 && pivot == 0) //Start fills
                                            {

                                                utemp3L = new List<float>();

                                                for(int i = 0/*, j = 0*/; i < temp.Length; i++)
                                                {
                                                    if(_undesampleCounter++ == (_undesample - 1))
                                                    {
                                                        _undesampleCounter = 0;
                                                        utemp3L.Add(temp[i]);
                                                    }
                                                }
                                                xData = new float[utemp3L.Count];

                                                for(int i = 0; i < utemp3L.Count; i++)
                                                    xData[i] = i * OscilloscopeParameters.Step * _undesample;

                                                _yFloats = new float[utemp3L.Count];

                                                Array.Copy(utemp3L.ToArray(), 0, _yFloats, 0, utemp3L.Count);
                                                pivot += utemp3L.Count;
                                            } //Follow
                                            else
                                            {
                                                utemp3L = new List<float>();

                                                for(int i = 0/*, j = 0*/; i < temp.Length; i++)
                                                {
                                                    if(_undesampleCounter++ == (_undesample - 1))
                                                    {
                                                        _undesampleCounter = 0;
                                                        utemp3L.Add((float)temp[i]);
                                                    }
                                                }
                                                Array.Resize(ref xData, utemp3L.Count + pivot);
                                                Array.Resize(ref _yFloats, utemp3L.Count + pivot);

                                                for(int i = 0; i < utemp3L.Count + pivot; i++)
                                                    xData[i] = i * OscilloscopeParameters.Step * _undesample;

                                                Array.Copy(utemp3L.ToArray(), 0, _yFloats, pivot, utemp3L.Count);
                                                pivot += utemp3L.Count;

                                            }

                                        }
                                        lock(this)
                                        {
                                            using(this.ChartData.SuspendUpdates())
                                            {
                                                using(this.ChartData1.SuspendUpdates())
                                                {
                                                    ChartData.Clear();
                                                    ChartData1.Clear();

                                                    if(ch1 != 0)
                                                        ChartData.Append(xData, _yFloats);
                                                    else if(ch2 != 0)
                                                        ChartData1.Append(xData, _yFloats);
                                                }
                                            }
                                        }

                                        AllYData.RemoveRange(0, temp.Length - 1);

                                        if(utemp3L != null && utemp3L.Count > 0)
                                        {
                                            utemp3L.Clear();
                                        }
                                        break;

                                    case (4):
                                        if(_undesample == 1)
                                        {
                                            temp3 = AllYData.Take(POintstoPlot).ToArray();
                                            carry = temp3.Length;
                                            yDataTemp = new float[POintstoPlot];
                                            if(_yFloats.Length != 0)
                                            {
                                                Array.Copy(_yFloats, carry, yDataTemp, 0, _yFloats.Length - (carry)); //Shift Left
                                                Array.Copy(temp3, 0, yDataTemp, _yFloats.Length - carry, carry); // Add range
                                                Array.Copy(yDataTemp, 0, _yFloats, 0, POintstoPlot);
                                            }
                                            for(int i = 0; i < POintstoPlot; i++)
                                                xData[i] = i * (OscilloscopeParameters.Step * _undesample);
                                            using(this.ChartData.SuspendUpdates())
                                            {
                                                using(this.ChartData1.SuspendUpdates())
                                                {
                                                    ChartData.Clear();
                                                    ChartData1.Clear();

                                                    if(ch1 != 0)
                                                        ChartData.Append(xData, _yFloats);
                                                    else if(ch2 != 0)
                                                        ChartData1.Append(xData, _yFloats);
                                                }
                                            }

                                            AllYData.RemoveRange(0, (carry) - 1);
                                        }
                                        else
                                        {
                                            utemp3L = new List<float>();
                                            utemp3 = AllYData.Take(POintstoPlot).ToArray(); //Take Data
                                            ucarry = utemp3.Length;

                                            for(int i = 0/*, j = 0*/; i < utemp3.Length; i++)
                                            {
                                                if(_undesampleCounter++ == (_undesample - 1))
                                                {
                                                    _undesampleCounter = 0;
                                                    utemp3L.Add(utemp3[i]);
                                                }
                                            }

                                            temp3 = utemp3L.ToArray();
                                            carry = temp3.Length;
                                            yDataTemp = new float[POintstoPlot];
                                            Array.Copy(_yFloats, carry, yDataTemp, 0, _yFloats.Length - (carry)); //Shift Left
                                            Array.Copy(temp3, 0, yDataTemp, _yFloats.Length - carry, carry); // Add range
                                            Array.Copy(yDataTemp, 0, _yFloats, 0, POintstoPlot);

                                            for(int i = 0; i < POintstoPlot; i++)
                                                xData[i] = i * (OscilloscopeParameters.Step * _undesample);

                                            using(this.ChartData.SuspendUpdates())
                                            {
                                                using(this.ChartData1.SuspendUpdates())
                                                {
                                                    ChartData.Clear();
                                                    ChartData1.Clear();

                                                    if(ch1 != 0)
                                                        ChartData.Append(xData, _yFloats);
                                                    else if(ch2 != 0)
                                                        ChartData1.Append(xData, _yFloats);
                                                }
                                            }

                                            AllYData.RemoveRange(0, (ucarry) - 1);
                                        }
                                        break;
                                }
                                #endregion Switch
                            }
                            #endregion
                        }
                        else if(OscilloscopeParameters.ChanTotalCounter == 2)// Two channels
                        {
                            //Debug.WriteLine("2: OscilloscopeParameters.Gain - OscilloscopeParameters.FullScale - SubGain1: " + OscilloscopeParameters.Gain.ToString() + " - " + OscilloscopeParameters.FullScale.ToString() + " - " + SubGain1.ToString());

                            #region DoubleChan
                            if(ParserRayonM1.GetInstanceofParser.FifoplotList.IsEmpty)
                            {
                                if(AllYData.Count > 1 && _isFull)
                                    State = 4;
                                else
                                    return;
                            }
                            else if(ActChenCount == 1)//First throw
                            {
                                float item;
                                float item2;
                                //Collect data from first channel
                                while(ParserRayonM1.GetInstanceofParser.FifoplotList.TryDequeue(out item))
                                {
                                    ParserRayonM1.GetInstanceofParser.FifoplotListCh2.TryDequeue(out item2);
                                }
                                ActChenCount = 0;
                            }
                            else //Collect whole the Data to the single grand list
                            {
                                float item;
                                float item2;
#if(DEBUG && DEBUG_PLOT)
                            Debug.WriteLine("Plot 1: " + DateTime.Now.ToString("h:mm:ss.fff"));
#endif
                                //Collect data from first channel
                                #region RecordAray
                                if(RecFlag)
                                {
                                    while(ParserRayonM1.GetInstanceofParser.FifoplotList.TryDequeue(out item))
                                    {
                                        AllYData.Add(calcFactor((item * OscilloscopeParameters.Gain * OscilloscopeParameters.FullScale * SubGain1), 1));
                                        ParserRayonM1.GetInstanceofParser.FifoplotListCh2.TryDequeue(out item2);
                                        AllYData2.Add(calcFactor((item2 * OscilloscopeParameters.Gain2 * OscilloscopeParameters.FullScale2 * SubGain2), 2));

                                        RecList.Add(calcFactor((item * OscilloscopeParameters.Gain * OscilloscopeParameters.FullScale * SubGain1), 1));
                                        RecList2.Add(calcFactor((item2 * OscilloscopeParameters.Gain2 * OscilloscopeParameters.FullScale2 * SubGain2), 2));
                                    }
                                }
                                #endregion RecordAray
                                else
                                {
                                    while(ParserRayonM1.GetInstanceofParser.FifoplotList.TryDequeue(out item))
                                    {
                                        AllYData.Add(calcFactor((item * OscilloscopeParameters.Gain * OscilloscopeParameters.FullScale * SubGain1), 1));
                                        ParserRayonM1.GetInstanceofParser.FifoplotListCh2.TryDequeue(out item2);
                                        AllYData2.Add(calcFactor((item2 * OscilloscopeParameters.Gain2 * OscilloscopeParameters.FullScale2 * SubGain2), 2));
                                    }
                                }

                                if(_isFull)
                                    State = 4;
                                else if(POintstoPlot > pivot && _isFull == false)//fills buffer
                                    State = 2;
                                else if(POintstoPlot == pivot && _isFull == false) //buffer is full
                                {
                                    _isFull = true;
                                    State = 4;
                                }
                                else
                                    return;
                                #region Switch
#if(DEBUG && DEBUG_PLOT)
                            Debug.WriteLine(POintstoPlot);
                            Debug.WriteLine(State);
#endif
                                switch(State)
                                {
                                    case (2): //Fills y buffer
                                        #region case2
                                        float[] temp;
                                        float[] temp2;

                                        if((POintstoPlot - pivot) > 0)
                                        {
                                            temp2 = AllYData2.Take(POintstoPlot - pivot).ToArray();
                                            temp = AllYData.Take(POintstoPlot - pivot).ToArray();
                                        }
                                        else
                                            return;

                                        if(_yFloats.Length == 0) //Start fills
                                        {
                                            _yFloats = new float[temp.Length];
                                            _yFloats2 = new float[temp2.Length];

                                            xData = new float[temp.Length];

                                            for(int i = 0; i < temp.Length; i++)
                                            {
                                                xData[i] = i * OscilloscopeParameters.Step;
                                            }
                                            Array.Copy(temp, 0, _yFloats, 0, temp.Length);
                                            Array.Copy(temp2, 0, _yFloats2, 0, temp2.Length);
                                            pivot = temp.Length;
                                        } //Follow
                                        else
                                        {
                                            Array.Resize(ref xData, temp.Length + pivot);
                                            Array.Resize(ref _yFloats, temp.Length + pivot);
                                            Array.Resize(ref _yFloats2, temp2.Length + pivot);

                                            for(int i = 0; i < pivot + temp.Length; i++)
                                            {
                                                xData[i] = i * OscilloscopeParameters.Step;
                                            }
                                            Array.Copy(temp, 0, _yFloats, pivot, temp.Length);
                                            Array.Copy(temp2, 0, _yFloats2, pivot, temp.Length);
                                            pivot = pivot + temp.Length; // pivot
                                        }
                                        if(xData.Length != 0 && _yFloats.Length != 0 && _yFloats2.Length != 0)
                                        {
                                            lock(this)
                                            {
                                                using(this.ChartData.SuspendUpdates())
                                                {
                                                    using(this.ChartData1.SuspendUpdates())
                                                    {
                                                        ChartData.Clear();
                                                        ChartData1.Clear();
                                                        ChartData.Append(xData, _yFloats);
                                                        ChartData1.Append(xData, _yFloats2);
                                                    }
                                                }
                                            }
                                        }
                                        if(AllYData.Count > 0)
                                            AllYData.RemoveRange(0, temp.Length - 1);
                                        if(AllYData2.Count > 0)
                                            AllYData2.RemoveRange(0, temp2.Length - 1);
                                        #endregion case2
                                        break;
                                    case (4):
                                        //_isFull = false;
                                        temp3 = AllYData.Take(POintstoPlot).ToArray();
                                        temp4 = AllYData2.Take(POintstoPlot).ToArray();

                                        carry = temp3.Length;
                                        carry2 = temp4.Length;
                                        //1
                                        yDataTemp = new float[POintstoPlot];
                                        if(_yFloats.Length != 0)
                                        {
                                            Array.Copy(_yFloats, carry, yDataTemp, 0, _yFloats.Length - (carry)); //Shift Left - public static void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length);
                                            Array.Copy(temp3, 0, yDataTemp, _yFloats.Length - carry, carry); // Add range
                                            Array.Copy(yDataTemp, 0, _yFloats, 0, POintstoPlot);
                                        }
                                        //2
                                        yDataTemp2 = new float[POintstoPlot];
                                        if(_yFloats2.Length != 0)
                                        {
                                            Array.Copy(_yFloats2, carry, yDataTemp2, 0, _yFloats2.Length - (carry2)); //Shift Left
                                            Array.Copy(temp4, 0, yDataTemp2, _yFloats2.Length - carry2, carry2); // Add range
                                            Array.Copy(yDataTemp2, 0, _yFloats2, 0, POintstoPlot);
                                        }
                                        for(int i = 0; i < POintstoPlot; i++)
                                            xData[i] = i * (OscilloscopeParameters.Step * _undesample);
#if PLOT_CHUNKED
                                    float chunk = POintstoPlot / 100;
                                    int rest = 0;
                                    if((int)(chunk)*100 != POintstoPlot)
                                    {
                                        rest = POintstoPlot - (int)chunk*100;
                                    }
                                    _series0.Clear();
                                    _series1.Clear();
                                    for(int i = 0; i < (int)chunk; i++)
                                    {

                                        float[] Xaxis = SubArray(xData, i * 100, 100);
                                        
                                        using(this.ChartData.SuspendUpdates())
                                        {
                                            using(this.ChartData1.SuspendUpdates())
                                            {
                                                
                                                _series0.Append(Xaxis, SubArray(_yFloats, i * 100, 100));
                                                _series1.Append(Xaxis, SubArray(_yFloats2, i * 100, 100));
                                            }
                                        }
                                    }
                                    if(rest > 0)
                                    {
                                        float[] Xaxis = SubArray(xData, (int)chunk * 100, rest);
                                        
                                        using(this.ChartData.SuspendUpdates())
                                        {
                                            using(this.ChartData1.SuspendUpdates())
                                            {
                                                //_series0.Clear();
                                                //_series1.Clear();
                                                _series0.Append(Xaxis, SubArray(_yFloats, (int)chunk * 100, rest));
                                                _series1.Append(Xaxis, SubArray(_yFloats2, (int)chunk * 100, rest));
                                            }
                                        }
                                    }
#endif
                                        if(_yFloats.Length != 0 && _yFloats2.Length != 0)
                                        {
                                            using(this.ChartData.SuspendUpdates())
                                            {
                                                using(this.ChartData1.SuspendUpdates())
                                                {
                                                    ChartData.Clear();
                                                    ChartData1.Clear();
                                                    ChartData.Append(xData, _yFloats);
                                                    ChartData1.Append(xData, _yFloats2);
                                                }
                                            }
                                            AllYData.RemoveRange(0, (carry) - 1);
                                            AllYData2.RemoveRange(0, (carry2) - 1);
                                        }
                                        break;
                                }
                                #endregion Switch

#if(DEBUG && DEBUG_PLOT)
                            Debug.WriteLine("Plot 2: " + DateTime.Now.ToString("h:mm:ss.fff"));
#endif
                            }
                            #endregion
                        }
                    }
                }
            }
            catch/*(Exception ex)*/
            {
                //Debug.WriteLine("Excep _____ : " + ex.Message);
            }

        }
       
        private DoubleRange _yLimit;
        public DoubleRange YLimit
        {
            get { return _yLimit; }
            set
            {
                if(_yLimit == value)
                    return;
                _yLimit = value;
                OnPropertyChanged("YLimit");
            }
        }

        public ActionCommand RecordCommand
        {
            get { return new ActionCommand(Record); }
        }

        private string filePath;
        private bool RecFlag = false;
        string delimiter = ",";

        public void Record()
        {
            if(RecFlag == true)
            {
                string Date = Day(DateTime.Now.Day) + ' ' + MonthTrans(DateTime.Now.Month) + ' ' + DateTime.Now.Year.ToString();
                string path = "\\MotorController\\Charts\\" + Date + ' ' + DateTime.Now.ToString("HH:mm:ss");
                path = (path.Replace('-', ' ')).Replace(':', '_');
                path += ".csv";
                path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + path;
                string name = LeftPanelViewModel.name;
                filePath = path;// LeftPanelViewModel.Recordsrootpath + name + "_Record.csv";
                if(!File.Exists(filePath))
                {
                    string tempPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\MotorController\\Charts\\";
                    if(!Directory.Exists(tempPath))
                        Directory.CreateDirectory(tempPath);
                    File.Create(filePath);
                }
            }
            else
            {
                _selectedCh1DataSource = ((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(
                ((int)((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsGroup["ChannelsList"][0]).CommandId),
                ((int)((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsGroup["ChannelsList"][0]).CommandSubId))]).ChSelectedItem;
                _selectedCh2DataSource = ((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(
                ((int)((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsGroup["ChannelsList"][1]).CommandId),
                ((int)((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsGroup["ChannelsList"][1]).CommandSubId))]).ChSelectedItem;

                Task.Factory.StartNew(() =>
                {
                    RecFlag = false;
                    Thread.Sleep(100);
                    StringBuilder sb = new StringBuilder();

                    if(RecList.Count <= 1040000)
                    {
                        float[] yxls = RecList.ToArray();
                        float[] yxls2 = RecList2.ToArray();

                        string[] xstring = new string[RecList.Count + 1];

                        string[] ystring = new string[RecList.Count + 1];

                        if(_selectedCh1DataSource != "Pause")
                        {
                            xstring = new string[RecList.Count + 1];
                            ystring[0] = "Channel 1 - " + _selectedCh1DataSource + " - Gain: " + SubGain1.ToString();
                        }

                        string[] ystring2 = new string[RecList2.Count + 1];
                        if(_selectedCh2DataSource != "Pause")
                        {
                            xstring = new string[RecList2.Count + 1];
                            ystring2[0] = "Channel 2 - " + _selectedCh2DataSource + " - Gain: " + SubGain2.ToString();
                        }
                        xstring[0] = "Time";

                        if(_selectedCh1DataSource != "Pause" && _selectedCh2DataSource != "Pause")
                        {
                            for(int i = 1; i < RecList.Count; i++)
                            {
                                xstring[i] = (i * OscilloscopeParameters.Step).ToString(CultureInfo.CurrentCulture);
                                ystring[i] = yxls[i - 1].ToString(CultureInfo.CurrentCulture);
                                ystring2[i] = yxls2[i - 1].ToString(CultureInfo.CurrentCulture);
                                sb.AppendLine(string.Join(delimiter, xstring[i - 1], ystring[i - 1], ystring2[i - 1]));
                            }
                            RecList.Clear();
                            RecList2.Clear();
                        }
                        else if(_selectedCh1DataSource != "Pause")
                        {
                            for(int i = 1; i < RecList.Count; i++)
                            {
                                xstring[i] = (i * OscilloscopeParameters.Step).ToString(CultureInfo.CurrentCulture);
                                ystring[i] = yxls[i - 1].ToString(CultureInfo.CurrentCulture);
                                sb.AppendLine(string.Join(delimiter, xstring[i - 1], ystring[i - 1]));
                            }
                            RecList.Clear();
                        }
                        else if(_selectedCh2DataSource != "Pause")
                        {
                            for(int i = 1; i < RecList2.Count; i++)
                            {
                                xstring[i] = (i * OscilloscopeParameters.Step).ToString(CultureInfo.CurrentCulture);
                                ystring2[i] = yxls2[i - 1].ToString(CultureInfo.CurrentCulture);
                                sb.AppendLine(string.Join(delimiter, xstring[i - 1], ystring2[i - 1]));
                            }
                            RecList2.Clear();
                        }

                        File.AppendAllText(filePath, sb.ToString());
                        sb.Clear();
                    }
                    else
                    {
                        int count = 0;
                        for(int j = 0, index = 0; j < RecList.Count; j += 1040000, index++)
                        {
                            if((j + 1040000) < RecList.Count)
                                count = 1040000;
                            else
                                count = RecList.Count - j - 2;

                            List<float> yxls = RecList.GetRange(j, count);
                            List<float> yxls2 = RecList2.GetRange(j, count);

                            string[] xstring = new string[count + 1];

                            string[] ystring = new string[count + 1];
                            if(_selectedCh1DataSource != "Pause")
                            {
                                xstring = new string[count + 1];
                                ystring[0] = "Channel 1 - " + _selectedCh1DataSource + " - Gain: " + SubGain1.ToString();
                            }

                            string[] ystring2 = new string[count + 1];
                            if(_selectedCh2DataSource != "Pause")
                            {
                                xstring = new string[count + 1];
                                ystring2[0] = "Channel 2 - " + _selectedCh2DataSource + " - Gain: " + SubGain2.ToString();
                            }
                            xstring[0] = "Time";

                            if(_selectedCh1DataSource != "Pause" && _selectedCh2DataSource != "Pause")
                            {
                                for(int i = 1; i < count; i++)
                                {
                                    xstring[i] = (i * OscilloscopeParameters.Step).ToString(CultureInfo.CurrentCulture);
                                    ystring[i] = yxls[i - 1].ToString(CultureInfo.CurrentCulture);
                                    ystring2[i] = yxls2[i - 1].ToString(CultureInfo.CurrentCulture);
                                    sb.AppendLine(string.Join(delimiter, xstring[i - 1], ystring[i - 1], ystring2[i - 1]));
                                }
                            }
                            else if(_selectedCh1DataSource != "Pause")
                            {
                                for(int i = 1; i < count; i++)
                                {
                                    xstring[i] = (i * OscilloscopeParameters.Step).ToString(CultureInfo.CurrentCulture);
                                    ystring[i] = yxls[i - 1].ToString(CultureInfo.CurrentCulture);
                                    sb.AppendLine(string.Join(delimiter, xstring[i - 1], ystring[i - 1]));
                                }
                            }
                            else if(_selectedCh2DataSource != "Pause")
                            {
                                for(int i = 1; i < count; i++)
                                {
                                    xstring[i] = (i * OscilloscopeParameters.Step).ToString(CultureInfo.CurrentCulture);
                                    ystring2[i] = yxls2[i - 1].ToString(CultureInfo.CurrentCulture);
                                    sb.AppendLine(string.Join(delimiter, xstring[i - 1], ystring2[i - 1]));
                                }
                            }
                            filePath = filePath.Substring(0, filePath.Length - 4) + "_" + j.ToString() + ".csv";
                            File.AppendAllText(filePath, sb.ToString());
                            sb.Clear();
                        }
                    }
                });
            }

        }
        public static string MonthTrans(int month)
        {
            switch(month)
            {
                case 1:
                    return "January";
                case 2:
                    return "February";
                case 3:
                    return "March";
                case 4:
                    return "April";
                case 5:
                    return "May";
                case 6:
                    return "June";
                case 7:
                    return "July";
                case 8:
                    return "August";
                case 9:
                    return "Septembre";
                case 10:
                    return "Octobre";
                case 11:
                    return "Novembre";
                case 12:
                    return "Decembre";
                default:
                    return "x";
            }

        }
        public static string Day(int day)
        {
            if(day < 10)
            {
                return "0" + day.ToString();
            }
            else
                return day.ToString();

        }

        private bool _is_recording = false;
        private bool _is_freeze = false;
        public bool IsRecording
        {
            get { return _is_recording; }
            set
            {
                if(_is_recording == value)
                    return;
                if(OscilloscopeParameters.ChanTotalCounter > 0 && plotActivationstate == 1)
                {
                    _is_recording = value;
                    if(value)
                    {
                        RecFlag = true;
                        Record();
                    }
                    else
                    {
                        RecFlag = false;
                        Record();
                    }
                }
                else
                {
                    EventRiser.Instance.RiseEevent(string.Format($"No plot detected !"));
                    _is_recording = false;
                }
                OnPropertyChanged("IsRecording");
            }
        }

        public bool IsFreeze
        {
            get { return _is_freeze; }
            set
            {
                if(_is_freeze == value)
                { return; }
                if(value)
                    OnExampleExit();
                else
                    OnExampleEnter();
                _is_freeze = value;

                if(value && OscilloscopeParameters.ChanTotalCounter == 0 && plotActivationstate == 0)
                {
                    EventRiser.Instance.RiseEevent(string.Format($"No plot detected !"));
                    _is_freeze = false;
                }
                OnPropertyChanged("IsFreeze");
            }
        }

        private ObservableCollection<object> _channelsList;
        public ObservableCollection<object> ChannelsList
        {

            get
            {
                return Commands.GetInstance.GenericCommandsGroup["ChannelsList"];
            }
            set
            {
                _channelsList = value;
                OnPropertyChanged();
            }
        }
        public ICommand MouseWheelChart
        {
            get
            {
                return new RelayCommand(MouseWheelChartFunc);
            }
        }
        private void MouseWheelChartFunc(object sender)
        {

        }
    }
}