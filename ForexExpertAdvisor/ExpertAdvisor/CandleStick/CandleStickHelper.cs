using NQuotes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpertAdvisor.CandleStick
{
    public class CandleStickHelper
    {
        private MqlApi _mqlApi;
        private string _symbol;
        public CandleStickHelper(MqlApi mqlApi, string symbol)
        {
            _mqlApi = mqlApi;
            _symbol = symbol;
        }

        public double AverageBody(int index)
        {
            int maPeriod = 12;
            double candleBody = 0;

            ///--- calculate the averaged size of the candle's body
            for (int i = index; i < index + maPeriod; i++)
            {
                candleBody += Math.Abs(_mqlApi.iOpen(_symbol, 0, i) - _mqlApi.iClose(_symbol, 0, i));
            }
            candleBody = candleBody / maPeriod;

            ///--- return body size
            return candleBody;
        }

        public double MidOpenClose(int index)
        {
            return 0.5 * (_mqlApi.iOpen(_symbol, 0, index) + _mqlApi.iClose(_symbol, 0, index));
        }
    }
}
