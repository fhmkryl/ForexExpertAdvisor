using NQuotes;

namespace ExpertAdvisor.TrendIndicator
{
    public class CandleStick
    {
        private readonly MqlApi _mqlApi;
        private readonly string _symbol;
        private readonly int _timeFrame;
        private readonly int _period;
        private readonly int _candleCount;

        public CandleStick(MqlApi api, string symbol, int timeFrame, int period, int candleCount)
        {
            _mqlApi = api;
            _symbol = symbol;
            _timeFrame = timeFrame;
            _period = period;
            _candleCount = candleCount;
        }

        public TrendDirection GetTrendDirection()
        {
            return TrendDirection.NONE;
        }

        public OrderAction GetOrderAction(TrendDirection trendDirection)
        {
            return null;
        }
    }
}
