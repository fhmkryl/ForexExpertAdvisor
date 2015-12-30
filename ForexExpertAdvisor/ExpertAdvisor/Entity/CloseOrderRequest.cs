using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpertAdvisor.Entity
{
    public class CloseOrderRequest
    {
        public int Magic { get; set; }
        public int Slippage { get; set; }
    }
}
