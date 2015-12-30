using NQuotes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpertAdvisor.Strategies
{
    public class SarMacd
    {
        private MqlApi _mqlApi;
        private string _symbol;
        public SarMacd(MqlApi mqlApi, string symbol)
        {
            _mqlApi = mqlApi;
            _symbol = symbol;
        }

        int FastEMA = 12;
        int SlowEMA = 26;
        int SignalSMA = 9;
        double SarStep = 0.01;
        double SarMax = 0.1;
        //---- indicator buffers
        List<double> MacdBuffer = new List<double>();
        List<double> SignalBuffer = new List<double>();
        List<double> SarBuffer = new List<double>();

        int save_lastreverse;
        bool save_dirlong;
        double save_start;
        double save_last_high;
        double save_last_low;
        double save_ep;
        double save_sar;
        int bartime;
        public void Execute()
        {
            int limit;
            int i, counted_bars = _mqlApi.IndicatorCounted();
            bool first = true;
            bool dirlong;
            double start, last_high, last_low;
            double ep, sar, price_low, price_high, price;

            if (_mqlApi.Bars < 3) return;
            i = _mqlApi.Bars - 2;

            //---- last counted bar will be recounted
            if (counted_bars > 0) counted_bars--;
            limit = _mqlApi.Bars - counted_bars;
            //---- macd counted in the 1-st buffer
            for (i = 0; i < limit; i++)
                MacdBuffer.Add(_mqlApi.iMA(_symbol, 0, FastEMA, 0, MqlApi.MODE_EMA, MqlApi.PRICE_CLOSE, i) - _mqlApi.iMA(_symbol, 0, SlowEMA, 0, MqlApi.MODE_EMA, MqlApi.PRICE_CLOSE, i));
            //---- signal line counted in the 2-nd buffer
            for (i = 0; i < limit; i++)
                SignalBuffer.Add(_mqlApi.iMAOnArray(MacdBuffer.ToArray(), _mqlApi.Bars, SignalSMA, 0, MqlApi.MODE_SMA, i));
            //---- done
            if (counted_bars == 0 || first)
            {
                first = false;
                dirlong = true;
                start = SarStep;
                last_high = -10000000.0;
                last_low = 10000000.0;
                //----
                i=-2;
                while (i > 0)
                {
                    save_lastreverse = i;
                    price_low = SignalBuffer[i];
                    //----
                    if (last_low > price_low)
                        last_low = price_low;
                    price_high = SignalBuffer[i];
                    //----
                    if (last_high < price_high)
                        last_high = price_high;
                    //----
                    if (price_high > SignalBuffer[i + 1] && price_low > SignalBuffer[i + 1])
                        break;
                    //----
                    if (price_high < SignalBuffer[i + 1] && price_low < SignalBuffer[i + 1])
                    {
                        dirlong = false;
                        break;
                    }
                    i--;
                }
                //---- initial zero
                int k = 0;
                _mqlApi.Print("  k=", k, "  Bars=", _mqlApi.Bars);
                //----
                while (k < _mqlApi.Bars)
                {
                    SarBuffer[k] = 0.0;
                    k++;
                }
                //---- check further
                if (dirlong)
                {
                    SarBuffer[i] = SignalBuffer[i + 1];
                    ep = SignalBuffer[i];
                }
                else
                {
                    SarBuffer[i] = SignalBuffer[i + 1];
                    ep = SignalBuffer[i];
                }
                i--;
            }
            else
            {
                i = save_lastreverse;
                start = save_start;
                dirlong = save_dirlong;
                last_high = save_last_high;
                last_low = save_last_low;
                ep = save_ep;
                sar = save_sar;
                //if (_mqlApi.Time[0] != bartime)
                //{
                //    bartime = Time[0];
                //    i++;
                //}
            }
            while (i >= 0)
            {
                price_low = SignalBuffer[i];
                price_high = SignalBuffer[i];
                //--- check for reverse from long to short
                if (dirlong && price_low < SarBuffer[i + 1])
                {
                    //SaveLastReverse(i, true, start, price_low, last_high, ep, sar);
                    start = SarStep;
                    dirlong = false;
                    ep = price_low;
                    last_low = price_low;
                    SarBuffer[i] = last_high;
                    i--;
                    continue;
                }
                //--- check for reverse from short to long  
                if (!dirlong && price_high > SarBuffer[i + 1])
                {
                    //SaveLastReverse(i, false, start, last_low, price_high, ep, sar);
                    start = SarStep;
                    dirlong = true;
                    ep = price_high;
                    last_high = price_high;
                    SarBuffer[i] = last_low;
                    i--;
                    continue;
                }
                //sar(i) = sar(i+1)+start*(ep-sar(i+1))
                price = SarBuffer[i + 1];
                sar = price + start * (ep - price);
                //----
                if (dirlong)
                {
                    if (ep < price_high && (start + SarStep) <= SarMax)
                        start += SarStep;
                    //----
                    if (price_high < SignalBuffer[i + 1] && i == _mqlApi.Bars - 2)
                        sar = SarBuffer[i + 1];
                    price = SignalBuffer[i + 1];
                    //----
                    if (sar > price)
                        sar = price;
                    price = SignalBuffer[i + 2];
                    //----
                    if (sar > price)
                        sar = price;
                    //----
                    if (sar > price_low)
                    {
                        //SaveLastReverse(i, true, start, price_low, last_high, ep, sar);
                        start = SarStep;
                        dirlong = false;
                        ep = price_low;
                        last_low = price_low;
                        SarBuffer[i] = last_high;
                        i--;
                        continue;
                    }
                    //----
                    if (ep < price_high)
                    {
                        last_high = price_high;
                        ep = price_high;
                    }
                }     //dir-long
                else
                {
                    if (ep > price_low && (start + SarStep) <= SarMax)
                        start += SarStep;
                    //----
                    if (price_low < SignalBuffer[i + 1] && i == _mqlApi.Bars - 2)
                        sar = SarBuffer[i + 1];
                    price = SignalBuffer[i + 1];
                    //----
                    if (sar < price)
                        sar = price;
                    price = SignalBuffer[i + 2];
                    //----
                    if (sar < price)
                        sar = price;
                    //----
                    if (sar < price_high)
                    {
                        //SaveLastReverse(i, false, start, last_low, price_high, ep, sar);
                        start = SarStep;
                        dirlong = true;
                        ep = price_high;
                        last_high = price_high;
                        SarBuffer[i] = last_low;
                        i--;
                        continue;
                    }
                    //----
                    if (ep > price_low)
                    {
                        last_low = price_low;
                        ep = price_low;
                    }
                }     //dir-short
                SarBuffer[i] = sar;
                i--;
            }
        }
    }
}
