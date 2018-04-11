using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumPubmedCrawler
{
    class Constants
    {
        internal static readonly string PUBMED_URL = @"https://www.ncbi.nlm.nih.gov/pubmed/?term=";
        internal static readonly string QUERY_PREFIX = @"(National University Of Singapore[Affiliation] AND ";

        // Number of articles to crawl before stopping (per name)
        public const int MAX_ARTICLE_COUNT_PER_QUERY = 50;

        // Names (temp list)
        // public static readonly string[] tempNames = { "Markus R Wenk", "Wong Wai-Shiu, Fred" };
        
        internal static readonly string INDEX_TITLE_CLASS_NAME = "title";
        internal static readonly string INDEX_ITEMCOUNT_CLASS_NAME = "result_count";
        internal static readonly string DETAIL_TITLE_XPATH = "//*[@id=\"maincontent\"]/div/div[5]/div/h1";
        internal static readonly string DETAIL_AUTHOR_CLASS_NAME = "auths";
        internal static readonly string DETAIL_AUTHOR_XPATH = "//*[@id=\"maincontent\"]/div/div[5]/div/div[2]/a";
        internal static readonly string DETAIL_ABSTRACT_CSS_SELECTOR = "abstracttext";
        internal static readonly string DETAILS_JOURNAL_XPATH = "//*[@id=\"maincontent\"]/div/div[5]/div/div[1]/span/a";
        internal static readonly string DETAILS_DATE_XPATH = "//*[@id=\"maincontent\"]/div/div[5]/div/div[1]";
        internal static readonly string DETAIL_ABSTRACT_XPATH = "//*[@id=\"maincontent\"]/div/div[5]/div/div[4]/div/p/abstracttext";
        internal static readonly string INDEX_JOURNAL_CLASS_NAME = "jrnl";
    }
}
