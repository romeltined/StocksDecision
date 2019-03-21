using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using StocksDecision.Models;
using System.Web.Helpers;
using PagedList;
using System.Threading.Tasks;

namespace StocksDecision.Controllers
{
    [Authorize]
    public class StocksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Stocks
        public ViewResult Index(string searchString, int? page)
        {
            //StocksData sd = new StocksData();
            //sd.GetDataPerSymbol(searchString);
            IPagedList<Stock> stocklist = null;
            ViewBag.Symbol = searchString;
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            var stocks = from s in db.Stocks
                           select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                stocks = stocks.Where(s => s.Symbol==searchString)
                   .OrderByDescending(s => s.Date);
                stocklist = stocks.ToPagedList(pageIndex, pageSize);
                return View(stocklist);
            }
            else
            {
                return View();
            }

        }

        // GET: Stocks/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Stock stock = db.Stocks.Find(id);
            if (stock == null)
            {
                return HttpNotFound();
            }
            return View(stock);
        }

        public ActionResult FilterIndex(string symbol, string range)
        {
            if (symbol == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //StocksData sd = new StocksData();
            //sd.GetDataPerSymbol(symbol);
            ViewBag.Range = range;
            ViewBag.Symbol = symbol;
            //return View(db.Stocks.ToList().Where(s => s.Symbol == symbol));
            StockSymbol stock = db.StockSymbols.Where(s => s.Symbol == symbol).FirstOrDefault();
            return View(stock);    
        }

        // GET: Stocks/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Stocks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Date,Symbol,Open,Low,High,Close,Volume")] Stock stock)
        {
            if (ModelState.IsValid)
            {
                db.Stocks.Add(stock);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(stock);
        }

        // GET: Stocks/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Stock stock = db.Stocks.Find(id);
            if (stock == null)
            {
                return HttpNotFound();
            }
            return View(stock);
        }

        // POST: Stocks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Date,Symbol,Open,Low,High,Close,Volume")] Stock stock)
        {
            if (ModelState.IsValid)
            {
                db.Entry(stock).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(stock);
        }

        // GET: Stocks/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Stock stock = db.Stocks.Find(id);
            if (stock == null)
            {
                return HttpNotFound();
            }
            return View(stock);
        }

        // POST: Stocks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Stock stock = db.Stocks.Find(id);
            db.Stocks.Remove(stock);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult LineChart(string symbol, string range)
        {
            int days = -360;
            switch (range)
            {
                case "3m":
                    days = -90;
                    break;
                case "6m":
                    days = -180;
                    break;
                default:
                    break;
            }

            var dateVar = DateTime.Now.AddDays(days);
            var stocks = db.Stocks.Where(s => s.Symbol == symbol
                && s.Date <= DateTime.Now && s.Date >= dateVar)
                .OrderBy(x => x.Date);

            List<string> xAxis = new List<string>();
            List<string> yAxis = new List<string>();
            foreach(var x in stocks)
            {
                xAxis.Add(x.Date.ToShortDateString());
                yAxis.Add(x.Close.ToString());
            }

            var key = new Chart(width: 600, height: 400)
                .AddSeries(
                    chartType: "line",
                    legend: "",
                    xValue: xAxis, //new[] { "Jan", "Feb", "Mar", "Apr", "May" },
                    yValues: yAxis) //new[] { "20", "20", "40", "10", "10" })
                //.Write();
                .GetBytes("png");

            return File(key, "image/png");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
