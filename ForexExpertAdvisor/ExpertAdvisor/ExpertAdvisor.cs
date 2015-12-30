using ExpertAdvisor.CandleStick;
using ExpertAdvisor.Entity;
using ExpertAdvisor.Indicator;
using ExpertAdvisor.OrderManagement;
using ExpertAdvisor.Strategies;
using NQuotes;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ExpertAdvisor
{
    public class ExpertAdvisor : MqlApi
    {
        OhlcvData[] BarList = new OhlcvData[15];
        public override int start()
        {
            StartInternal();

            return 1;
        }

        private void StartInternal()
        {
            var symbol = Symbol();

            // Add new bar to list
            PopulateBarList(symbol);

            // Apply candle stick
            BullishEngulfing bullishEngulfing = new BullishEngulfing(this, symbol);
            BearishEngulfing bearishEngulfing = new BearishEngulfing(this, symbol);

            // Get Order Type
            OrderType orderTypeBullishEngulf = bullishEngulfing.GetOrderType(BarList);
            if (orderTypeBullishEngulf != Entity.OrderType.NoOrder)
            {
                Print(string.Format("OrderType is {0}", orderTypeBullishEngulf));
                OrderSend(symbol, 0, 0.1, Ask, 1, 0, 0, "Comment", 1111, DateTime.MinValue, Color.Green);
            }

            OrderType orderTypeBearishEngulf = bearishEngulfing.GetOrderType(BarList);
            if (orderTypeBearishEngulf != Entity.OrderType.NoOrder)
            {
                Print(string.Format("OrderType is {0}", orderTypeBearishEngulf));
                OrderSend(symbol, 0, 0.1, Ask, 1, 0, 0, "Comment", 1111, DateTime.MinValue, Color.Red);
            }
        }

        private void PopulateBarList(string symbol)
        {
            for (int i = 0; i < BarList.Length; i++)
            {
                var open = iOpen(symbol, 0, i);
                var high = iHigh(symbol, 0, i);
                var low = iLow(symbol, 0, i);
                var close = iClose(symbol, 0, i);

                BarList[i] = new OhlcvData
                {
                    Open = open,
                    High = high,
                    Low = low,
                    Close = close
                };
            }
        }

        private OhlcvData GetNewBar(string symbol)
        {
            var Open = iOpen(symbol, 0, 1);
            var High = iHigh(symbol, 0, 1);
            var Low = iLow(symbol, 0, 1);
            var Close = iClose(symbol, 0, 1);

            return new OhlcvData()
            {
                Open = Open,
                High = High,
                Low = Low,
                Close = Close
            };
        }

        private void AddToBarList(OhlcvData newBar)
        {
            var length = BarList.Length;
            if (BarList[length - 1] != null) //Array Dolu
            {
                // Shift
                for (var i = 1; i < length; i++)
                {
                    BarList[i - 1] = BarList[i];
                }

                BarList[length - 1] = newBar;
            }
            else
            {
                for (var i = 0; i < length; i++)
                {
                    if (BarList[i] != null)
                        continue;

                    BarList[i] = newBar;
                    break;
                }
            }
        }
    }
}