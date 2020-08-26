using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logarithmic.Data
{
    public class DoubleSeries : List<XYPoint>
    {
        public DoubleSeries()
        {
        }

        public DoubleSeries(int capacity) : base(capacity)
        {
        }

        public IList<double> XData { get { return this.Select(x => x.X).ToArray(); } }
        public IList<double> YData { get { return this.Select(x => x.Y).ToArray(); } }
    }
}