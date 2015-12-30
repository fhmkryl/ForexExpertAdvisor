using NQuotes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpertAdvisor.Entity
{
    public class IndicatorResult
    {
        public TrendDirectionEnum TrendDirection { get; set; }
        public OrderTypeEnum OrderType { get; set; }

        public MqlApi MqlApi { get; set; }
        public string Symbol { get; set; }
        public double Volume { get; set; }
        public int Slippage { get; set; }
        public double TakeProfit { get; set; }
        public int StopLossSize { get; set; }
        public double TrailingStart { get; set; }
        public double TrailingSize { get; set; }
        public int AccountDigits { get; set; }
        public string Comment { get; set; }
        public int Magic { get; set; }
        public DateTime Expiration { get; set; }

        public int ConcurrentOrderCount { get; set; }
    }
}
