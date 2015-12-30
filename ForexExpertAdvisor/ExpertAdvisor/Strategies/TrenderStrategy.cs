using ExpertAdvisor.Entity;
using ExpertAdvisor.OrderManagement;
using NQuotes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpertAdvisor.Strategies
{
    public class TrenderStrategy
    {
        private MqlApi _mqlApi;
        private string _symbol;
        private TrendDirectionEnum _trenDirection;
        public TrenderStrategy(string symbol, MqlApi mqlApi, TrendDirectionEnum trendDirection)
        {
            _symbol = symbol;
            _mqlApi = mqlApi;
            _trenDirection = trendDirection;
        }

        public void Start()
        {
            TrenderOrderManager orderManager = new TrenderOrderManager(_mqlApi, _symbol);

            var volume = orderManager.GetAvailableVolume(_symbol);
            OpenOrderRequest openOrderRequest = new OpenOrderRequest
            {
                OrderType = _trenDirection == TrendDirectionEnum.UP ? OrderTypeEnum.BUY : OrderTypeEnum.SELL,
                Volume = volume,
                
                // Todo: Get from DB!
                Slippage = 3,
                StopLoss = 0,
                TakeProfit = 0,
                Comment = "Comment",
                Magic = 1000000,

                Expiration = DateTime.MinValue
            };
            OpenOrderResponse openOrderResponse = orderManager.OpenOrder(openOrderRequest);
        }

        public void Close()
        {
            TrenderOrderManager orderManager = new TrenderOrderManager(_mqlApi, _symbol);

            // Get list of orders
            List<Order> orders = orderManager.GetOrders(_symbol);

            // Close each order
            foreach (var order in orders)
            {
                CloseOrderRequest closeOrderRequest = new CloseOrderRequest
                {
                    Magic = 1000000, 
                    Slippage = 3
                };
                orderManager.CloseAllOrders(closeOrderRequest);
            }
        }

        public void Trail()
        {
            TrenderOrderManager orderManager = new TrenderOrderManager(_mqlApi, _symbol);

            // Get list of orders
            List<Order> orders = orderManager.GetOrders(_symbol);

            // Close each order
            foreach (var order in orders)
            {
                TrailOrderRequest trailOrderRequest = new TrailOrderRequest
                {

                };
                orderManager.TrailOrder(trailOrderRequest);
            }
        }
    }
}
