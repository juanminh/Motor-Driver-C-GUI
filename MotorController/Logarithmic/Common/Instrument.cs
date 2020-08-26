using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logarithmic.Common
{
    public class Instrument : StrongTyped<string>
    {
        public string InstrumentName { get; private set; }
        public string Symbol { get { return Value; } }
        public int DecimalPlaces { get; private set; }

        public Instrument(string value, string instrumentName, int decimalPlaces) : base(value)
        {
            InstrumentName = instrumentName;
            DecimalPlaces = decimalPlaces;
        }

        public static Instrument Parse(string instrumentString)
        {
            return new[] { EurUsd, Indu, Spx500, CrudeOil, Test }.Single(x => x.Symbol.ToUpper(CultureInfo.InvariantCulture) == instrumentString.ToUpper(CultureInfo.InvariantCulture));
        }

        public static readonly Instrument EurUsd = new Instrument("EURUSD", "FX Euro US Dollar", 4);
        public static readonly Instrument Indu = new Instrument("INDU", "Dow Jones Industrial Average", 0);
        public static readonly Instrument Spx500 = new Instrument("SPX500", "S&P500 Index", 0);
        public static readonly Instrument CrudeOil = new Instrument("CL", "Light Crude Oil", 0);
        public static readonly Instrument Test = new Instrument("TEST", "Test data only", 0);
    }
}