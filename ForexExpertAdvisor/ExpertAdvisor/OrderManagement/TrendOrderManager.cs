using ExpertAdvisor.Entity;
using NQuotes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpertAdvisor.OrderManagement
{
    public class TrenderOrderManager
    {
        private MqlApi _mqlApi;
        private string _symbol;
        public TrenderOrderManager(MqlApi mqlApi, string symbol)
        {
            _mqlApi = mqlApi;
            _symbol = symbol;
        }

        public OpenOrderResponse OpenOrder(OpenOrderRequest orderRequest)
        {
            var symbol = _symbol;
            var command = orderRequest.OrderType == OrderTypeEnum.BUY ? 0 : 1;
            var volume = orderRequest.Volume;
            var price = orderRequest.OrderType == OrderTypeEnum.BUY ? _mqlApi.Ask : _mqlApi.Bid;
            var slippage = orderRequest.Slippage;
            var stopLoss = orderRequest.StopLoss;
            var takeProfit = orderRequest.TakeProfit;
            var comment = orderRequest.Comment;
            var magic = orderRequest.Magic;
            var expiration = orderRequest.Expiration;

            try
            {
                _mqlApi.RefreshRates();

                var result = _mqlApi.OrderSend(symbol, command, volume, price, slippage, stopLoss, takeProfit, comment, magic, expiration);

                return new OpenOrderResponse
                {

                };
            }
            catch (Exception exception)
            {
                // Log exception
                throw;
            }
        }

        public CloseOrderResponse CloseAllOrders(CloseOrderRequest request)
        {
            try
            {
                int orderCount = _mqlApi.OrdersTotal();
                for (int i = orderCount - 1; i >= 0; i--)
                {
                    if (!_mqlApi.OrderSelect(i, MqlApi.SELECT_BY_POS, MqlApi.MODE_TRADES))
                        continue;
                    if (_mqlApi.OrderSymbol() != _symbol)
                        continue;
                    if (_mqlApi.OrderMagicNumber() != request.Magic)
                        continue;

                    int type = _mqlApi.OrderType();

                    // Close buys
                    if (type == 0)
                    {
                        _mqlApi.RefreshRates();
                        CloseOrder(_mqlApi.OrderTicket(), _mqlApi.OrderLots(), _mqlApi.MarketInfo(_symbol, MqlApi.MODE_ASK), request.Slippage);
                        continue;
                    }

                    // Close sells
                    if (type == 2)
                    {
                        _mqlApi.RefreshRates();
                        CloseOrder(_mqlApi.OrderTicket(), _mqlApi.OrderLots(), _mqlApi.MarketInfo(_symbol, MqlApi.MODE_BID), request.Slippage);
                        continue;
                    }
                }

                return new CloseOrderResponse();
            }
            catch (Exception exception)
            {
                // Log exception
                throw;
            }
        }

        private bool CloseOrder(int ticket, double lot, double price, int slippage)
        {
            try
            {
                return _mqlApi.OrderClose(ticket, lot, price, slippage);
            }
            catch (Exception exception)
            {
                // Log exception
                throw;
            }
        }

        public TrailOrderResponse TrailOrder(TrailOrderRequest request)
        {
            return new TrailOrderResponse();
        }

        public List<Order> GetOrders(string symbol)
        {
            int orderCount = _mqlApi.OrdersTotal();
            List<Order> orders = new List<Order>();
            for (int i = orderCount - 1; i >= 0; i--)
            {
                _mqlApi.RefreshRates();

                if (!_mqlApi.OrderSelect(i, MqlApi.SELECT_BY_POS, MqlApi.MODE_TRADES))
                    continue;
                if (_mqlApi.OrderSymbol() != symbol)
                    continue;

                int type = _mqlApi.OrderType();

                Order order = new Order
                {
                    Ticket = _mqlApi.OrderTicket(),
                    Volume = _mqlApi.OrderLots(),
                    Price = type == 0 ? _mqlApi.MarketInfo(symbol, MqlApi.MODE_ASK) : _mqlApi.MarketInfo(symbol, MqlApi.MODE_BID),
                    OrderType = type == 0 ? OrderTypeEnum.BUY : OrderTypeEnum.SELL
                };

                orders.Add(order);
            }
            return orders;
        }

        public double GetAvailableVolume(string symbol)
        {
            return 0.1;
        }
    }
}
