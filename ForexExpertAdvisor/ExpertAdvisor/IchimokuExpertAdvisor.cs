using ExpertIndicator;
using NQuotes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpertAdvisor
{
    public class IchimokuExpertAdvisor : MqlApi
    {
        public override int start()
        {

            Execute();

            return 0;
        }

        public void Execute()
        {
            BollingerBandsEa();
        }

        private void BollingerBandsEa()
        {
            var symbol = Symbol();
            var timeFrame = PERIOD_M1;
            var period = 20;
            var deviation = 2;
            var bandsShift = 0;
            var appliedPrice = 0;
            var shift = 0;
            var point = MarketInfo(symbol, MODE_POINT);
            var digits = MarketInfo(Symbol(), MODE_DIGITS);
            var ask = Ask;
            var bid = Bid;
            var macdTrend = GetTrendDirectionWithMacd(symbol, 26);

            var lower = iBands(symbol, timeFrame, period, deviation, bandsShift, appliedPrice, MODE_LOWER, shift);
            var main = iBands(symbol, timeFrame, period, deviation, bandsShift, appliedPrice, MODE_MAIN, shift);
            var upper = iBands(symbol, timeFrame, period, deviation, bandsShift, appliedPrice, MODE_UPPER, shift);
            
            if (macdTrend == TrendMode.UP)
            {
                if (ask == main)
                {
                    StartOperation(symbol, OP_BUY, 1, lower, upper);
                }
            }
            else if (macdTrend == TrendMode.DOWN)
            {
                if (ask == main)
                {
                    StartOperation(symbol, OP_SELL, 1, upper, lower);
                }
            }
            else
            {
                Print("There is no trend...");
            }
        }

        private TrendMode GetTrendDirectionWithMacd(string symbol, int trendPeriod)
        {
            double MacdCurrent, MacdPrevious, SignalCurrent;
            double SignalPrevious, MaCurrent, MaPrevious;
            int total;
            double MACDOpenLevel=3;

            // initial data checks
            // it is important to make sure that the expert works with a normal
            // chart and the user did not make any mistakes setting external 
            // variables (Lots, StopLoss, TakeProfit, 
            // TrailingStop) in our case, we check TakeProfit
            // on a chart of less than 100 bars
            if (Bars < 100)
            {
                Print("bars less than 100");
                return (0);
            }
            

            // to simplify the coding and speed up access
            // data are put into internal variables
            MacdCurrent = iMACD(symbol, 0, 12, 26, 9, PRICE_CLOSE, MODE_MAIN, 0);
            MacdPrevious = iMACD(symbol, 0, 12, 26, 9, PRICE_CLOSE, MODE_MAIN, 1);
            SignalCurrent = iMACD(symbol, 0, 12, 26, 9, PRICE_CLOSE, MODE_SIGNAL, 0);
            SignalPrevious = iMACD(symbol, 0, 12, 26, 9, PRICE_CLOSE, MODE_SIGNAL, 1);
            MaCurrent = iMA(symbol, 0, trendPeriod, 0, MODE_EMA, PRICE_CLOSE, 0);
            MaPrevious = iMA(symbol, 0, trendPeriod, 0, MODE_EMA, PRICE_CLOSE, 1);
            total = OrdersTotal();
            
            // check for long position (BUY) possibility
            if (MacdCurrent < 0 && MacdCurrent > SignalCurrent && MacdPrevious < SignalPrevious &&
                MathAbs(MacdCurrent) > (MACDOpenLevel * Point) && MaCurrent > MaPrevious)
            {
                return TrendMode.UP;
            }

            // check for short position (SELL) possibility
            if (MacdCurrent > 0 && MacdCurrent < SignalCurrent && MacdPrevious > SignalPrevious &&
                MacdCurrent > (MACDOpenLevel * Point) && MaCurrent < MaPrevious)
            {
                return TrendMode.DOWN;
            }

            return TrendMode.NONE;
        }

        int CloseAllOrders(int orderType)
        {
            int numOfOrders = OrdersTotal();
            int FirstOrderType = 0;

            for (int index = 0; index < OrdersTotal(); index++)
            {
                OrderSelect(index, SELECT_BY_POS, MODE_TRADES);
                if (OrderSymbol() == Symbol())
                {
                    FirstOrderType = OrderType();
                    break;
                }
            }

            for (var index = numOfOrders - 1; index >= 0; index--)
            {
                OrderSelect(index, SELECT_BY_POS, MODE_TRADES);

                if (OrderSymbol() == Symbol())
                    switch (OrderType())
                    {
                        case OP_BUY:
                            if (orderType == OP_BUY)
                            {
                                OrderClose(OrderTicket(), OrderLots(), MarketInfo(OrderSymbol(), MODE_BID), 0);
                            }
                            break;
                        case OP_SELL:
                            if (orderType == OP_SELL)
                            {
                                OrderClose(OrderTicket(), OrderLots(), MarketInfo(OrderSymbol(), MODE_ASK), 0);
                            }
                            break;
                    }
            }

            return 1;
        }

        private void TrenderEaWithIchimoku()
        {
            var symbol = Symbol();
            var period = PERIOD_D1;

            // Get ichimoku signal to decide trade
            Ichimoku ichimokuSignal = GetIchimokuSignal(symbol, period);

            // Start trender based on ichimoku signal
            StartTrender(symbol, ichimokuSignal);
        }

        private void StartTrender(string symbol, Ichimoku ichimokuSignal)
        {
            var volume = 0.01;
            if (ichimokuSignal.OpenBuy)
            {
                // Open buy operation
                //StartOperation(symbol, OP_BUY, volume);
            }
            else if (ichimokuSignal.OpenSell)
            {
                // Open sell operation
                //StartOperation(symbol, OP_SELL, volume);
            }
            else if (ichimokuSignal.CloseBuy)
            {
                // Close buy operations
            }
            else if (ichimokuSignal.CloseSell)
            {
                // Close sell operations
            }
            else
            {
                Print("Advisor waiting for IchimokuSignal");
            }
        }

        private void StartOperation(string symbol, int command, double volume, double stopLoss, double takeProfit)
        {
            //var balance = AccountBalance();
            //var equity = AccountEquity();
            //var freeMargin = AccountFreeMargin();
            //var margin = AccountMargin();
            //var marginLevelPercentage = 100 * (equity / margin);

            RefreshRates();

            var price = command == 0 ? Ask : Bid;
            var slippage = 3;
            double minstoplevel = MarketInfo(Symbol(), MODE_STOPLEVEL);
            var comment = "Comment";
            var magic = 20050610;
            var expiration = DateTime.MaxValue;
            var color = Color.Green;

            var ticket = OrderSend(symbol, command, volume, price, slippage, stopLoss, takeProfit, comment, magic, expiration, color);
            if (ticket < 0)
            {
                var error = GetLastError();
                Print("OrderSend failed with error #", GetLastError());
            }
            else
            {
                Print(string.Format("{0} {1} lot {2} operation started", volume, symbol, command == 0 ? "BUY" : "SELL"));
            }
        }

        private void CloseOperation()
        {

        }

        public Ichimoku GetIchimokuSignal(string symbol, int period)
        {
            double T_0;           // Value of Tenkan-sen in bar 0
            double T_1;           // Value of Tenkan-sen in bar 1
            double K_0;           // Value of Kijun-sen in bar 0
            double K_1;           // Value of Kijun-sen in bar 1

            T_0 = iIchimoku(symbol, period, 9, 26, 52, MODE_TENKANSEN, 0);
            T_1 = iIchimoku(symbol, period, 9, 26, 52, MODE_TENKANSEN, 1);
            K_0 = iIchimoku(symbol, period, 9, 26, 52, MODE_KIJUNSEN, 0);
            K_1 = iIchimoku(symbol, period, 9, 26, 52, MODE_KIJUNSEN, 1);

            return new Ichimoku
            {
                TenkansenBar_0 = T_0,
                TenkansenBar_1 = T_1,
                KijunsenBar_0 = K_0,
                KijunsenBar_1 = K_1
            };
        }
    }

    public enum BollingerMode
    {
        UPPER,
        MAIN,
        LOWER
    }

    public enum TrendMode
    {
        UP,
        DOWN,
        NONE
    }
}
