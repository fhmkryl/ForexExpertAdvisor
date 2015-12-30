using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpertAdvisor.Entity
{
    public class TickData : ITick
    {
        public long Id { get; set; }
        public string Symbol { get; set; }
        public DateTime Time { get; set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
    }

    public interface ITick
    {
        long Id { get; }
        string Symbol { get; }
        DateTime Time { get; }
        double Bid { get; }
        double Ask { get; }
    }
}
