namespace StocksDecision.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SD2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.StockMarkets", "Quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.StockMarkets", "BuyTarget", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.StockMarkets", "SellTarget", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.StockMarkets", "SellTarget", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.StockMarkets", "BuyTarget", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.StockMarkets", "Quantity", c => c.Decimal(precision: 18, scale: 2));
        }
    }
}
