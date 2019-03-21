using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace StocksDecision.Models
{
    public class StockSymbol
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public decimal? LastPrice { get; set; }
        public decimal? PreviousPrice { get; set; }
    }
}