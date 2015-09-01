using NQuotes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpertAdvisor.TrendIndicator
{
    public class ParabolicSar
    {
        private MqlApi _mqlApi;
        private string _symbol;
        private int _timeFrame;
        private int _period;

        public ParabolicSar(MqlApi api, string symbol, int timeFrame, int period)
        {
            _mqlApi = api;
            _symbol = symbol;
            _timeFrame = timeFrame;
            _period = period;
        }

        public TrendDirection GetTrendDirection()
        {
            var sar = _mqlApi.iSAR(_symbol, _timeFrame, 0.02, 0.2, 0);
            var ask = _mqlApi.Ask;
            var bid = _mqlApi.Bid;

            if (sar < ask)
            {
                return TrendDirection.UP;
            }
            if (sar > ask)
            {
                return TrendDirection.DOWN;
            }

            return TrendDirection.NONE;
        }

        public OrderActionType GetOrderAction()
        {
            return OrderActionType.NONE;
        }
    }
}
