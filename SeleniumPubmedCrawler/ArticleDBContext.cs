using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using SeleniumPubmedCrawler.Models;

namespace SeleniumPubmedCrawler
{
    public class ArticleDBContext : DbContext
    {
        public ArticleDBContext() : base("name=ArticleDBContext") {}
        public DbSet<Article> Article { get; set; }
        public DbSet<Journal> Journal { get; set; }
        public DbSet<Author> Author { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Article>()
                .HasMany(t => t.authors)
                .WithMany(t => t.articles)
                .Map(m =>
                {
                    m.ToTable("AuthorArticles");
                    m.MapLeftKey("AuthorID");
                    m.MapRightKey("ArticlePubmedID");
                });

            base.OnModelCreating(modelBuilder);
        }
    }
}
