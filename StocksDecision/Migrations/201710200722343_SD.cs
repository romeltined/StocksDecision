namespace StocksDecision.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SD : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Blogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Message = c.String(unicode: false),
                        User = c.String(unicode: false),
                        CreatedOn = c.DateTime(nullable: false, precision: 0),
                        Contributors = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Comments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Message = c.String(unicode: false),
                        User = c.String(unicode: false),
                        CreatedOn = c.DateTime(nullable: false, precision: 0),
                        Blog_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Blogs", t => t.Blog_Id)
                .Index(t => t.Blog_Id);
            
            CreateTable(
                "dbo.StockCorrels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        Value = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.StockMarkets",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Symbol = c.String(unicode: false),
                        Quantity = c.Decimal(precision: 18, scale: 2),
                        BuyTarget = c.Decimal(precision: 18, scale: 2),
                        SellTarget = c.Decimal(precision: 18, scale: 2),
                        UserId = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Stocks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false, precision: 0),
                        Symbol = c.String(unicode: false),
                        Open = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Low = c.Decimal(nullable: false, precision: 18, scale: 2),
                        High = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Close = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Volume = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.StockSymbols",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Symbol = c.String(unicode: false),
                        LastPrice = c.Decimal(precision: 18, scale: 2),
                        PreviousPrice = c.Decimal(precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Comments", "Blog_Id", "dbo.Blogs");
            DropIndex("dbo.Comments", new[] { "Blog_Id" });
            DropTable("dbo.StockSymbols");
            DropTable("dbo.Stocks");
            DropTable("dbo.StockMarkets");
            DropTable("dbo.StockCorrels");
            DropTable("dbo.Comments");
            DropTable("dbo.Blogs");
        }
    }
}
