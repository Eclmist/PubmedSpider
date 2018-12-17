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
    }
}
