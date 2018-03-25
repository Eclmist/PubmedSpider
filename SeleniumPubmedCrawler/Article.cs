using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumPubmedCrawler
{
    struct Article
    {
        public string[] authors;
        public string title;
        public string abstractTxt;
        public string journal;

        public string date;

        public override string ToString()
        {
            return title + "(" + journal + ") by " + authors[0] + " and friends";
        }
    }
}
