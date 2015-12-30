using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpertAdvisor.Entity
{
    public class Bar
    {
        public string Symbol { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
        public DateTime Time { get; set; }

        public override string ToString()
        {
            return string.Format("[Symbol:{0} Open:{1} High:{2} Low:{3} Close:{4} Volume:{5} Time:{6}]",
                Symbol, Open, High, Low, Close, Volume, Time);
        }
    }
}
