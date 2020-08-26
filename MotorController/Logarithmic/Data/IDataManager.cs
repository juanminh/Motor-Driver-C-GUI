using System;
using System.Collections.Generic;
using Logarithmic.Common;

namespace Logarithmic.Data
{
    public interface IDataManager
    {
        PriceSeries GetPriceData(string symbol, TimeFrame timeFrame);
        DoubleSeries GetFourierSeries(double amplitude, double phaseShift, int count = 5000);
        DoubleSeries GetSquirlyWave();

        IEnumerable<Instrument> AvailableInstruments { get; }
        IEnumerable<TimeFrame> GetAvailableTimeFrames(Instrument forInstrument);
    }
}