using NQuotes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpertAdvisor.Entity
{
    public class OpenOrderRequest
    {
        public OrderTypeEnum OrderType { get; set; }
        public double Volume { get; set; }
        public int Slippage { get; set; }
        public double TakeProfit { get; set; }
        public int StopLoss { get; set; }
        public int AccountDigits { get; set; }
        public string Comment { get; set; }
        public int Magic { get; set; }
        public DateTime Expiration { get; set; }
    }
}
