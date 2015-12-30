using ExpertAdvisor.Entity;
using NQuotes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpertAdvisor.CandleStick
{
    public class EveningStar
    {
        private MqlApi _mqlApi;
        private string _symbol;
        public EveningStar(MqlApi mqlApi, string symbol)
        {
            _mqlApi = mqlApi;
            _symbol = symbol;
        }

        public OrderType GetOrderType(OhlcvData[] BarList)
        {
            var patternExists = PatternExists(BarList);
            if (patternExists)
            {
                return OrderType.Sell;
            }

            return OrderType.NoOrder;
        }

        private bool PatternExists(OhlcvData[] barList)
        {
            var bar1 = barList[1];
            var bar2 = barList[2];
            var bar3 = barList[3];
            var candleStickHelper = new CandleStickHelper(_mqlApi, _symbol);

            if (bar3.Close - bar3.Open > candleStickHelper.AverageBody(1)
                && Math.Abs(bar2.Close - bar2.Open) < candleStickHelper.AverageBody(1) * 0.5
                    && bar2.Close > bar3.Close
                    && bar2.Open > bar3.Open
                    && bar1.Close < candleStickHelper.MidOpenClose(3)
                )
            {
                return true;
            }

            //if (bar3.Close - bar3.Open > candleStickHelper.AverageBody(1)
            //    && candleStickHelper.AverageBody(2) < candleStickHelper.AverageBody(1) * 0.1
            //        && bar2.Close > bar3.Close
            //        && bar2.Open > bar3.Open
            //        && bar1.Open < bar2.Close
            //        && bar1.Close < bar2.Close
            //    )
            //{
            //    return true;
            //}

            return false;
        }
    }
}
