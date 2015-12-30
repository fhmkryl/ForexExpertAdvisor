using System;
using System.Drawing;
using ExpertAdvisor.TrendIndicator;
using NQuotes;
using System.Collections.Generic;
using ExpertAdvisor.Entity;
using System.Linq;
using System.Text;
using System.IO;

namespace ExpertAdvisor.Advisor
{
    public class ScalperAdvisor : ExpertAdvisor
    {
        private readonly MqlApi _mqlApi;
        private readonly int _timeFrame;
        private readonly int _bollingersPeriod;
        private readonly int _candleStickPeriod;
        private readonly int _bollingersTrendDirectionCandleCount;
        private readonly int _candleStickCandleCount;
        private readonly int _bollingersMarginPoints;
        private readonly double _trailing;

        public ScalperAdvisor(MqlApi api)
        {
            _mqlApi = api;
            _timeFrame = MqlApi.PERIOD_M5;
            _bollingersPeriod = 20;
            _candleStickPeriod = 20;
            _bollingersTrendDirectionCandleCount = 3;
            _candleStickCandleCount = 3;
            _bollingersMarginPoints = 1;
            _trailing = 30;
        }

        public override void Execute()
        {
            var symbol = _mqlApi.Symbol();

            // Get order action
            OrderAction orderAction = GetOrderAction(symbol);
            if (orderAction != null)
            {
                _mqlApi.Print(string.Format("Action : {0} , TakeProfit : {1} , StopLoss : {2}", orderAction.OrderActionType,
                    orderAction.TakeProfit, orderAction.StopLoss));

                // Do order operations
                if (orderAction.OrderActionType != OrderActionType.NONE)
                {
                    const double volume = 0.1;
                    ExecuteOrder(symbol, volume, orderAction);
                }
            }

            // Modify orders
            ModifyOrders(symbol);
        }

        public OrderAction GetOrderAction(string symbol)
        {
            StringBuilder resultBuilder = new StringBuilder();
            var timeFrame = MqlApi.PERIOD_M1;

            var recentBars = GetRecentBars(symbol, timeFrame);
            var lastBar = recentBars.OrderByDescending(p => p.Time).First();
            var lastTickBar = LastTickBar.lastTickBars.OrderBy(p => p.Time).FirstOrDefault();
            
            // Yeni bar açıldı!
            // Todo: Diğer timeframeler için de burası değiştirilmeli
            if (lastTickBar != null)
            {
                if (lastBar.Time.Minute - lastTickBar.Time.Minute  == 5)
                {
                    _mqlApi.Print("Clearing last bars...");
                    LastTickBar.lastTickBars = new List<Bar>();
                }
            }
            LastTickBar.lastTickBars.Add(lastBar);

            if (LastTickBar.lastTickBars.Count > 3)
            {
                var currentTickbarIndex = LastTickBar.lastTickBars.Count - 1;
                var currentTickBar1 = LastTickBar.lastTickBars[currentTickbarIndex];

                var currentTickBarDirection1 = GetCurrentTickBarActionType(currentTickBar1, currentTickbarIndex, LastTickBar.lastTickBars);

                if (currentTickBarDirection1 == OrderActionType.BUY)
                {
                    return new OrderAction
                    {
                        OrderActionType = OrderActionType.BUY
                    };
                }
                if (currentTickBarDirection1 == OrderActionType.SELL)
                {
                    return new OrderAction
                    {
                        OrderActionType = OrderActionType.SELL
                    };
                }
            }
            
            return new OrderAction
            {
                OrderActionType = OrderActionType.NONE
            };
        }

        private OrderActionType GetCurrentTickBarActionType(Bar currentTickBar, int currentTickBarIndex, List<Bar> list)
        {
            if (currentTickBar.Open > list[currentTickBarIndex - 1].Open
                && currentTickBar.Open > list[currentTickBarIndex - 2].Open
                && currentTickBar.Open > list[currentTickBarIndex - 3].Open)
            {
                return OrderActionType.BUY;
            }
            if (currentTickBar.Open < list[currentTickBarIndex - 1].Open
                && currentTickBar.Open < list[currentTickBarIndex - 2].Open
                && currentTickBar.Open < list[currentTickBarIndex - 3].Open)
            {
                return OrderActionType.SELL;
            }

            return OrderActionType.NONE;
        }

        private List<Bar> GetRecentBars(string symbol,int timeFrame)
        {
            List<Bar> bars = new List<Bar>();
            for (int i = 1; i <= _mqlApi.iBars(symbol, timeFrame); i++)
            {
                var open = _mqlApi.iOpen(symbol, timeFrame, i);
                var high = _mqlApi.iHigh(symbol, timeFrame, i);
                var low = _mqlApi.iLow(symbol, timeFrame, i);
                var close = _mqlApi.iClose(symbol, timeFrame, i);
                var volume = _mqlApi.iVolume(symbol, timeFrame, i);
                var time = _mqlApi.iTime(symbol, timeFrame, i);

                var bar = new Bar
                {
                    Symbol = symbol,
                    Open = open,
                    High = high,
                    Low = low,
                    Close = close,
                    Volume = volume,
                    Time = time
                };

                bars.Add(bar);
            }

            return bars;
        }

        private void ExecuteOrder(string symbol, double volume, OrderAction orderAction)
        {
            var command = orderAction.OrderActionType == OrderActionType.BUY ? MqlApi.OP_BUY : MqlApi.OP_SELL;
            var price = command == 0 ? _mqlApi.Ask : _mqlApi.Bid;
            const int slippage = 3;
            const string comment = "Comment";
            const int magic = 20050610;
            var expiration = DateTime.MaxValue;
            var color = Color.Green;

            var ticket = _mqlApi.OrderSend(symbol, command, volume, price, slippage, orderAction.StopLoss,
                orderAction.TakeProfit, comment, magic, expiration, color);
            if (ticket < 0)
            {
                var error = _mqlApi.GetLastError();
                _mqlApi.Print("OrderSend failed with error #", error);
            }
            else
            {
                _mqlApi.Print(string.Format("{0} {1} lot {2} operation started", volume, symbol,
                    orderAction.OrderActionType));
            }
        }

        private void ModifyOrders(string symbol)
        {
            var numberOfOrders = _mqlApi.OrdersTotal();
            for (int i = 0; i < numberOfOrders; i++)
            {
                _mqlApi.OrderSelect(i, MqlApi.SELECT_BY_POS, MqlApi.MODE_TRADES);

                var orderType = _mqlApi.OrderType();
                var orderOpenPrice = _mqlApi.OrderOpenPrice();
                var orderTicket = _mqlApi.OrderTicket();
                var orderTakeProfit = _mqlApi.OrderTakeProfit();
                var orderStopLoss = _mqlApi.OrderStopLoss();
                var ask = _mqlApi.Ask;
                var bid = _mqlApi.Bid;
                var point = _mqlApi.Point;

                var minStopLevel = _mqlApi.MarketInfo(symbol, MqlApi.MODE_STOPLEVEL);
                if (orderType == MqlApi.OP_BUY)
                {
                    //if (bid - orderOpenPrice >= TakeProfit * Point || orderOpenPrice - ask > StopLoss * Point)
                    //{
                    //    OrderClose(orderTicket, OrderLots(), bid, 0, Color.Violet);
                    //}
                    if (_trailing > 0)
                    {
                        if (
                            bid > orderOpenPrice + point*_trailing + minStopLevel*point
                            && orderStopLoss < bid - point*_trailing - minStopLevel*point // Check previous stop loss
                            )
                        {
                            orderStopLoss = bid - point*_trailing - minStopLevel*point;
                            var result = _mqlApi.OrderModify(orderTicket, orderOpenPrice, orderStopLoss, orderTakeProfit,
                                DateTime.MaxValue, Color.Green);
                            if (!result)
                            {
                                _mqlApi.Print("OrderModify failed with error #", _mqlApi.GetLastError());
                            }
                        }
                    }
                }
                if (orderType == MqlApi.OP_SELL)
                {
                    //if (orderOpenPrice - ask >= TakeProfit * Point || bid - orderOpenPrice > StopLoss * Point)
                    //{
                    //    OrderClose(orderTicket, OrderLots(), ask, 0, Color.Violet);
                    //}
                    if (_trailing > 0)
                    {
                        if (
                            orderOpenPrice > ask + point*_trailing + minStopLevel*point
                            &&
                            (
                                orderStopLoss > ask + point*_trailing + minStopLevel*point
                                || orderStopLoss <= 0
                                ) // Check previous stop loss
                            )
                        {
                            orderStopLoss = ask + point*_trailing + minStopLevel*point;
                            var result = _mqlApi.OrderModify(orderTicket, orderOpenPrice, orderStopLoss, orderTakeProfit,
                                DateTime.MaxValue, Color.Red);
                            if (!result)
                            {
                                _mqlApi.Print("OrderModify failed with error #", _mqlApi.GetLastError());
                            }
                        }
                    }
                }
            }

        }
    }
}
