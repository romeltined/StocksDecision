namespace StocksDecision.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SD1 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Blogs", "Contributors");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Blogs", "Contributors", c => c.String(unicode: false));
        }
    }
}
