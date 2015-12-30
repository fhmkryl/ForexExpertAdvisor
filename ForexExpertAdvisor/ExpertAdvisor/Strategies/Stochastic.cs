using NQuotes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpertAdvisor.Strategies
{
    public class Stochastic
    {
        private MqlApi _mqlApi;
        private string _symbol;

        int AccDigits = 5;
        double Lots = 0.5;
        int StopLoss = 100;
        int TakeProfit = 0;
        int Slippage = 3;
        int Magic = 20091107;

        string _tmp2_ = " --- Stochastic ---";
        int StochKperiod = 5;
        int StochDperiod = 3;
        int Stochslowing = 3;
        int Stochmethod = MqlApi.MODE_SMA;
        int Stochprice_field = 0;
        int StochSignalBar = 1;
        double StochOverboughtLevel = 80.0;
        double StochOversoldLevel = 20.0;


        string _tmp3_ = " --- Trailing ---";
        bool TrailingOn = true;
        double TrailingStart = 10;
        double TrailingSize = 10;

        string _tmp4_ = " --- Chart ---";
        Color clBuy = Color.DodgerBlue;
        Color clSell = Color.Red;
        Color clModify = Color.Silver;
        Color clClose = Color.Gold;

        string trendDirection = "DOWN";
        int RepeatN = 5;

        int BuyCnt, SellCnt;
        int BuyStopCnt, SellStopCnt;
        int BuyLimitCnt, SellLimitCnt;

        public Stochastic(MqlApi mqlApi, string symbol)
        {
            _mqlApi = mqlApi;
            _symbol = symbol;
        }

        public void Execute()
        {
            //-----

            if (TrailingOn) TrailPositions();

            //-----

            double STO_M1 = _mqlApi.iStochastic(_symbol, 0, StochKperiod, StochDperiod, Stochslowing, Stochmethod, Stochprice_field, MqlApi.MODE_MAIN, StochSignalBar);
            double STO_M2 = _mqlApi.iStochastic(_symbol, 0, StochKperiod, StochDperiod, Stochslowing, Stochmethod, Stochprice_field, MqlApi.MODE_MAIN, StochSignalBar + 1);

            //-----

            if (OrdersCountBar0(0) > 0) return;

            RecountOrders();

            //-----

            double price, sl, tp;
            int ticket;

            if (STO_M1 > StochOversoldLevel && STO_M2 <= StochOversoldLevel)
            {
                if (BuyCnt > 0) return;
                if (CloseOrders(MqlApi.OP_SELL) > 0) return;

                //-----

                for (int i = 0; i < RepeatN; i++)
                {
                    _mqlApi.RefreshRates();
                    price = _mqlApi.Ask;

                    var p = _mqlApi.Point;
                    var f = fpc();

                    sl = If(StopLoss > 0, price - StopLoss * _mqlApi.Point * fpc(), 0);
                    tp = If(TakeProfit > 0, price + TakeProfit * _mqlApi.Point * fpc(), 0);

                    ticket = Buy(_symbol, GetLots(), price, sl, tp, Magic);
                    if (ticket > 0) break;
                }

                return;
            }

            if (STO_M1 < StochOverboughtLevel && STO_M2 >= StochOverboughtLevel)
            {
                if (SellCnt > 0) return;
                if (CloseOrders(MqlApi.OP_BUY) > 0) return;

                //-----    

                for (int i = 0; i < RepeatN; i++)
                {
                    _mqlApi.RefreshRates();
                    price = _mqlApi.Bid;

                    sl = If(StopLoss > 0, price + StopLoss * _mqlApi.Point * fpc(), 0);
                    tp = If(TakeProfit > 0, price - TakeProfit * _mqlApi.Point * fpc(), 0);

                    ticket = Sell(_symbol, GetLots(), price, sl, tp, Magic);
                    if (ticket > 0) break;
                }

                return;
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        double If(bool cond, double if_true, double if_false)
        {
            if (cond) return (if_true);
            return (if_false);
        }

        int fpc()
        {
            if (AccDigits == 5) return (10);
            if (AccDigits == 6) return (100);
            return (1);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        double GetLots()
        {
            return (Lots);
        }

        void RecountOrders()
        {
            BuyCnt = 0;
            SellCnt = 0;
            BuyStopCnt = 0;
            SellStopCnt = 0;
            BuyLimitCnt = 0;
            SellLimitCnt = 0;

            int cnt = _mqlApi.OrdersTotal();
            for (int i = 0; i < cnt; i++)
            {
                if (!_mqlApi.OrderSelect(i, MqlApi.SELECT_BY_POS, MqlApi.MODE_TRADES)) continue;
                if (_mqlApi.OrderSymbol() != _symbol) continue;
                if (_mqlApi.OrderMagicNumber() != Magic) continue;

                int type = _mqlApi.OrderType();
                if (type == MqlApi.OP_BUY) BuyCnt++;
                if (type == MqlApi.OP_SELL) SellCnt++;
                if (type == MqlApi.OP_BUYSTOP) BuyStopCnt++;
                if (type == MqlApi.OP_SELLSTOP) SellStopCnt++;
                if (type == MqlApi.OP_BUYLIMIT) BuyLimitCnt++;
                if (type == MqlApi.OP_SELLLIMIT) SellLimitCnt++;
            }
        }

        int OrdersCountBar0(int TF)
        {
            int orders = 0;

            int cnt = _mqlApi.OrdersTotal();
            for (int i = 0; i < cnt; i++)
            {
                if (!_mqlApi.OrderSelect(i, MqlApi.SELECT_BY_POS, MqlApi.MODE_TRADES)) continue;
                if (_mqlApi.OrderSymbol() != _symbol) continue;
                if (_mqlApi.OrderMagicNumber() != Magic) continue;

                if (_mqlApi.OrderOpenTime() >= _mqlApi.iTime(_symbol, TF, 0)) orders++;
            }

            cnt = _mqlApi.OrdersHistoryTotal();
            for (int i = 0; i < cnt; i++)
            {
                if (!_mqlApi.OrderSelect(i, MqlApi.SELECT_BY_POS, MqlApi.MODE_HISTORY)) continue;
                if (_mqlApi.OrderSymbol() != _symbol) continue;
                if (_mqlApi.OrderMagicNumber() != Magic) continue;

                if (_mqlApi.OrderOpenTime() >= _mqlApi.iTime(_symbol, TF, 0)) orders++;
            }

            return (orders);
        }

        int CloseOrders(int type1, int type2 = -1)
        {
            int cnt = _mqlApi.OrdersTotal();
            for (int i = cnt - 1; i >= 0; i--)
            {
                if (!_mqlApi.OrderSelect(i, MqlApi.SELECT_BY_POS, MqlApi.MODE_TRADES)) continue;
                if (_mqlApi.OrderSymbol() != _symbol) continue;
                if (_mqlApi.OrderMagicNumber() != Magic) continue;

                int type = _mqlApi.OrderType();
                if (type != type1 && type != type2) continue;

                if (type == MqlApi.OP_BUY)
                {
                    _mqlApi.RefreshRates();
                    CloseOrder(_mqlApi.OrderTicket(), _mqlApi.OrderLots(), _mqlApi.MarketInfo(_symbol, MqlApi.MODE_BID));
                    continue;
                }

                if (type == MqlApi.OP_SELL)
                {
                    _mqlApi.RefreshRates();
                    CloseOrder(_mqlApi.OrderTicket(), _mqlApi.OrderLots(), _mqlApi.MarketInfo(_symbol, MqlApi.MODE_ASK));
                    continue;
                }
            }

            int orders = 0;
            cnt = _mqlApi.OrdersTotal();
            for (int i = 0; i < cnt; i++)
            {
                if (!_mqlApi.OrderSelect(i, MqlApi.SELECT_BY_POS, MqlApi.MODE_TRADES)) continue;
                if (_mqlApi.OrderSymbol() != _symbol) continue;
                if (_mqlApi.OrderMagicNumber() != Magic) continue;

                int type = _mqlApi.OrderType();
                if (type != type1 && type != type2) continue;

                orders++;
            }

            return (orders);
        }

        void TrailPositions()
        {
            double StopLevel = _mqlApi.MarketInfo(_symbol, MqlApi.MODE_STOPLEVEL) + 1;
            double sl;

            int cnt = _mqlApi.OrdersTotal();
            for (int i = 0; i < cnt; i++)
            {
                if (!_mqlApi.OrderSelect(i, MqlApi.SELECT_BY_POS, MqlApi.MODE_TRADES)) continue;
                if (_mqlApi.OrderSymbol() != _symbol) continue;
                if (_mqlApi.OrderMagicNumber() != Magic) continue;

                int type = _mqlApi.OrderType();
                if (type == MqlApi.OP_BUY)
                {
                    if (_mqlApi.Bid - _mqlApi.OrderOpenPrice() > TrailingStart * _mqlApi.Point * fpc())
                    {
                        sl = _mqlApi.Bid - TrailingSize * _mqlApi.Point * fpc();

                        if (sl >= _mqlApi.Bid - StopLevel * _mqlApi.Point) continue;

                        if (_mqlApi.OrderStopLoss() < sl - 1 * _mqlApi.Point * fpc())
                        {
                            _mqlApi.OrderModify(_mqlApi.OrderTicket(), _mqlApi.OrderOpenPrice(), sl, _mqlApi.OrderTakeProfit(), DateTime.MinValue, clModify);
                        }
                    }
                }

                if (type == MqlApi.OP_SELL)
                {
                    if (_mqlApi.OrderOpenPrice() - _mqlApi.Ask > TrailingStart * _mqlApi.Point * fpc())
                    {
                        sl = _mqlApi.Ask + TrailingSize * _mqlApi.Point * fpc();

                        if (sl <= _mqlApi.Ask + StopLevel * _mqlApi.Point) continue;

                        if (_mqlApi.OrderStopLoss() > sl + 1 * _mqlApi.Point * fpc() || _mqlApi.OrderStopLoss() == 0)
                        {
                            _mqlApi.OrderModify(_mqlApi.OrderTicket(), _mqlApi.OrderOpenPrice(), sl, _mqlApi.OrderTakeProfit(), DateTime.MinValue, clModify);
                        }
                    }
                }
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        int SleepOk = 2000;
        int SleepErr = 6000;

        int Buy(string symbol, double lot, double price, double sl, double tp, int magic, string comment = "")
        {
            int dig = (int)_mqlApi.MarketInfo(symbol, MqlApi.MODE_DIGITS);

            price = _mqlApi.NormalizeDouble(price, dig);
            sl = _mqlApi.NormalizeDouble(sl, dig);
            tp = _mqlApi.NormalizeDouble(tp, dig);

            string _lot = _mqlApi.DoubleToStr(lot, 2);
            string _price = _mqlApi.DoubleToStr(price, dig);
            string _sl = _mqlApi.DoubleToStr(sl, dig);
            string _tp = _mqlApi.DoubleToStr(tp, dig);

            _mqlApi.Print("Buy \"", symbol, "\", ", _lot, ", ", _price, ", ", Slippage, ", ", _sl, ", ", _tp, ", ", magic, ", \"", comment, "\"");

            int res = _mqlApi.OrderSend(symbol, MqlApi.OP_BUY, lot, price, Slippage, sl, tp, comment, magic, DateTime.MinValue, clBuy);
            if (res >= 0)
            {
                _mqlApi.Sleep(SleepOk);
                return (res);
            }

            int code = _mqlApi.GetLastError();
            //_mqlApi.Print("Error opening BUY order: ", _mqlApi.ErrorDescription(code), " (", code, ")");
            _mqlApi.Sleep(SleepErr);

            return (-1);
        }

        int Sell(string symbol, double lot, double price, double sl, double tp, int magic, string comment = "")
        {
            int dig = (int)_mqlApi.MarketInfo(symbol, MqlApi.MODE_DIGITS);

            price = _mqlApi.NormalizeDouble(price, dig);
            sl = _mqlApi.NormalizeDouble(sl, dig);
            tp = _mqlApi.NormalizeDouble(tp, dig);

            string _lot = _mqlApi.DoubleToStr(lot, 2);
            string _price = _mqlApi.DoubleToStr(price, dig);
            string _sl = _mqlApi.DoubleToStr(sl, dig);
            string _tp = _mqlApi.DoubleToStr(tp, dig);

            _mqlApi.Print("Sell \"", symbol, "\", ", _lot, ", ", _price, ", ", Slippage, ", ", _sl, ", ", _tp, ", ", magic, ", \"", comment, "\"");

            int res = _mqlApi.OrderSend(symbol, MqlApi.OP_SELL, lot, price, Slippage, sl, tp, comment, magic, DateTime.MinValue, clSell);
            if (res >= 0)
            {
                _mqlApi.Sleep(SleepOk);
                return (res);
            }

            int code = _mqlApi.GetLastError();
            //_mqlApi.Print("Error opening SELL order: ", _mqlApi.ErrorDescription(code), " (", code, ")");
            _mqlApi.Sleep(SleepErr);

            return (-1);
        }

        bool CloseOrder(int ticket, double lot, double price)
        {
            if (!_mqlApi.OrderSelect(ticket, MqlApi.SELECT_BY_TICKET)) return (false);
            if (_mqlApi.OrderCloseTime() > DateTime.MinValue) return (false);

            int dig = (int)_mqlApi.MarketInfo(_mqlApi.OrderSymbol(), MqlApi.MODE_DIGITS);
            string _lot = _mqlApi.DoubleToStr(lot, 2);
            string _price = _mqlApi.DoubleToStr(price, dig);

            _mqlApi.Print("CloseOrder ", ticket, ", ", _lot, ", ", _price, ", ", Slippage);

            bool res = _mqlApi.OrderClose(ticket, lot, price, Slippage, clClose);
            if (res)
            {
                _mqlApi.Sleep(SleepOk);
                return (res);
            }

            int code = _mqlApi.GetLastError();
            //_mqlApi.Print("CloseOrder failed: ", _mqlApi.ErrorDescription(code), " (", code, ")");
            _mqlApi.Sleep(SleepErr);

            return (false);
        }
    }
}
