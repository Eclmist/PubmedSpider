using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumPubmedCrawler
{
    class Constants
    {
        public const string PUBMED_URL = @"https://www.ncbi.nlm.nih.gov/pubmed/?term=";
        public const string QUERY_PREFIX = @"(National University Of Singapore[Affiliation] AND ";

        // Number of articles to crawl before stopping (per name)
        public const int MAX_ARTICLE_COUNT_PER_QUERY = 2;

        // Names (temp list)
        // public static readonly string[] tempNames = { "Markus R Wenk", "Wong Wai-Shiu, Fred" };
        
        public const string INDEX_TITLE_CLASS_NAME = "title";
        public const string INDEX_ITEMCOUNT_CLASS_NAME = "result_count";
        public const string DETAIL_TITLE_XPATH = "//*[@id=\"maincontent\"]/div/div[5]/div/h1";
        public const string DETAIL_AUTHOR_CLASS_NAME = "auths";
        public const string DETAIL_ABSTRACT_CSS_SELECTOR = "abstracttext";
        public const string DETAILS_JOURNAL_XPATH = "//*[@id=\"maincontent\"]/div/div[5]/div/div[1]/span/a";
        public const string DETAILS_DATE_XPATH = "//*[@id=\"maincontent\"]/div/div[5]/div/div[1]";
    }
}
