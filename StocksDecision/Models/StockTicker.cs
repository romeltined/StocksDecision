using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Linq;
using System.Net;
using System.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;
using StackExchange.Redis.Extensions.Core;

namespace StocksDecision.Models
{
    public class StockTicker
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // Singleton instance
        private readonly static Lazy<StockTicker> _instance = new Lazy<StockTicker>(() => new StockTicker(GlobalHost.ConnectionManager.GetHubContext<StockTickerHub>().Clients));

        private readonly ConcurrentDictionary<string, StockMarket> _stocks = new ConcurrentDictionary<string, StockMarket>();

        private readonly object _updateStockPricesLock = new object();

        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(10000);

        private readonly Timer _timer;
        private volatile bool _updatingStockPrices = false;

        private ICacheClient stackCache = new StackExchangeRedis().Connection;

        private StockTicker(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;
            _timer = new Timer(UpdateStockPrices, null, _updateInterval, _updateInterval);
        }

        public static StockTicker Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private IHubConnectionContext<dynamic> Clients
        {
            get;
            set;
        }

        public IEnumerable<StockMarket> GetAllStocks()
        {
            return _stocks.Values.OrderBy(s=>s.Symbol);
        }

        public List<StockSymbol> GetAllStockMarkets()
        {

            try
            {
                //using (var context = new ApplicationDbContext())
                //{
                //    var stocks = context.Database.SqlQuery<StockSymbol>("SELECT * FROM StockSymbols").OrderBy(s => s.Symbol).ToList();
                //    return stocks;
                //}
                var stocks = GetCacheAllStocks();
                return stocks;
            }
            catch (Exception ex)
            { }
            return null;
        }

        private void UpdateStockPrices(object state)
        {
            try
            { 
                StockRealTime sr = new StockRealTime();
                lock (_updateStockPricesLock)
                {
                    if (!_updatingStockPrices)
                    {
                        _updatingStockPrices = true;
                        //using (var context = new ApplicationDbContext())
                        //{
                        //    var stocks = context.Database.SqlQuery<StockSymbol>("SELECT * FROM StockSymbols").OrderBy(s => s.Symbol).ToList();
                        //    //var stocks = db.StockMarkets.AsNoTracking().OrderBy(s => s.Symbol).ToList();
                        //    BroadcastAllStockPrice(stocks);
                        //}
                        var stocks = GetCacheAllStocks();
                        BroadcastAllStockPrice(stocks);
                        _updatingStockPrices = false;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private List<StockSymbol> GetCacheAllStocks()
        {
            //var serializer = new NewtonsoftSerializer();
            //var cacheClient = new StackExchangeRedisCacheClient(serializer);
            string key = "StocksDecisionAllStocks";
            
            var cacheForecast = stackCache.Get<List<StockSymbol>>(key);
            while(cacheForecast==null)
            {
                cacheForecast = stackCache.Get<List<StockSymbol>>(key);
            }
            return cacheForecast;
        }

        private void BroadcastAllStockPrice(List<StockSymbol> stocks)
        {
            Clients.All.updateAllStockPrice(stocks);
        }

        //private bool TryUpdateStockPrice(StockMarket stock)
        //{

        //    var _stock = db.Database.SqlQuery<StockMarket>("SELECT * FROM StockMarkets WHERE Id=" + stock.Id).FirstOrDefault();
        //    stock.LastPrice = _stock.LastPrice;
        //    return true;
        //}

        //private void BroadcastStockPrice(StockMarket stock)
        //{
        //    Clients.All.updateStockPrice(stock);
        //}


    }
}