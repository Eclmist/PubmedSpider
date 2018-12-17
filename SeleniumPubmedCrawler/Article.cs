using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumPubmedCrawler
{
    class Article
    {
        [Key] public string PMID { get; set; }
        public string authors { get; set; }
        public string title { get; set; }
        public string abstractTxt { get; set; }
        public string journal { get; set; }
        public DateTime date { get; set; }

        public string doi { get; set; }
        public string pii { get; set; }

        public override string ToString()
        {
            return title + "(" + journal + ") by " + authors + " PMID: " + PMID;
        }
    }
}
