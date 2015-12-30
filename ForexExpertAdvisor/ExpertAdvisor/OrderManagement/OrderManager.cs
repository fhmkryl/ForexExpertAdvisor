using NQuotes;
using System;
using System.Drawing;
using System.Linq;
using System.Text;
using ExpertAdvisor.Entity;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace ExpertAdvisor.OrderManagement
{
    public class OrderManager
    {
        private IndicatorResult _indicatorResult;
        private static DateTime _lastOrderOpenTime;
        public OrderManager(IndicatorResult indicatorResult)
        {
            _indicatorResult = indicatorResult;
        }

        public void DoOperation()
        {
            var mqlApi = _indicatorResult.MqlApi;
            var orderTotal = mqlApi.OrdersTotal();
            var openNewOrder = DateTime.Now.Subtract(_lastOrderOpenTime).Minutes > 1 || orderTotal == 0;
            switch (_indicatorResult.OrderType)
            {
                case OrderTypeEnum.BUY:
                    CloseAll();
                    if (openNewOrder)
                    {
                        OpenOrder();
                    }
                    break;
                case OrderTypeEnum.SELL:
                    CloseAll();
                    if (openNewOrder)
                    {
                        OpenOrder();
                    }
                    break;
                case OrderTypeEnum.CLOSEALL:
                    CloseAll();
                    break;
                default:
                    break;
            }
        }

        private int OpenOrder()
        {
            var mqlApi = _indicatorResult.MqlApi;
            if (_indicatorResult.ConcurrentOrderCount <= GetOpenOrderCount())
            {
                return -1;
            }

            var symbol = _indicatorResult.Symbol;
            var command = _indicatorResult.OrderType == OrderTypeEnum.BUY ? 0 : 1;
            var volume = _indicatorResult.Volume;
            var price = _indicatorResult.OrderType == OrderTypeEnum.BUY ? mqlApi.Ask : mqlApi.Bid;
            var slippage = _indicatorResult.Slippage;
            var stopLoss = GetStopLoss();
            var takeProfit = GetTakeProfit();
            var comment = _indicatorResult.Comment;
            var magic = _indicatorResult.Magic;
            var expiration = _indicatorResult.Expiration;

            try
            {
                var result = mqlApi.OrderSend(symbol, command, volume, price, slippage, stopLoss, takeProfit, comment, magic, expiration);
                if (result > 0)
                {
                    _lastOrderOpenTime = DateTime.Now;
                }

                return result;
            }
            catch (Exception exception)
            {
                // Log exception
                throw;
            }
        }

        private double GetTakeProfit()
        {
            var mqlApi = _indicatorResult.MqlApi;
            var price = _indicatorResult.OrderType == OrderTypeEnum.BUY
                ? mqlApi.Ask
                : mqlApi.Bid;
            var takeProfit = _indicatorResult.TakeProfit;

            if (_indicatorResult.OrderType == OrderTypeEnum.BUY)
            {
                return If(takeProfit > 0, price + takeProfit * mqlApi.Point * Fpc(), 0);
            }
            if (_indicatorResult.OrderType == OrderTypeEnum.SELL)
            {
                return If(takeProfit > 0, price - takeProfit * mqlApi.Point * Fpc(), 0);
            }

            return 0;
        }

        private double GetStopLoss()
        {
            var mqlApi = _indicatorResult.MqlApi;
            var price = _indicatorResult.OrderType == OrderTypeEnum.BUY 
                ? mqlApi.Ask
                : mqlApi.Bid;
            var stopLoss = _indicatorResult.StopLossSize;

            if (_indicatorResult.OrderType == OrderTypeEnum.BUY)
            {
                return If(stopLoss > 0, price - stopLoss * mqlApi.Point * Fpc(), 0);
            }
            if (_indicatorResult.OrderType == OrderTypeEnum.SELL)
            {
                return If(stopLoss > 0, price + stopLoss * mqlApi.Point * Fpc(), 0);
            }

            return 0;
        }

        private bool CloseOrder(int ticket, double lot, double price)
        {
            var mqlApi = _indicatorResult.MqlApi;
            var slippage = _indicatorResult.Slippage;
            var color = Color.Gold;
            try
            {
                return mqlApi.OrderClose(ticket, lot, price, slippage, color);
            }
            catch (Exception exception)
            {
                // Log exception
                throw;
            }
        }

        /// <summary>
        /// Todo: Close all
        /// </summary>
        public void CloseAll()
        {
            var mqlApi = _indicatorResult.MqlApi;
            var symbol = _indicatorResult.Symbol;
            var magic = _indicatorResult.Magic;
            int orderCount = mqlApi.OrdersTotal();
            for (int i = orderCount - 1; i >= 0; i--)
            {
                if (!mqlApi.OrderSelect(i, MqlApi.SELECT_BY_POS, MqlApi.MODE_TRADES))
                    continue;
                if (mqlApi.OrderSymbol() != symbol)
                    continue;
                if (mqlApi.OrderMagicNumber() != magic)
                    continue;

                int type = mqlApi.OrderType();

                // Close buys
                if (type == 0)
                {
                    mqlApi.RefreshRates();
                    CloseOrder(mqlApi.OrderTicket(), mqlApi.OrderLots(), mqlApi.MarketInfo(symbol, MqlApi.MODE_ASK));
                    continue;
                }

                // Close sells
                if (type == 2)
                {
                    mqlApi.RefreshRates();
                    CloseOrder(mqlApi.OrderTicket(), mqlApi.OrderLots(), mqlApi.MarketInfo(symbol, MqlApi.MODE_BID));
                    continue;
                }
            }
        }

        public void TrailOrders()
        {
            var mqlApi = _indicatorResult.MqlApi;
            var symbol = _indicatorResult.Symbol;
            var magic = _indicatorResult.Magic;
            var trailingStart = _indicatorResult.TrailingStart;
            var trailingSize = _indicatorResult.TrailingSize;
            var accountDigits = _indicatorResult.AccountDigits;
            Color color = Color.Silver;

            double stopLevel = mqlApi.MarketInfo(symbol, MqlApi.MODE_STOPLEVEL) + 1;
            double sl;

            int cnt = mqlApi.OrdersTotal();
            for (int i = 0; i < cnt; i++)
            {
                if (!mqlApi.OrderSelect(i, MqlApi.SELECT_BY_POS, MqlApi.MODE_TRADES)) continue;
                if (mqlApi.OrderSymbol() != symbol) continue;
                if (mqlApi.OrderMagicNumber() != magic) continue;

                int type = mqlApi.OrderType();
                if (type == MqlApi.OP_BUY)
                {
                    if (mqlApi.Bid - mqlApi.OrderOpenPrice() > trailingStart * mqlApi.Point * Fpc())
                    {
                        sl = mqlApi.Bid - trailingSize * mqlApi.Point * Fpc();

                        if (sl >= mqlApi.Bid - stopLevel * mqlApi.Point) continue;

                        if (mqlApi.OrderStopLoss() < sl - 1 * mqlApi.Point * Fpc())
                        {
                            mqlApi.OrderModify(mqlApi.OrderTicket(), mqlApi.OrderOpenPrice(), sl, mqlApi.OrderTakeProfit(), DateTime.MinValue, color);
                        }
                    }
                }

                if (type == MqlApi.OP_SELL)
                {
                    if (mqlApi.OrderOpenPrice() - mqlApi.Ask > trailingStart * mqlApi.Point * Fpc())
                    {
                        sl = mqlApi.Ask + trailingSize * mqlApi.Point * Fpc();

                        if (sl <= mqlApi.Ask + stopLevel * mqlApi.Point) continue;

                        if (mqlApi.OrderStopLoss() > sl + 1 * mqlApi.Point * Fpc() || mqlApi.OrderStopLoss() == 0)
                        {
                            mqlApi.OrderModify(mqlApi.OrderTicket(), mqlApi.OrderOpenPrice(), sl, mqlApi.OrderTakeProfit(), DateTime.MinValue, color);
                        }
                    }
                }
            }
        }

        private int GetOpenOrderCount()
        {
            var mqlApi = _indicatorResult.MqlApi;
            var symbol = _indicatorResult.Symbol;
            var magic = _indicatorResult.Magic;

            int orderCount = mqlApi.OrdersTotal();
            int openOrderCount = 0;
            for (int i = orderCount - 1; i >= 0; i--)
            {
                if (!mqlApi.OrderSelect(i, MqlApi.SELECT_BY_POS, MqlApi.MODE_TRADES))
                    continue;
                if (mqlApi.OrderSymbol() != symbol)
                    continue;
                openOrderCount++;
            }

            return openOrderCount;
        }

        private double Fpc()
        {
            if (_indicatorResult.AccountDigits == 5) return 10;
            if (_indicatorResult.AccountDigits == 6) return 100;

            return 1;
        }

        private double If(bool cond, double ifTrue, double ifFalse)
        {
            if (cond) 
                return ifTrue;

            return
                ifFalse;
        }

        public DateTime GetLastOrderOpenTime()
        {
            var mqlApi = _indicatorResult.MqlApi;
            var symbol = _indicatorResult.Symbol;
            var magic = _indicatorResult.Magic;

            int orderCount = mqlApi.OrdersTotal();
            List<DateTime> openTimes = new List<DateTime>();
            for (int i = orderCount - 1; i >= 0; i--)
            {
                if (!mqlApi.OrderSelect(i, MqlApi.SELECT_BY_POS, MqlApi.MODE_TRADES))
                    continue;
                if (mqlApi.OrderSymbol() != symbol)
                    continue;

                openTimes.Add(mqlApi.OrderOpenTime());
            }

            openTimes.Sort((a, b) => b.CompareTo(a));

            return openTimes.FirstOrDefault();
        }
    }
}
