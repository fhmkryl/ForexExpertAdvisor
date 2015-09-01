using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpertIndicator
{
    public class Ichimoku
    {
        public double TenkansenBar_0 { get; set; }
        public double TenkansenBar_1 { get; set; }
        public double KijunsenBar_0 { get; set; }
        public double KijunsenBar_1 { get; set; }

        public bool OpenBuy
        {
            get
            {
                return TenkansenBar_1 < KijunsenBar_1 && TenkansenBar_0 >= KijunsenBar_0;
            }
        }
        
        public bool OpenSell
        {
            get
            {
                return TenkansenBar_1 > KijunsenBar_1 && TenkansenBar_0 <= KijunsenBar_0;
            }
        }

        public bool CloseBuy
        {
            get
            {
                return TenkansenBar_1 > KijunsenBar_1 && TenkansenBar_0 <= KijunsenBar_0;
            }
        }

        public bool CloseSell
        {
            get
            {
                return TenkansenBar_1 < KijunsenBar_1 && TenkansenBar_0 >= KijunsenBar_0;
            }
        }
    }
}
