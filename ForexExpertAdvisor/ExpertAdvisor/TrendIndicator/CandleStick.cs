using NQuotes;
using System;
using System.Drawing;
using System.Text;
using TicTacTec.TA.Library;

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

        /**
     * The total number of periods to generate data for.
     */
        public const int TOTAL_PERIODS = 100;

        /**
         * The number of periods to average together.
         */
        public const int PERIODS_AVERAGE = 30;
        public TrendDirection GetTrendDirection()
        {
            try
            {
                var arraySize = 5;
                var startIdx = 0;
                var endIndex = TOTAL_PERIODS - 1;
                double[] inOpen = new double[TOTAL_PERIODS];
                double[] inHigh = new double[TOTAL_PERIODS];
                double[] inLow = new double[TOTAL_PERIODS];
                double[] inClose = new double[TOTAL_PERIODS];
                double optInPenetration = -4 * Math.Pow(10, 37);
                int outBegIndex;
                int outNBElement;
                int[] outInteger = new int[TOTAL_PERIODS];

                for (int i = 0; i < inClose.Length; i++)
                {
                    inOpen[i] = _mqlApi.Open[i];
                    inHigh[i] = _mqlApi.High[i];
                    inLow[i] = _mqlApi.Low[i];
                    inClose[i] = _mqlApi.Close[i];
                }

                //Core.RetCode returnCode = Core.CdlMorningDojiStar(startIdx, endIndex, inOpen, inHigh, inLow, inClose, optInPenetration, out outBegIndex, out outNBElement, outInteger);
                Core.RetCode returnCode = Core.CdlDoji(startIdx, endIndex, inOpen, inHigh, inLow, inClose, out outBegIndex, out outNBElement, outInteger);
                for (int i = 0; i < inClose.Length; i++)
                {
                    if (outInteger[i] > 0)
                    {
                        var chartId = _mqlApi.ChartID();
                        _mqlApi.ObjectSetString(chartId, "label_object", MqlApi.OBJ_TEXT, MqlApi.OBJPROP_TEXT, string.Format("Simple Label {0}", i));
                    }
                }
                
            }
            catch (Exception exception)
            {
                throw;
            }

            return TrendDirection.NONE;
        }

        public OrderAction GetOrderAction(TrendDirection trendDirection)
        {
            return null;
        }

        private int PaintBar(int shift)
        {
            int bull = 1;
            int bear = -1;
            int pig = 0;
            double percentage = 33.33;

            double open = _mqlApi.iOpen(_symbol, 0, shift);
            double close = _mqlApi.iClose(_symbol, 0, shift);
            double high = _mqlApi.iHigh(_symbol, 0, shift);
            double low = _mqlApi.iLow(_symbol, 0, shift);

            double range = high - low;
            if (range == 0) return (pig);

            double OLrange = (open - low) * 100 / range;
            double OHrange = (high - open) * 100 / range;
            double CLrange = (close - low) * 100 / range;
            double CHrange = (high - close) * 100 / range;

            if (OLrange < percentage && CHrange < percentage) return (bull);
            if (OHrange < percentage && CLrange < percentage) return (bear);

            return (pig);
        }
    }
}
