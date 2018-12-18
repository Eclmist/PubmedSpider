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
                        pubmedID = c.String(nullable: false, maxLength: 128),
                        title = c.String(),
                        abstractTxt = c.String(),
                        date = c.DateTime(nullable: false),
                        doi = c.String(),
                        pii = c.String(),
                        journal_nlmUniqueID = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.pubmedID)
                .ForeignKey("dbo.Journal", t => t.journal_nlmUniqueID)
                .Index(t => t.journal_nlmUniqueID);
            
            CreateTable(
                "dbo.Author",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        firstName = c.String(),
                        lastName = c.String(),
                        initials = c.String(),
                        affliation = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Journal",
                c => new
                    {
                        nlmUniqueID = c.String(nullable: false, maxLength: 128),
                        name = c.String(),
                        nameAbbreviation = c.String(),
                        impactFactor = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.nlmUniqueID);
            
            CreateTable(
                "dbo.AuthorArticles",
                c => new
                    {
                        Author_ID = c.Guid(nullable: false),
                        Article_pubmedID = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Author_ID, t.Article_pubmedID })
                .ForeignKey("dbo.Author", t => t.Author_ID, cascadeDelete: true)
                .ForeignKey("dbo.Article", t => t.Article_pubmedID, cascadeDelete: true)
                .Index(t => t.Author_ID)
                .Index(t => t.Article_pubmedID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Article", "journal_nlmUniqueID", "dbo.Journal");
            DropForeignKey("dbo.AuthorArticles", "Article_pubmedID", "dbo.Article");
            DropForeignKey("dbo.AuthorArticles", "Author_ID", "dbo.Author");
            DropIndex("dbo.AuthorArticles", new[] { "Article_pubmedID" });
            DropIndex("dbo.AuthorArticles", new[] { "Author_ID" });
            DropIndex("dbo.Article", new[] { "journal_nlmUniqueID" });
            DropTable("dbo.AuthorArticles");
            DropTable("dbo.Journal");
            DropTable("dbo.Author");
            DropTable("dbo.Article");
        }
    }
}
