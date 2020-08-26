using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logarithmic.Common
{
    public class TimeFrame : StrongTyped<string>
    {
        public TimeFrame(string value, string displayname) : base(value)
        {
            Displayname = displayname;
        }

        public static readonly TimeFrame Daily = new TimeFrame("Daily", "Daily");
        public static readonly TimeFrame Hourly = new TimeFrame("Hourly", "1 Hour");
        public static readonly TimeFrame Minute15 = new TimeFrame("Minute15", "15 Minutes");

        public string Displayname { get; private set; }

        public static TimeFrame Parse(string input)
        {
            return new[] { Minute15, Hourly, Daily }.Single(x => x.Value.ToUpper(CultureInfo.InvariantCulture) == input.ToUpper(CultureInfo.InvariantCulture));
        }
    }
}