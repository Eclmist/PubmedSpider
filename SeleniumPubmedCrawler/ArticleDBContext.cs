using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace SeleniumPubmedCrawler
{
    class ArticleDBContext : DbContext
    {
        public DbSet<Article> Articles { get; set; }
    }
}
