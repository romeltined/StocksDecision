using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using StocksDecision.Models;
using NLog;

namespace StocksDecision.Controllers
{
    public class StocksApiController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        // GET: api/StocksApi
        public IEnumerable<string> Get()
        {
            if (DateTime.UtcNow.Hour >= 13 && DateTime.UtcNow.Hour <= 21 && (DateTime.UtcNow.DayOfWeek != DayOfWeek.Saturday && DateTime.UtcNow.DayOfWeek != DayOfWeek.Sunday))
            {
                StockRealTime sr = new StockRealTime();
                sr.RefreshDataAll();
            }
            //logger.Info("#StocksApi Get");
            return new string[] { "value1", "value2" };
        }

        // GET: api/StocksApi/5
        public string Get(int id)
        {
            try
            { 
            StockRealTime sr = new StockRealTime();
            StocksData sd = new StocksData();
            switch (id)
            {
                case 1234:
                    sr.NotifyBuyOrSell();
                    break;
                case 2345:
                    sr.RefreshDataAll();
                    break;
                case 3456:
                    sd.GetDataAll();
                    break;
                case 999:
                    sd.AddStockCorrels();
                    sd.ProcessCorrelation();
                    break;
                case 6789:
                    sr.CacheSimulator();
                    break;
                case 102:
                    //sr.NewMailSendTester();
                    sr.NotifyUser();
                    break;
            }
            }
            catch (Exception ex)
            {
                //logger.Error("#" + ex.Message);   
            }
            return "value";
        }

        // POST: api/StocksApi
        public void Post([FromBody]string value)
        {

        }

        // PUT: api/StocksApi/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/StocksApi/5
        public void Delete(int id)
        {
        }
    }
}
