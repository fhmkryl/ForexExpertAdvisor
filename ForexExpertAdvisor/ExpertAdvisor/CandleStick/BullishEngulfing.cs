using ExpertAdvisor.Entity;
using NQuotes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacTec.TA.Library;

namespace ExpertAdvisor.CandleStick
{
    public class BullishEngulfing
    {
        private MqlApi _mqlApi;
        private string _symbol;
        public BullishEngulfing(MqlApi mqlApi, string symbol)
        {
            _mqlApi = mqlApi;
            _symbol = symbol;
        }

        public OrderType GetOrderType(OhlcvData[] BarList)
        {
            var patternExists = PatternExists(BarList);
            if (patternExists)
            {
                return OrderType.Buy;
            }

            return OrderType.NoOrder;
        }

        private bool PatternExists(OhlcvData[] barList)
        {
            var lastBar = barList[1];
            var previousBar = barList[2];
            
            if (lastBar == null || previousBar == null)
            {
                return false;
            }

            if (lastBar.Open < lastBar.Close
                 && previousBar.Open > previousBar.Close
                 && lastBar.Open < previousBar.Close
                 && lastBar.Close > previousBar.Open 
                 && lastBar.Low < previousBar.Low
                 && lastBar.High > previousBar.High
                )
            {
                return true;
            }

            return false;
        }
    }
}
