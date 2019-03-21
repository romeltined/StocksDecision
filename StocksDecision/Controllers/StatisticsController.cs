using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Newtonsoft;
using StocksDecision.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StocksDecision.Controllers
{
    public class StatisticsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        //private RedisConnection obj = new RedisConnection();
        private ICacheClient stackCache = new StackExchangeRedis().Connection;

        // GET: Correlations
        public ActionResult Index()
        {
            var stockCorrels = db.StockCorrels.OrderByDescending(s => s.Value).ToList();
            return View(stockCorrels);
        }

        // GET: Correlations
        public ActionResult Forecast(DateTime? datepicker, string model="")
        {
            DateTime selectedDate = datepicker ?? DateTime.Now;
            ViewBag.SelectedDate = selectedDate.ToString("dd/MMM/yyyy");
            model = model == "" ? "2w" : model;
            ViewBag.SelectedModel = model;
            string key = ViewBag.SelectedDate + "@" + ViewBag.SelectedModel;

            List<RegressionUnit> result = GetForecast(selectedDate, model, key);

            return View(result);
        }

        // GET: Correlations/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        public List<RegressionUnit> GetForecast(DateTime date, string model, string key)
        {

            List<RegressionUnit> forecast = null;
            //var serializer = new NewtonsoftSerializer();
            //var cacheClient = new StackExchangeRedisCacheClient(serializer);

            var cacheForecast = stackCache.Get<List<RegressionUnit>>(key);
            if (cacheForecast == null)
            {
                StocksData sd = new StocksData();
                forecast = sd.Forecast(date, model);
                stackCache.Add(key, forecast, DateTimeOffset.Now.AddHours(1));
                return forecast;
            }

            return cacheForecast;

        }

    }



    public class Symbols
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
