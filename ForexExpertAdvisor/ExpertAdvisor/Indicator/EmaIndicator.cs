using ExpertAdvisor.Entity;
using NQuotes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpertAdvisor.Indicator
{
    public class EmaIndicator
    {
        private MqlApi _mqlApi;
        private string _symbol;
        public EmaIndicator(MqlApi mqlApi, string symbol)
        {
            _mqlApi = mqlApi;
            _symbol = symbol;
        }

        int shortEmaPeriod = 5;
        int longEmaPeriod = 60;
        public IndicatorResult GetIndicatorResult()
        {
            double SEma, LEma;
            SEma = _mqlApi.iMA(_symbol, 0, shortEmaPeriod, 0, MqlApi.MODE_EMA, MqlApi.PRICE_CLOSE, 0);
            LEma = _mqlApi.iMA(_symbol, 0, longEmaPeriod, 0, MqlApi.MODE_EMA, MqlApi.PRICE_CLOSE, 0);

            int isCrossed = 0;
            isCrossed = Crossed(LEma, SEma);

            // Buy signal
            if (isCrossed == 1)
            {
                return new IndicatorResult
                {
                    TrendDirection = TrendDirectionEnum.UP,
                    OrderType = OrderTypeEnum.BUY
                };
            }

            // Sell signal
            if (isCrossed == 2)
            {
                return new IndicatorResult
                {
                    TrendDirection = TrendDirectionEnum.DOWN,
                    OrderType = OrderTypeEnum.SELL
                };
            }

            return new IndicatorResult
            {
                TrendDirection = TrendDirectionEnum.NONE,
                OrderType = OrderTypeEnum.NONE
            };
        }

        int Crossed(double line1, double line2)
        {
            int last_direction = 0;
            int current_direction = 0;
            //Don't work in the first load, wait for the first cross!
            //bool first_time = true;
            //if (first_time == true)
            //{
            //    first_time = false;
            //    return (0);
            //}
            //----
            if (line1 > line2)
                current_direction = 1;  //up
            if (line1 < line2)
                current_direction = 2;  //down
            //----
            if (current_direction != last_direction)  //changed 
            {
                last_direction = current_direction;
                return (last_direction);
            }
            else
            {
                return (0);  //not changed
            }
        }
    }
}
