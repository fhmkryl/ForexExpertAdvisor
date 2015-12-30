using ExpertAdvisor.Entity;
using NQuotes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpertAdvisor.Indicator
{
    public class SarIndicator
    {
        private MqlApi _mqlApi;
        private string _symbol;
        public SarIndicator(MqlApi mqlApi, string symbol)
        {
            _mqlApi = mqlApi;
            _symbol = symbol;
        }

        public IndicatorResult GetIndicatorResult()
        {
            // Parameterize these variables
            var timeFrame = MqlApi.PERIOD_H1;
            double step = 0.02;
            double maximum = 0.2;
            int dot = 1;

            double sarValue = _mqlApi.iSAR(_symbol, timeFrame, step, maximum, dot);

            // Buy signal            
            if (sarValue < _mqlApi.Low[dot])
            {

                return new IndicatorResult
                {
                    TrendDirection = TrendDirectionEnum.UP,
                    OrderType = OrderTypeEnum.BUY
                };
            }

            // Sell signal
            if (sarValue > _mqlApi.High[dot])
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
    }
}
