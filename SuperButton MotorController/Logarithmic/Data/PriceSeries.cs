using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logarithmic.Data
{
    public class PriceSeries : List<PriceBar>
    {
        public string Symbol { get; set; }

        public PriceSeries()
        {
        }
    }
}