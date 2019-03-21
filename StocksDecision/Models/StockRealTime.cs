using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using StocksDecision.Models;
using System.IO;
using System.Globalization;
using Newtonsoft;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using System.Security.Principal;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using Newtonsoft.Json;
using StackExchange.Redis.Extensions.Newtonsoft;
using StackExchange.Redis.Extensions.Core;
using NLog;

namespace StocksDecision.Models
{
    public class StockRealTime
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private readonly IIdentity currentUser;
        private ICacheClient stackCache = new StackExchangeRedis().Connection;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public StockRealTime()
        {
            currentUser = ClaimsPrincipal.Current.Identity;
        }

        public List<StockMarket> GetDataPerUser()
        {
            List<StockMarket> result = null;
            string userid = currentUser.GetUserId();
            result = db.StockMarkets.Where(s => s.UserId == userid).OrderBy(s=>s.Symbol).ToList();
            return result;
        }


        public StockSymbol GetDataPerSymbol(string symbol)
        {
            string rawdata = GetRawData(symbol);

            StockSymbol stockSymbol = new StockSymbol();

            string cleandata = rawdata.Replace("//", "");
            cleandata = cleandata.Replace("[", "");
            cleandata = cleandata.Replace("]", "");
            var details = JObject.Parse(cleandata);
            var metadata = JObject.Parse(details["Meta Data"].ToString());
            var lastRefresh = DateTime.Parse(metadata["3. Last Refreshed"].ToString()).ToString("yyyy-MM-dd");
            var stocks = JObject.Parse(details["Time Series (Daily)"].ToString());
            var stock = JObject.Parse(stocks[lastRefresh].ToString());
            string strPreviousDay = GetPreviousWorkingDay(DateTime.Parse(lastRefresh)).ToString("yyyy-MM-dd");
            var prevStock = JObject.Parse(stocks[strPreviousDay].ToString());

            stockSymbol.Symbol = symbol;
            stockSymbol.LastPrice = decimal.Parse(stock["5. adjusted close"].ToString());
            stockSymbol.PreviousPrice = decimal.Parse(prevStock["5. adjusted close"].ToString());

            return stockSymbol;
        }

        public void RefreshDataAll()
        {
            try
            { 
                //using (var context = new ApplicationDbContext())
                //{ 
                //    List<StockSymbol> allStocks = context.StockSymbols.ToList();
                //    foreach (var stockSymbol in allStocks)
                //    {
                //        var stock = GetDataPerSymbol(stockSymbol.Symbol);
                //        stockSymbol.LastPrice = stock.LastPrice;
                //        stockSymbol.PreviousPrice = stock.PreviousPrice;
                //        context.StockSymbols.Attach(stockSymbol);
                //        context.Entry(stockSymbol).Property(p => p.LastPrice).IsModified = true;
                //        context.Entry(stockSymbol).Property(p => p.PreviousPrice).IsModified = true;
                //        context.SaveChanges();
                //    }
                //    CacheAllStocks(allStocks);
                //}


                    List<StockSymbol> allStocks = db.StockSymbols.ToList();
                    foreach (var stockSymbol in allStocks)
                    {
                        var stock = GetDataPerSymbol(stockSymbol.Symbol);
                        stockSymbol.LastPrice = stock.LastPrice;
                        stockSymbol.PreviousPrice = stock.PreviousPrice;
                        db.StockSymbols.Attach(stockSymbol);
                        db.Entry(stockSymbol).Property(p => p.LastPrice).IsModified = true;
                        db.Entry(stockSymbol).Property(p => p.PreviousPrice).IsModified = true;
                        db.SaveChanges();
                    }
                    CacheAllStocks(allStocks);


            }
            catch (Exception ex)
            {
                //logger.Error("#" + ex.Message);
            }
        }

        public void CacheAllStocks(List<StockSymbol> allStocks)
        {
            //var serializer = new NewtonsoftSerializer();
            //var cacheClient = new StackExchangeRedisCacheClient(serializer);
            string key = "StocksDecisionAllStocks";
            stackCache.Remove(key);
            stackCache.Add(key, allStocks, DateTimeOffset.Now.AddHours(156));         
        }

        public void AddStockSymbol(string symbol)
        {
            db.StockSymbols.Add(GetDataPerSymbol(symbol));
            db.SaveChanges();
        }

        public void NotifyBuyOrSell()
        {
            string instruction = "No action needed. \n\n";
            string message = "";
            List<StockMarket> allStocks = db.StockMarkets.ToList(); //.Where(s=>s.BuyDecision=="Buy" || s.SellDecision=="Sell").OrderBy(s=>s.Symbol).ToList();
            foreach (StockMarket st in allStocks)
            {
                instruction = "Do the following action(s): \n\n";
                //if(st.BuyDecision=="Buy")
                //{
                //    message += string.Format("Buy {0} @ {1} \n\n",st.Symbol,st.LastPrice);
                //}
                //if (st.SellDecision == "Sell")
                //{
                //    message += string.Format("Sell {0} @ {1} \n\n", st.Symbol, st.LastPrice);
                //}
            }

            Uri uri = new Uri("https://api.mailgun.net/v3");
            RestClient client = new RestClient();
            client.BaseUrl = uri;
            client.Authenticator =
            new HttpBasicAuthenticator("api",
                                      "key-2025f215cdbd2574c366c3a80f34156b");
            RestRequest request = new RestRequest();
            request.AddParameter("domain", "adafast.com", ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "Adafast Advisor <postmaster@adafast.com>");
            request.AddParameter("to", "memel <rstined@gmail.com>");
            request.AddParameter("to", "yel <ma.elisa.a.bautista@gmail.com>");
            request.AddParameter("subject", "Stocks Advisory");
            request.AddParameter("text", instruction+message);
            request.Method = Method.POST;
            var result = client.Execute(request).ToString();
        }



        public string GetRawData(string symbol)
        {
            //string sourceURL = "http://finance.google.com/finance/info?client=ig&q=";
            //string sourceURL = "https://finance.google.com/finance?q="; 
            //string sourceURL = "https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol={0}&interval=60min&apikey=I6DJMCFVCAYC1WY8";
            string sourceURL = "https://www.alphavantage.co/query?function=TIME_SERIES_DAILY_ADJUSTED&symbol={0}&interval=1min&apikey=I6DJMCFVCAYC1WY8";

            //string stockmarket = market + ":" + symbol;

            string uRLrequest = string.Format(sourceURL, symbol);

            try
            {
                // Create a request for the URL.   
                //WebRequest request = WebRequest.Create(sourceURL + stockmarket);
                WebRequest request = WebRequest.Create(uRLrequest);

                // If required by the server, set the credentials.  
                //request.Credentials = CredentialCache.DefaultCredentials;
                // Get the response.  
                WebResponse response = request.GetResponse();
                // Display the status.  
                Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                // Get the stream containing content returned by the server.  
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.  
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.  
                string result = reader.ReadToEnd();
                return result;
            }
            catch
            {
                return null;
            }
        }

        public decimal GetLastPrice(string rawdata)
        {

            decimal result = 0;
            string cleandata = rawdata.Replace("//", "");
            cleandata = cleandata.Replace("[", "");
            cleandata = cleandata.Replace("]", "");
            var details = JObject.Parse(cleandata);
            string resultstr = details["l"].ToString();
            result = Decimal.Parse(resultstr);
            return result;
        }


        public DateTime GetPreviousWorkingDay(DateTime date)
        {
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return date.AddDays(-2);
                case DayOfWeek.Monday:
                    return date.AddDays(-3);
                default:
                    return date.AddDays(-1);
            }
        }

        public string GetAccessToken()
        {
            var client = new RestClient("https://adafast.auth0.com/oauth/token");
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", "{\"grant_type\":\"client_credentials\",\"client_id\": \"qd1CqRuODZqb3I0HUq2v2hUE9tmJ1X6G\",\"client_secret\": \"l5DG4MJy0CMqXr7RWfl0Sw0cAQiq9TO1vqC0K2R9lbjlnw0Vtt0f5uTAtX-FvziL\",\"audience\": \"https://adafast.auth0.com/api/v2/\"}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            
            var result = JsonConvert.DeserializeObject<JWT>(response.Content.ToString());
            string access_token = result.access_token;
            return access_token;
        } 

        public void NotifyUser()
        {
            var access_token = GetAccessToken();

            var userlist = db.StockMarkets.Select(s => s.UserId).ToList().Distinct().ToList();

            foreach(var user in userlist)
            { 
                var client = new RestClient("https://adafast.auth0.com/api/v2/users/" + user + "?fields=email&include_fields=true");
                var request = new RestRequest(Method.GET);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("authorization", "Bearer " + access_token);
                IRestResponse response = client.Execute(request);
                var result = JsonConvert.DeserializeObject<UserEmail>(response.Content.ToString());
                var email = result.email;

                string message = PrepareMessage(user); // "Good day! \n\n You have new advise from Adafast. \n\n Open http://adafast.com/StockMarkets/Index# to view.";

                if (!String.IsNullOrEmpty(message))
                { SendEmail(email, message); }
            }
        }

        public void SendEmail(string email, string message)
        {
            Uri uri = new Uri("https://api.mailgun.net/v3");
            RestClient client = new RestClient();
            client.BaseUrl = uri;
            client.Authenticator =
            new HttpBasicAuthenticator("api",
                                      "key-2025f215cdbd2574c366c3a80f34156b");
            RestRequest request = new RestRequest();
            request.AddParameter("domain", "adafast.com", ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "Adafast Advisor <postmaster@adafast.com>");
            request.AddParameter("to", email);
            request.AddParameter("subject", "Stocks Advisory");
            request.AddParameter("text", message);
            request.Method = Method.POST;
            var result = client.Execute(request).ToString();
        }


        public string PrepareMessage(string userid)
        {
            decimal tolerance = 0.3m;
            var data = (from sm in db.StockMarkets
                        join ss in db.StockSymbols on sm.Symbol equals ss.Symbol
                        where (sm.UserId==userid)
                        select new { Symbol = sm.Symbol, LastPrice = ss.LastPrice, sm.BuyTarget, sm.SellTarget, sm.UserId }).ToList();

            string message = "";
            foreach(var record in data)
            {
                if(record.LastPrice<=(record.BuyTarget + tolerance) && record.BuyTarget!=0)
                { message += "Buy " + record.Symbol + " @" + record.LastPrice + "\n\n"; }
                if (record.LastPrice >= (record.SellTarget- tolerance) && record.SellTarget != 0)
                { message += "Sell " + record.Symbol + " @" + record.LastPrice + "\n\n"; }

            }

            return message;
        }

        public void NewMailSendTester()
        {
            string email = "rstined@yahoo.com";
            string message = PrepareMessage("auth0|59d5bdbd8025c603ce7354f0");
            SendEmail(email, message);
        }

        public void CacheSimulator()
        {
            //decimal? lastprice = 0;
            //decimal? prevprice = 0;
            int i = 0;
            var allStocks = new List<StockSymbol>();
            string[] stocksymbols = new string[] { "HLF","IPG", "AMD", "DNKN", "POT", "TUES", "JBLU","ACN", "MSFT", "BAC", "BCS", "AIG","CMG","FIT" };
            Random random = new Random(DateTime.Now.Millisecond);
            foreach (string s in stocksymbols)
            {
                i++;
                decimal lastprice = 0;
                decimal prevprice = 0;
                lastprice = Convert.ToDecimal(random.Next(140, 400)) * .3213m;
                prevprice = Convert.ToDecimal(lastprice) * 1.532m;
                StockSymbol ss = new StockSymbol() { Id=i, Symbol=s, LastPrice=lastprice, PreviousPrice=prevprice};
                allStocks.Add(ss);
            }

            //var serializer = new NewtonsoftSerializer();
            //var cacheClient = new StackExchangeRedisCacheClient(serializer);
            string key = "StocksDecisionAllStocks";
            stackCache.Remove(key);
            stackCache.Add(key, allStocks, DateTimeOffset.Now.AddHours(40));
        }
    }

    public class UserEmail
    {
        public string email { get; set; }
    }
    public class JWT
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }
}