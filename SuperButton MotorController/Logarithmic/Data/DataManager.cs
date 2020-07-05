using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Threading.Tasks;
using Logarithmic.Common;

namespace Logarithmic.Data
{
    public class DataManager : IDataManager
    {
        private readonly IDictionary<string, PriceSeries> _dataSets = new Dictionary<string, PriceSeries>();
        private readonly List<DoubleSeries> _acousticPlotData = new List<DoubleSeries>();
        private static readonly DataManager _instance = new DataManager();
        private IList<Instrument> _availableInstruments;
        private IDictionary<Instrument, IList<TimeFrame>> _availableTimeFrames;
        private Random _random = new Random();

        public static DataManager Instance
        {
            get { return _instance; }
        }

        private static string ResourceDirectory
        {
            get
            {
                return "Abt.Controls.SciChart.Example.Resources";
            }
        }

        public PriceSeries GetPriceData(string dataset, TimeFrame timeFrame)
        {
            return GetPriceData(string.Format("{0}_{1}", dataset, timeFrame));
        }

        public DoubleSeries GetFourierSeries(double amplitude, double phaseShift, int count = 5000)
        {
            var doubleSeries = new DoubleSeries();

            for(int i = 0; i < count; i++)
            {
                var xyPoint = new XYPoint();

                double time = 10 * i / (double)count;
                double wn = 2 * Math.PI / (count / 10);

                xyPoint.X = time;
                xyPoint.Y = Math.PI * amplitude *
                            (Math.Sin(i * wn + phaseShift) +
                             0.33 * Math.Sin(i * 3 * wn + phaseShift) +
                             0.20 * Math.Sin(i * 5 * wn + phaseShift) +
                             0.14 * Math.Sin(i * 7 * wn + phaseShift) +
                             0.11 * Math.Sin(i * 9 * wn + phaseShift) +
                             0.09 * Math.Sin(i * 11 * wn + phaseShift));
                doubleSeries.Add(xyPoint);
            }

            return doubleSeries;
        }

        public DoubleSeries GetSquirlyWave()
        {
            var doubleSeries = new DoubleSeries();
            var rand = new Random((int)DateTime.Now.Ticks);

            const int COUNT = 1000;
            for(int i = 0; i < COUNT; i++)
            {
                var xyPoint = new XYPoint();

                var time = i / (double)COUNT;
                xyPoint.X = time;
                xyPoint.Y = time * Math.Sin(2 * Math.PI * i / (double)COUNT) +
                             0.2 * Math.Sin(2 * Math.PI * i / (COUNT / 7.9)) +
                             0.05 * (rand.NextDouble() - 0.5) +
                             1.0;

                doubleSeries.Add(xyPoint);
            }

            return doubleSeries;
        }

        public IEnumerable<Instrument> AvailableInstruments
        {
            get
            {
                if(_availableInstruments == null)
                {
                    lock(typeof(DataManager))
                    {
                        if(_availableInstruments == null)
                        {
                            var assembly = typeof(DataManager).Assembly;
                            _availableInstruments = new List<Instrument>();

                            foreach(var resourceString in assembly.GetManifestResourceNames())
                            {
                                if(resourceString.Contains("_"))
                                {
                                    string instrumentString = GetSubstring(resourceString, ResourceDirectory + ".", "_");
                                    var instr = Instrument.Parse(instrumentString);
                                    if(!_availableInstruments.Contains(instr))
                                    {
                                        _availableInstruments.Add(instr);
                                    }
                                }
                            }
                        }
                    }
                }

                return _availableInstruments;
            }
        }

        private string GetSubstring(string input, string before, string after)
        {
            int beforeIndex = string.IsNullOrEmpty(before) ? 0 : input.IndexOf(before) + before.Length;
            int afterIndex = string.IsNullOrEmpty(after) ? input.Length : input.IndexOf(after) - beforeIndex;
            return input.Substring(beforeIndex, afterIndex);
        }

        public IEnumerable<TimeFrame> GetAvailableTimeFrames(Instrument forInstrument)
        {
            if(_availableTimeFrames == null)
            {
                lock(typeof(DataManager))
                {
                    if(_availableTimeFrames == null)
                    {
                        // Initialise the Timeframe dictionary
                        _availableTimeFrames = new Dictionary<Instrument, IList<TimeFrame>>();
                        foreach(var instr in AvailableInstruments)
                        {
                            _availableTimeFrames[instr] = new List<TimeFrame>();
                        }

                        var assembly = typeof(DataManager).Assembly;

                        foreach(var resourceString in assembly.GetManifestResourceNames())
                        {
                            if(resourceString.Contains("_"))
                            {
                                var instrument = Instrument.Parse(GetSubstring(resourceString, ResourceDirectory + ".", "_"));
                                var timeframe = TimeFrame.Parse(GetSubstring(resourceString, "_", ".csv"));

                                _availableTimeFrames[instrument].Add(timeframe);
                            }
                        }
                    }
                }
            }

            return _availableTimeFrames[forInstrument];
        }

        public PriceSeries GetPriceData(string dataset)
        {
            if(_dataSets.ContainsKey(dataset))
            {
                return _dataSets[dataset];
            }

            // e.g. resource format: Abt.Controls.SciChart.Example.Resources.EURUSD_Daily.csv 
            var csvResource = string.Format("{0}.{1}", ResourceDirectory, Path.ChangeExtension(dataset, "csv"));

            var priceSeries = new PriceSeries();
            priceSeries.Symbol = dataset;

            var assembly = typeof(DataManager).Assembly;
            // Debug.WriteLine(string.Join(", ", assembly.GetManifestResourceNames()));
            using(var stream = assembly.GetManifestResourceStream(csvResource))
            using(var streamReader = new StreamReader(stream))
            {
                string line = streamReader.ReadLine();
                while(line != null)
                {
                    var priceBar = new PriceBar();
                    // Line Format: 
                    // Date, Open, High, Low, Close, Volume 
                    // 2007.07.02 03:30, 1.35310, 1.35310, 1.35280, 1.35310, 12 
                    var tokens = line.Split(',');
                    priceBar.DateTime = DateTime.Parse(tokens[0], DateTimeFormatInfo.InvariantInfo);
                    priceBar.Open = double.Parse(tokens[1], NumberFormatInfo.InvariantInfo);
                    priceBar.High = double.Parse(tokens[2], NumberFormatInfo.InvariantInfo);
                    priceBar.Low = double.Parse(tokens[3], NumberFormatInfo.InvariantInfo);
                    priceBar.Close = double.Parse(tokens[4], NumberFormatInfo.InvariantInfo);
                    priceBar.Volume = long.Parse(tokens[5], NumberFormatInfo.InvariantInfo);
                    priceSeries.Add(priceBar);

                    line = streamReader.ReadLine();
                }
            }

            _dataSets.Add(dataset, priceSeries);

            return priceSeries;
        }

        public DoubleSeries GetExponentialCurve(double power, int pointCount)
        {
            var doubleSeries = new DoubleSeries(pointCount);

            double x = 0.00001;
            const double fudgeFactor = 1.4;
            for(int i = 0; i < pointCount; i++)
            {
                x *= fudgeFactor;
                double y = Math.Pow((double)i + 1, power);
                doubleSeries.Add(new XYPoint() { X = x, Y = y });
            }

            return doubleSeries;
        }
    }
}