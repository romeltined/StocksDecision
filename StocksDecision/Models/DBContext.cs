using System.Data.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure;
using MySql.Data.Entity;

namespace StocksDecision.Models
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public System.Data.Entity.DbSet<StocksDecision.Models.Stock> Stocks { get; set; }

        public System.Data.Entity.DbSet<StocksDecision.Models.StockSymbol> StockSymbols { get; set; }

        public System.Data.Entity.DbSet<StocksDecision.Models.StockMarket> StockMarkets { get; set; }

        public System.Data.Entity.DbSet<StocksDecision.Models.Blog> Blogs { get; set; }

        public System.Data.Entity.DbSet<StocksDecision.Models.StockCorrel> StockCorrels { get; set; }

    }
}