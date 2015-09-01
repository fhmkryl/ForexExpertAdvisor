using NQuotes;
using TicTacTec.TA.Library;

namespace ExpertAdvisor.TrendIndicator
{
    /// <summary>
    /// Todo: Double bollingers band will be added
    /// </summary>
    public class BollingerBands
    {
        private readonly MqlApi _mqlApi;
        private readonly string _symbol;
        private readonly int _timeFrame;
        private readonly int _period;
        private readonly int _trendDirectionCandleCount;
        private readonly int _marginPoints;

        private const int Deviation = 2;
        private const int BandsShift = 0;
        private const int AppliedPrice = 0;
        private const int Shift = 0;

        public BollingerBands(MqlApi api, string symbol, int timeFrame, int period, int trendDirectionCandleCount,
            int marginPoints)
        {
            _mqlApi = api;
            _symbol = symbol;
            _timeFrame = timeFrame;
            _period = period;
            _trendDirectionCandleCount = trendDirectionCandleCount;
            _marginPoints = marginPoints;
        }

        public TrendDirection GetTrendDirection()
        {
            var ask = _mqlApi.Ask;
            var bid = _mqlApi.Bid;

            var lower = _mqlApi.iBands(_symbol, MqlApi.PERIOD_M30, _period, Deviation, BandsShift, AppliedPrice,
                MqlApi.MODE_LOWER, Shift);
            var main = _mqlApi.iBands(_symbol, MqlApi.PERIOD_M30, _period, Deviation, BandsShift, AppliedPrice,
                MqlApi.MODE_MAIN, Shift);
            var upper = _mqlApi.iBands(_symbol, MqlApi.PERIOD_M30, _period, Deviation, BandsShift, AppliedPrice,
                MqlApi.MODE_UPPER, Shift);

            var upTrendCandleCount = 0;
            var downTrendCandleCount = 0;
            for (int i = 0; i < _trendDirectionCandleCount; i++)
            {
                var closePrice = _mqlApi.Close[i];
                var openPrice = _mqlApi.Open[i];
                if (closePrice > main && openPrice > main)
                {
                    upTrendCandleCount++;
                }
                if (closePrice < main && openPrice < main)
                {
                    downTrendCandleCount++;
                }
            }

            if (upTrendCandleCount > downTrendCandleCount)
            {
                return TrendDirection.UP;
            }
            if (downTrendCandleCount > upTrendCandleCount)
            {
                return TrendDirection.DOWN;
            }

            return TrendDirection.NONE;
        }

        public OrderAction GetOrderAction(TrendDirection trendDirection)
        {
            var ask = _mqlApi.Ask;
            var bid = _mqlApi.Bid;

            var lower = _mqlApi.iBands(_symbol, _timeFrame, _period, Deviation, BandsShift, AppliedPrice,
                MqlApi.MODE_LOWER, Shift);
            var main = _mqlApi.iBands(_symbol, _timeFrame, _period, Deviation, BandsShift, AppliedPrice,
                MqlApi.MODE_MAIN, Shift);
            var upper = _mqlApi.iBands(_symbol, _timeFrame, _period, Deviation, BandsShift, AppliedPrice,
                MqlApi.MODE_UPPER, Shift);

            var points = _mqlApi.MarketInfo(_symbol, MqlApi.MODE_POINT);

            // Todo: Margin should be added!
            switch (trendDirection)
            {
                case TrendDirection.UP:
                    // Case 1 Touches middle band
                    if (ask + _marginPoints * points <= main)
                    {
                        return new OrderAction
                        {
                            OrderActionType = OrderActionType.BUY
                        };
                    }

                    // Case 2 Touches lower band
                    if (ask + _marginPoints * points <= lower)
                    {
                        return new OrderAction
                        {
                            OrderActionType = OrderActionType.BUY
                        };
                    }
                    break;
                case TrendDirection.DOWN:
                    if (ask - _marginPoints * points >= main)
                    {
                        return new OrderAction
                        {
                            OrderActionType = OrderActionType.SELL
                        };
                    }

                    if (ask - _marginPoints * points >= upper)
                    {
                        return new OrderAction
                        {
                            OrderActionType = OrderActionType.SELL
                        };
                    }
                    break;
            }

            return new OrderAction
            {
                OrderActionType = OrderActionType.NONE
            };
        }
    }
}
