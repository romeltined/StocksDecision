using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using StocksDecision.Models;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Security.Claims;
using System.Threading;
using System.Security.Principal;
using Microsoft.AspNet.Identity;

namespace StocksDecision.Controllers
{
    //[Authorize(Roles = "admin")]
    [Authorize]
    public class StockMarketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private readonly IIdentity currentUser;
        // GET: StockMarkets
        public StockMarketsController()
        {
            currentUser= ClaimsPrincipal.Current.Identity;
        }


        public ActionResult Index()
        {
            var claims = ClaimsPrincipal.Current.Identity;
            string name = ClaimsPrincipal.Current.FindFirst("name")?.Value;
            string user_id = ClaimsPrincipal.Current.FindFirst("user_id")?.Value;

            StockRealTime sr = new StockRealTime();
            var result = sr.GetDataPerUser();

            return View(result);
            //return View(db.StockMarkets.ToList().OrderBy(s => s.Symbol));
        }

        // GET: StockMarkets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockMarket stockMarket = db.StockMarkets.Find(id);
            if (stockMarket == null)
            {
                return HttpNotFound();
            }
            return View(stockMarket);
        }

        // GET: StockMarkets/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: StockMarkets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Symbol,BuyTarget,SellTarget,Quantity")] StockMarket stockMarket)
        {
            if (ModelState.IsValid)
            {
                StockRealTime sr = new StockRealTime();
                var stock = db.StockSymbols.Where(s => s.Symbol == stockMarket.Symbol).FirstOrDefault();
                if(stock==null)
                {
                    sr.AddStockSymbol(stockMarket.Symbol);
                }

                stockMarket.UserId = currentUser.GetUserId();
                db.StockMarkets.Add(stockMarket);
                db.SaveChanges();


                HostingEnvironment.QueueBackgroundWorkItem(clt =>
                    {
                        GetData(stockMarket.Symbol, clt);
                    }
                 );

                return RedirectToAction("Index");
            }

            return View(stockMarket);
        }


        public void GetData(string symbol, CancellationToken ct)
        {
            StocksData sd = new StocksData();
            sd.GetDataPerSymbol(symbol);
        }


        // GET: StockMarkets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockMarket stockMarket = db.StockMarkets.Find(id);
            if (stockMarket == null)
            {
                return HttpNotFound();
            }
            return View(stockMarket);
        }

        // POST: StockMarkets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,BuyTarget,LastPrice,SellTarget,Quantity")] StockMarket stockMarket)
        {
            if (ModelState.IsValid)
            {
                StockRealTime sr = new StockRealTime();
                //sr.RefreshDataPerSymbol(stockMarket);

                db.StockMarkets.Attach(stockMarket);
                db.Entry(stockMarket).Property(p => p.BuyTarget).IsModified = true;
                db.Entry(stockMarket).Property(p => p.SellTarget).IsModified = true;
                db.Entry(stockMarket).Property(p => p.Quantity).IsModified = true;
                //db.Entry(stockMarket).Property(p => p.SellDecision).IsModified = true;
                //db.Entry(stockMarket).Property(p => p.BuyDecision).IsModified = true;
                //db.Entry(stockMarket).Property(p => p.CurrentValue).IsModified = true;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(stockMarket);
        }

        // GET: StockMarkets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockMarket stockMarket = db.StockMarkets.Find(id);
            if (stockMarket == null)
            {
                return HttpNotFound();
            }
            return View(stockMarket);
        }

        // POST: StockMarkets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            StockMarket stockMarket = db.StockMarkets.Find(id);
            db.StockMarkets.Remove(stockMarket);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult ShopStock()
        {
            return View(db.StockMarkets.ToList());
        }

        public ActionResult PartialIndex()
        {
            try
            {
                //StockRealTime sd = new StockRealTime();
                //sd.GetData();
            }
            catch
            {

            }
            finally
            {

            }
            return View(db.StockMarkets.ToList().OrderBy(s=>s.Symbol));
        }
    }
}
