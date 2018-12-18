namespace SeleniumPubmedCrawler.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class deployment2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Article", "journal_nlmUniqueID", "dbo.Journal");
            DropIndex("dbo.Article", new[] { "journal_nlmUniqueID" });
            RenameColumn(table: "dbo.AuthorArticles", name: "Author_ID", newName: "ArticlePubmedID");
            RenameColumn(table: "dbo.AuthorArticles", name: "Article_pubmedID", newName: "AuthorID");
            RenameColumn(table: "dbo.Article", name: "journal_nlmUniqueID", newName: "journalID");
            RenameIndex(table: "dbo.AuthorArticles", name: "IX_Article_pubmedID", newName: "IX_AuthorID");
            RenameIndex(table: "dbo.AuthorArticles", name: "IX_Author_ID", newName: "IX_ArticlePubmedID");
            DropPrimaryKey("dbo.AuthorArticles");
            AlterColumn("dbo.Article", "journalID", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.AuthorArticles", new[] { "AuthorID", "ArticlePubmedID" });
            CreateIndex("dbo.Article", "journalID");
            AddForeignKey("dbo.Article", "journalID", "dbo.Journal", "nlmUniqueID", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Article", "journalID", "dbo.Journal");
            DropIndex("dbo.Article", new[] { "journalID" });
            DropPrimaryKey("dbo.AuthorArticles");
            AlterColumn("dbo.Article", "journalID", c => c.String(maxLength: 128));
            AddPrimaryKey("dbo.AuthorArticles", new[] { "Author_ID", "Article_pubmedID" });
            RenameIndex(table: "dbo.AuthorArticles", name: "IX_ArticlePubmedID", newName: "IX_Author_ID");
            RenameIndex(table: "dbo.AuthorArticles", name: "IX_AuthorID", newName: "IX_Article_pubmedID");
            RenameColumn(table: "dbo.Article", name: "journalID", newName: "journal_nlmUniqueID");
            RenameColumn(table: "dbo.AuthorArticles", name: "AuthorID", newName: "Article_pubmedID");
            RenameColumn(table: "dbo.AuthorArticles", name: "ArticlePubmedID", newName: "Author_ID");
            CreateIndex("dbo.Article", "journal_nlmUniqueID");
            AddForeignKey("dbo.Article", "journal_nlmUniqueID", "dbo.Journal", "nlmUniqueID");
        }
    }
}
