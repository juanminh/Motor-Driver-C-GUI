using Abt.Controls.SciChart;
using Abt.Controls.SciChart.Model.DataSeries;
using Abt.Controls.SciChart.Visuals.Axes;
using Logarithmic.Data;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Logarithmic.View
{
    public partial class LogarithmicAxisView : UserControl, INotifyPropertyChanged
    {
        private LogarithmicNumericAxis _xAxisLog;
        private LogarithmicNumericAxis _yAxisLog;
        private NumericAxis _xAxisNum;
        private NumericAxis _yAxisNum;

        private IAxis _xAxis;
        private IAxis _yAxis;

        private static readonly TimeSpan ts = TimeSpan.FromMilliseconds(500);

        public event PropertyChangedEventHandler PropertyChanged;

        public IAxis XAxis
        {
            get { return _xAxis; }
            set
            {
                _xAxis = value;
                OnPropertyChanged("XAxis");
            }
        }

        public IAxis YAxis
        {
            get { return _yAxis; }
            set
            {
                _yAxis = value;
                OnPropertyChanged("YAxis");
            }
        }

        public LogarithmicAxisView()
        {
            InitializeAxes();

            InitializeComponent();
        }

        private void InitializeAxes()
        {
            _xAxisLog = new LogarithmicNumericAxis
            {
                TextFormatting = "#.#E+0",
                ScientificNotation = ScientificNotation.LogarithmicBase,
                VisibleRange = new DoubleRange(0, 100),
                GrowBy = new DoubleRange(0.1, 0.1),
                DrawMajorBands = false,
                LogarithmicBase = 10
            };

            _xAxisNum = new NumericAxis
            {
                TextFormatting = "#.#E+0",
                ScientificNotation = ScientificNotation.Normalized,
                VisibleRange = new DoubleRange(0, 100),
                GrowBy = new DoubleRange(0.1, 0.1),
            };

            _yAxisLog = new LogarithmicNumericAxis
            {
                TextFormatting = "#.#E+0",
                ScientificNotation = ScientificNotation.LogarithmicBase,
                AxisAlignment = AxisAlignment.Left,
                GrowBy = new DoubleRange(0.1, 0.1),
                DrawMajorBands = false,
                LogarithmicBase = 10
            };

            _yAxisNum = new NumericAxis
            {
                TextFormatting = "#.#E+0",
                ScientificNotation = ScientificNotation.Normalized,
                AxisAlignment = AxisAlignment.Left,
                GrowBy = new DoubleRange(0.1, 0.1)
            };

            //var converter = new LogarithmicBaseConverter();
            //var logBinding = new Binding("SelectedValue") { ElementName = "logBasesChbx", Converter = converter };

            //_xAxisLog.SetBinding(LogarithmicNumericAxis.LogarithmicBaseProperty, logBinding);
            //_yAxisLog.SetBinding(LogarithmicNumericAxis.LogarithmicBaseProperty, logBinding);
        }

        private void OnLogarithmicAxisViewLoaded(object sender, RoutedEventArgs e)
        {
            // Create some DataSeries of type X=double, Y=double
            var dataSeries0 = new XyDataSeries<double, double>();
            var dataSeries1 = new XyDataSeries<double, double>();
            var dataSeries2 = new XyDataSeries<double, double>();

            var data1 = DataManager.Instance.GetExponentialCurve(1.8, 100);
            var data2 = DataManager.Instance.GetExponentialCurve(2.25, 100);
            var data3 = DataManager.Instance.GetExponentialCurve(3.59, 100);

            // Append data to series.
            dataSeries0.Append(data1.XData, data1.YData);
            dataSeries1.Append(data2.XData, data2.YData);
            dataSeries2.Append(data3.XData, data3.YData);

            // Attach DataSeries to RendetableSeries
            sciChart.RenderableSeries[0].DataSeries = dataSeries0;
            sciChart.RenderableSeries[1].DataSeries = dataSeries1;
            sciChart.RenderableSeries[2].DataSeries = dataSeries2;

            // Zoom to extents of the data
            sciChart.ZoomExtents();

            //btnZoom.IsChecked = true;

            // Workaround for Silverlight, to obtain the initial state of checkboxes
            OnAxisTypeChanged(null, null);
        }

        private void OnAxisTypeChanged(object sender, RoutedEventArgs e)
        {
            //var isYLog = yLogChbx != null && yLogChbx.IsChecked.HasValue && yLogChbx.IsChecked.Value;
            //var isXLog = xLogChbx != null && xLogChbx.IsChecked.HasValue && xLogChbx.IsChecked.Value;

            YAxis = _yAxisLog;//isYLog ? _yAxisLog : _yAxisNum;
            XAxis = _xAxisLog;//isXLog ? _xAxisLog : _xAxisNum;

            if(sciChart != null)
            {
                sciChart.AnimateZoomExtents(ts);
            }
        }

        private void ZoomExtentsClick(object sender, RoutedEventArgs e)
        {
            if(sciChart != null)
            { sciChart.AnimateZoomExtents(ts); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if(handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    //public class LogarithmicBaseConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        var str = (string)value;

    //        var result = str.ToUpperInvariant().Equals("E") ? Math.E : Double.Parse(str, CultureInfo.InvariantCulture);

    //        return result;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

}
