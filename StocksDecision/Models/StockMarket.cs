using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StocksDecision.Models
{
    public class StockMarket
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public decimal Quantity { get; set; }
        public decimal BuyTarget { get; set; }
        public decimal SellTarget { get; set; }
        public string UserId { get; set; }
    }

}