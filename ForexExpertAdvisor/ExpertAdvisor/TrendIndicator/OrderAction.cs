using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpertAdvisor.TrendIndicator
{
    public class OrderAction
    {
        public OrderActionType OrderActionType { get; set; }
        public double TakeProfit { get; set; }
        public double StopLoss { get; set; }
    }

    public enum OrderActionType
    {
        BUY,
        SELL,
        CLOSE_BUY,
        CLOSE_SELL,
        NONE
    }
}
