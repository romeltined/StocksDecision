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
using MathNet.Numerics.Statistics;
using MathNet.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StocksDecision.Models
{
    public class StocksData
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public Stock GetLatestData(string symbol)
        {
            var data = db.Stocks.Where(d => d.Symbol == symbol);
            var maxdata= data.Where(s => s.Date == data.Max(x => x.Date))
                    .FirstOrDefault();
            if(maxdata != null)
            {
                return maxdata;
            }
            else
            {
                return null;
            }
        }



        public string GetRawData(string symbol)
        {
            string sourceURL = "https://www.alphavantage.co/query?function=TIME_SERIES_DAILY_ADJUSTED&symbol={0}&interval=1min&apikey=I6DJMCFVCAYC1WY8";
            string uRLrequest = string.Format(sourceURL, symbol);

            try
            {
                string result = null;
                DateTime initime = DateTime.Now;
                while (result==null && ((DateTime.Now - initime).TotalSeconds <10))
                { 
                    WebRequest request = WebRequest.Create(uRLrequest);
                    WebResponse response = request.GetResponse();
                    Stream dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    result = reader.ReadToEnd();
                }
                return result;
            }
            catch
            {
                return null;
            }
        }

        //New Using Alphavantage
        public List<Stock> ParseRawData(string symbol, string rawdata)
        {
            List<Stock> list = new List<Stock>();

            //implementation
            string cleandata = rawdata.Replace("//", "");
            cleandata = cleandata.Replace("[", "");
            cleandata = cleandata.Replace("]", "");
            var details = JObject.Parse(cleandata);
            var stocks = JObject.Parse(details["Time Series (Daily)"].ToString());


            foreach (var s in stocks)
            {
                var element = s.Key;
                var objStock = JObject.Parse(stocks[element].ToString());

                Stock stock = new Stock();
                stock.Symbol = symbol;
                stock.Date = DateTime.Parse(element);
                stock.Open = decimal.Parse(objStock["1. open"].ToString());
                stock.High = decimal.Parse(objStock["2. high"].ToString());
                stock.Low = decimal.Parse(objStock["3. low"].ToString());
                stock.Close = decimal.Parse(objStock["4. close"].ToString());
                stock.Volume = decimal.Parse(objStock["6. volume"].ToString());
                list.Add(stock);

            }

            return list;
        }

        public void AddStocks(Stock stock)
        {
            db.Stocks.Add(stock);
            db.SaveChanges();
        }

        public void GetDataPerSymbol(string symbol)
        {
            Stock stk = GetLatestData(symbol);
            string rawData = GetRawData(symbol);
            if (rawData != "")
            {
                List<Stock> list = ParseRawData(symbol, rawData);
                if (stk != null)
                {
                    DateTime iter = stk.Date.AddDays(1);
                    while (DateTime.Now >= iter)
                    {
                        var addStock = list.Where(s => s.Date == iter).FirstOrDefault();
                        if (addStock != null)
                        {
                            AddStocks(addStock);
                        }
                        iter = iter.AddDays(1);
                    }
                }
                else
                {
                    foreach (Stock stock in list)
                    {
                        //if (stock.Date != DateTime.Now)
                        AddStocks((Stock)stock);
                    }
                }
            }
        }


        public void GetDataAll()
        {
            var allStocks = db.Stocks.Select(s => s.Symbol).ToList().Distinct().ToList();
            //List<StockMarket> allStocks = db.StockMarkets.ToList();
            foreach (var st in allStocks)
            {
                GetDataPerSymbol(st);
            }
 
        }

        public void GetNewEntry()
        {
            List<string> newStocks = db.StockSymbols.Distinct().Select(s => s.Symbol).ToList().Except(db.Stocks.Distinct().Select(s => s.Symbol).ToList()).ToList();
            foreach (string st in newStocks)
            {
                GetDataPerSymbol(st);
            }
        }

        public DateTime StringToDate(string date)
        {
            //string[] element = date.Split(new string[] { "-" }, StringSplitOptions.None);
            //string monthInDigit = DateTime.ParseExact(element[1].ToString(), "MMM", CultureInfo.CurrentCulture).Month.ToString();
            //string yearInDigit = "20" + element[2].ToString();
            //DateTime dtvalue = DateTime.Parse(string.Format("{0}/{1}/{2}", monthInDigit, element[0], yearInDigit));

            string format = "d/M/yyyy";
            CultureInfo provider = CultureInfo.InvariantCulture;

            string[] element = date.Split(new string[] { "-" }, StringSplitOptions.None);
            string monthInDigit = DateTime.ParseExact(element[1].ToString(), "MMM", CultureInfo.CurrentCulture).Month.ToString();
            string yearInDigit = "20" + element[2].ToString();
            string dateString = element[0] + "/" + monthInDigit + "/" + yearInDigit;// + " 12:00:00.00";

            DateTime dtvalue = DateTime.ParseExact(dateString, format, provider);
            return dtvalue;
        }


        public void ProcessCorrelation()
        {
            try
            {
                double correl = 0;
                var list = db.StockCorrels.ToList();
                foreach(var s in list)
                {
                    string[] symbols = s.Name.Split(' ');
                    correl = GetCorrelation(symbols[0], symbols[1]);
                    s.Value = correl;
                    db.Entry(s).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch
            { }
        }

        public double GetCorrelation(string symbol1, string symbol2)
        {
            double result = 0;

            try
            { 
                List<CorrelUnit> array1 = new List<CorrelUnit>();
                List<CorrelUnit> array2 = new List<CorrelUnit>();
                List<double> data1 = new List<double>();
                List<double> data2 = new List<double>();

                array1 = db.Stocks.Where(s => s.Symbol == symbol1)
                            .Select(s => new CorrelUnit() { Date = s.Date, Value = s.Close }).ToList();

                array2 = db.Stocks.Where(s => s.Symbol == symbol2)
                            .Select(s => new CorrelUnit() { Date = s.Date, Value = s.Close }).ToList();

                var join = from q in array1
                             join a in array2
                             on q.Date equals a.Date
                             select new { Date = q.Date, Value1 = q.Value, Value2=a.Value };

                foreach(var d in join)
                {
                    data1.Add(Convert.ToDouble(d.Value1));
                    data2.Add(Convert.ToDouble(d.Value2)); 
                }

                result = Correlation.Pearson(data1, data2);
            }
            catch
            {
                result = 0;
            }

            return result;
        }


        public void AddStockCorrels()
        {
            var existingList = db.StockCorrels.Select(s => s.Name).OrderBy(s => s).ToList();
            //var sample = db.Stocks.Select(s => s.Symbol).ToList().Distinct();
            var newList = PermutationList(db.Stocks.Select(s => s.Symbol).ToList().Distinct().ToList()).Except(existingList).ToList();
            foreach(string s in newList)
            {
                StockCorrel sc = new StockCorrel { Name = s, Value = 0 };
                db.StockCorrels.Add(sc);
                db.SaveChanges();
            }

        }

        public List<string> PermutationList(List<string> list)
        {
            List<string> finalResult = new List<string>();
            var result = GetPermutations(list.OrderBy(s => s), 2);
            foreach (var perm in result)
            {
                string comb = "";
                foreach (var c in perm)
                {
                    //Console.Write(c + " ");
                    comb += c + " ";
                }
                finalResult.Add(comb.TrimEnd());
            }

            return finalResult;
        }

        public IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> items, int count)
        {
            int i = 0;
            foreach (var item in items)
            {
                if (count == 1)
                    yield return new T[] { item };
                else
                {
                    foreach (var result in GetPermutations(items.Skip(i + 1), count - 1))
                        yield return new T[] { item }.Concat(result);
                }

                ++i;
            }
        }


        public List<RegressionUnit> Forecast(DateTime date, string model)
        {
            List<RegressionUnit> result = new List<RegressionUnit>();
            DateTime reference = DateTime.Parse("1/1/1900");
            int addDays = -15;
            double currentvalue;
            double percentdiff;

            switch (model)
            { 
                case "2w":
                    addDays = -15; break;
                case "1mo":
                    addDays = -30; break;
            }
            
            DateTime datescope = DateTime.Now.AddDays(addDays);
            try
            {
                var symbols = db.Stocks.Select(s => s.Symbol).ToList().Distinct().ToList();
                List<double> xdata = new List<double>();
                List<double> ydata = new List<double>();
                
                foreach (var symbol in symbols)
                {
                    currentvalue = 0;
                    xdata.Clear();
                    ydata.Clear();
                    var stocks = db.Stocks.Where(s => s.Symbol == symbol && s.Date>=datescope).OrderBy(s => s.Date).Select(p=> new { Date = p.Date, Value = p.Close }).ToList();
                    foreach (var stock in stocks)
                    {
                        xdata.Add((stock.Date - reference).TotalDays);
                        ydata.Add(Convert.ToDouble(stock.Value));
                        if (stocks.IndexOf(stock)+1 == stocks.Count) { currentvalue = Convert.ToDouble(stock.Value); }
                    }
                    Tuple<double, double> r = Fit.Line(xdata.ToArray<double>(), ydata.ToArray<double>());
                    double a = r.Item1; 
                    double b = r.Item2; 
                    double y = a + b * ((date - reference).TotalDays);
                    percentdiff = Math.Round(100 * ((y - currentvalue) / currentvalue),2);
                    result.Add(new RegressionUnit { Symbol = symbol, Value = y, CurrentValue=currentvalue, PercentDiff=percentdiff });
                }

            }
            catch (Exception ex)
            { }

            return result.OrderBy(s=>s.Symbol).ToList();
        }

    }

    public class RegressionUnit
    {
        public string Symbol { get; set; }
        public double Value { get; set; }
        public virtual double CurrentValue { get; set; }
        public virtual double PercentDiff { get; set; }
    }


    public class CorrelUnit
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
    }


    //Deprecated due to Google Finance sunset
    //public List<Stock> ParseRawData(string symbol, string rawdata)
    //{
    //    List<Stock> list = new List<Stock>();
    //    string[] lines = rawdata.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
    //    foreach (string line in lines)
    //    {
    //        if(line!="")
    //        { 
    //            string[] element = line.Split(new string[] { "," }, StringSplitOptions.None);
    //            if (element[0] != "Date")
    //            {
    //                Stock stock = new Stock();
    //                stock.Date =  StringToDate(element[0]);
    //                stock.Symbol = symbol;
    //                decimal elementvalue;
    //                //Open
    //                if (Decimal.TryParse(element[1], out elementvalue))
    //                    stock.Open = elementvalue;
    //                else
    //                    stock.Open = 0;
    //                //High
    //                if (Decimal.TryParse(element[2], out elementvalue))
    //                    stock.High = elementvalue;
    //                else
    //                    stock.High = 0;
    //                //Low
    //                if (Decimal.TryParse(element[3], out elementvalue))
    //                    stock.Low = elementvalue;
    //                else
    //                    stock.Low = 0;
    //                //Close
    //                if (Decimal.TryParse(element[4], out elementvalue))
    //                    stock.Close = elementvalue;
    //                else
    //                    stock.Close = 0;
    //                //Volume
    //                if (Decimal.TryParse(element[5], out elementvalue))
    //                    stock.Volume = elementvalue;
    //                else
    //                    stock.Volume = 0;
    //                list.Add(stock);
    //            }
    //            else { }
    //        }
    //    }
    //    return list;
    //}

    //Deprecated due to Google Finance sunset
    //public string GetRawData(string symbol)
    //{
    //    string responseFromServer = "";
    //    WebRequest request = WebRequest.Create(
    //      "https://www.google.com/finance/historical?output=csv&q=" + symbol);
    //    try
    //    {
    //        WebResponse response = request.GetResponse();
    //        //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
    //        Stream dataStream = response.GetResponseStream();
    //        StreamReader reader = new StreamReader(dataStream);
    //        responseFromServer = reader.ReadToEnd();
    //        return responseFromServer;
    //    }
    //    catch { responseFromServer = "";  }
    //    return responseFromServer;
    //}

    //New Using Alphavantage
}