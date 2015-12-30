using ExpertAdvisor.Entity;
using NQuotes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpertAdvisor.Indicator
{
    public class StochasticIndicator
    {
        private MqlApi _mqlApi;
        private string _symbol;
        public StochasticIndicator(MqlApi mqlApi, string symbol)
        {
            _mqlApi = mqlApi;
            _symbol = symbol;
        }

        // Parameterize these variables
        int StochKPeriod = 5;
        int StochDPeriod = 3;
        int StochsLowing = 3;
        int StochMethod = MqlApi.MODE_SMA;
        int StochPriceField = 0;
        int StochSignalBar = 1;
        double StochOverboughtLevel = 80.0;
        double StochOversoldLevel = 20.0;

        public IndicatorResult GetIndicatorResult()
        {
            double STO_M1 = _mqlApi.iStochastic(_symbol, 0, StochKPeriod, StochDPeriod, StochsLowing, StochMethod, StochPriceField, MqlApi.MODE_MAIN, StochSignalBar);
            double STO_M2 = _mqlApi.iStochastic(_symbol, 0, StochKPeriod, StochDPeriod, StochsLowing, StochMethod, StochPriceField, MqlApi.MODE_MAIN, StochSignalBar + 1);

            // Buy signal
            if (STO_M1 > StochOversoldLevel && STO_M2 <= StochOversoldLevel)
            {
                return new IndicatorResult
                {
                    TrendDirection = TrendDirectionEnum.UP,
                    OrderType = OrderTypeEnum.BUY
                };
            }

            // Sell signal
            if (STO_M1 < StochOverboughtLevel && STO_M2 >= StochOverboughtLevel)
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
