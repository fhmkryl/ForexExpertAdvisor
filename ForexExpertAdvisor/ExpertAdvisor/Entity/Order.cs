using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpertAdvisor.Entity
{
    public class Order
    {
        public int Ticket { get; set; }
        public double Volume { get; set; }
        public double Price { get; set; }
        public int Slippage { get; set; }
        public OrderTypeEnum OrderType { get; set; }
    }
}
