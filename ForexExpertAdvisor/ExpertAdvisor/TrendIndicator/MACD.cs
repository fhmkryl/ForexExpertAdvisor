using NQuotes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpertAdvisor.TrendIndicator
{
    public class MACD
    {
        private MqlApi _mqlApi;
        private string _symbol;
        private int _period;

        public MACD(MqlApi api, string symbol, int period)
        {
            _mqlApi = api;
            _symbol = symbol;
            _period = period;
        }

        public TrendDirection GetTrendDirection()
        {
            double MacdCurrent, MacdPrevious, SignalCurrent;
            double SignalPrevious, MaCurrent, MaPrevious;
            double MACDOpenLevel = 3;

            // initial data checks
            // it is important to make sure that the expert works with a normal
            // chart and the user did not make any mistakes setting external 
            // variables (Lots, StopLoss, TakeProfit, 
            // TrailingStop) in our case, we check TakeProfit
            // on a chart of less than 100 bars
            if (_mqlApi.Bars < 100)
            {
                _mqlApi.Print("bars less than 100");
                return (0);
            }


            // to simplify the coding and speed up access
            // data are put into internal variables
            MacdCurrent = _mqlApi.iMACD(_symbol, 0, 12, 26, 9, MqlApi.PRICE_CLOSE, MqlApi.MODE_MAIN, 0);
            MacdPrevious = _mqlApi.iMACD(_symbol, 0, 12, 26, 9, MqlApi.PRICE_CLOSE, MqlApi.MODE_MAIN, 1);
            SignalCurrent = _mqlApi.iMACD(_symbol, 0, 12, 26, 9, MqlApi.PRICE_CLOSE, MqlApi.MODE_SIGNAL, 0);
            SignalPrevious = _mqlApi.iMACD(_symbol, 0, 12, 26, 9, MqlApi.PRICE_CLOSE, MqlApi.MODE_SIGNAL, 1);
            MaCurrent = _mqlApi.iMA(_symbol, 0, _period, 0, MqlApi.MODE_EMA, MqlApi.PRICE_CLOSE, 0);
            MaPrevious = _mqlApi.iMA(_symbol, 0, _period, 0, MqlApi.MODE_EMA, MqlApi.PRICE_CLOSE, 1);

            // check for long position (BUY) possibility
            if (MacdCurrent < 0 && MacdCurrent > SignalCurrent && MacdPrevious < SignalPrevious &&
                _mqlApi.MathAbs(MacdCurrent) > (MACDOpenLevel * _mqlApi.Point) && MaCurrent > MaPrevious)
            {
                return TrendDirection.UP;
            }

            // check for short position (SELL) possibility
            if (MacdCurrent > 0 && MacdCurrent < SignalCurrent && MacdPrevious > SignalPrevious &&
                MacdCurrent > (MACDOpenLevel * _mqlApi.Point) && MaCurrent < MaPrevious)
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
