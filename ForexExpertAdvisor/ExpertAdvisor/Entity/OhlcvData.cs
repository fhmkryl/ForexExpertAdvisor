using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpertAdvisor.Entity
{
    public class OhlcvData : ITick
    {
        public long Id { get; set; }
        public string Symbol { get; set; }
        public string Period { get; set; }
        public DateTime Time { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }


        public double Bid
        {
            get { return Close; }
        }

        public double Ask
        {
            get { return Close; }
        }
    }
}
