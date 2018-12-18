namespace SeleniumPubmedCrawler.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class deployment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Article", "publicationDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Article", "dateCrawled", c => c.DateTime(nullable: false));
            DropColumn("dbo.Article", "date");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Article", "date", c => c.DateTime(nullable: false));
            DropColumn("dbo.Article", "dateCrawled");
            DropColumn("dbo.Article", "publicationDate");
        }
    }
}
