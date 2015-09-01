using System;
using System.Drawing;
using ExpertAdvisor.TrendIndicator;
using NQuotes;

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
            _mqlApi.Print(string.Format("Action : {0} , TakeProfit : {1} , StopLoss : {2}", orderAction.OrderActionType,
                orderAction.TakeProfit, orderAction.StopLoss));

            // Do order operations
            if (orderAction.OrderActionType != OrderActionType.NONE)
            {
                const double volume = 1.0;
                ExecuteOrder(symbol, volume, orderAction);
            }

            // Modify orders
            ModifyOrders(symbol);
        }

        public OrderAction GetOrderAction(string symbol)
        {
            // Check bollinger trend direction
            BollingerBands bollingersIndicator = new BollingerBands(_mqlApi, symbol, _timeFrame, _bollingersPeriod, _bollingersTrendDirectionCandleCount, _bollingersMarginPoints);
            TrendDirection bollingersTrendDirection = bollingersIndicator.GetTrendDirection(); 

            // Check candle sticks trend direction
            CandleStick candleStickIndicator = new CandleStick(_mqlApi, symbol, _timeFrame, _candleStickPeriod, _candleStickCandleCount);
            TrendDirection candleStickTrendDirection = candleStickIndicator.GetTrendDirection();

            // Todo: Choose one of them
            TrendDirection trendDirectionResult = TrendDirection.NONE;

            // Check bollinger order action
            OrderAction bollingersOrderAction = bollingersIndicator.GetOrderAction(trendDirectionResult);

            // Check candle stick order action
            OrderAction candleStickOrderAction = candleStickIndicator.GetOrderAction(trendDirectionResult);

            // Todo: Choose one of them

            return null;
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
