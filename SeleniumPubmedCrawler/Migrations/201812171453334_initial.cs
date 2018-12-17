namespace SeleniumPubmedCrawler.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Article",
                c => new
                    {
                        PMID = c.String(nullable: false, maxLength: 128),
                        title = c.String(),
                        abstractTxt = c.String(),
                        date = c.DateTime(nullable: false),
                        doi = c.String(),
                        pii = c.String(),
                        journal_nameAbbreviation = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.PMID)
                .ForeignKey("dbo.Journal", t => t.journal_nameAbbreviation)
                .Index(t => t.journal_nameAbbreviation);
            
            CreateTable(
                "dbo.Author",
                c => new
                    {
                        firstName = c.String(nullable: false, maxLength: 128),
                        lastName = c.String(nullable: false, maxLength: 128),
                        initials = c.String(nullable: false, maxLength: 128),
                        affliation = c.String(),
                        Article_PMID = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => new { t.firstName, t.lastName, t.initials })
                .ForeignKey("dbo.Article", t => t.Article_PMID)
                .Index(t => t.Article_PMID);
            
            CreateTable(
                "dbo.Journal",
                c => new
                    {
                        nameAbbreviation = c.String(nullable: false, maxLength: 128),
                        name = c.String(),
                        impactFactor = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.nameAbbreviation);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Article", "journal_nameAbbreviation", "dbo.Journal");
            DropForeignKey("dbo.Author", "Article_PMID", "dbo.Article");
            DropIndex("dbo.Author", new[] { "Article_PMID" });
            DropIndex("dbo.Article", new[] { "journal_nameAbbreviation" });
            DropTable("dbo.Journal");
            DropTable("dbo.Author");
            DropTable("dbo.Article");
        }
    }
}
