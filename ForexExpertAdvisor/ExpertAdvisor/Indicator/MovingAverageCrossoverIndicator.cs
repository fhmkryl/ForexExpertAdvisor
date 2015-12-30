using ExpertAdvisor.Entity;
using NQuotes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpertAdvisor.Indicator
{
    public class MovingAverageCrossoverIndicator
    {
        private MqlApi _mqlApi;
        private string _symbol;
        public MovingAverageCrossoverIndicator(MqlApi mqlApi, string symbol)
        {
            _mqlApi = mqlApi;
            _symbol = symbol;
        }
    }
}
