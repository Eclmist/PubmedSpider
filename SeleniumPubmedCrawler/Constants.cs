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
        public const string QUERY_PREFIX = @"National University Of Singapore";

        // Number of articles to crawl before stopping (per name)
        public const int MAX_ARTICLE_COUNT_PER_QUERY = 5;

        // Names (temp list)
        // public static readonly string[] tempNames = { "Markus R Wenk", "Wong Wai-Shiu, Fred" };

        public const string INDEX_TITLE_CLASS_NAME = "title";

        public const string XPATH_PMID = "//MedlineCitation/PMID";
        public const string XPATH_JOURNAL = "//MedlineCitation/Article/Journal/Title";
        public const string XPATH_JOURNAL_ABBREV = "//MedlineCitation/Article/Journal/ISOAbbreviation";
        public const string XPATH_JOURNAL_ID = "//MedlineCitation/MedlineJournalInfo/NlmUniqueID";
        public const string XPATH_ARTICLE_AUTHORS = "//MedlineCitation/Article/AuthorList/Author";
        public const string XPATH_ARTICLE_TITLE = "//MedlineCitation/Article/ArticleTitle";
        public const string XPATH_ELOCATION = "//MedlineCitation/Article/ELocationID";
        public const string XPATH_ARTICLE_ABSTRACT = "//MedlineCitation/Article/Abstract";

        public const string XPATH_ARTICLE_DATE_YEAR_LOC1 = "//MedlineCitation/Article/Journal/JournalIssue/PubDate/Year";
        public const string XPATH_ARTICLE_DATE_MONTH_LOC1 = "//MedlineCitation/Article/Journal/JournalIssue/PubDate/Month";
        public const string XPATH_ARTICLE_DATE_DAY_LOC1 = "//MedlineCitation/Article/Journal/JournalIssue/PubDate/Day";
        public const string XPATH_ARTICLE_DATE_YEAR_LOC2 = "//MedlineCitation/Article/ArticleDate/Year";
        public const string XPATH_ARTICLE_DATE_MONTH_LOC2 = "//MedlineCitation/Article/ArticleDate/Month";
        public const string XPATH_ARTICLE_DATE_DAY_LOC2 = "//MedlineCitation/Article/ArticleDate/Day";

        public static readonly string[] KNOWN_DATE_FORMATS = {
            "dd-MMM-yyyy",
            "dd-mm-yyyy",
            "dd-mm-yy",
            "dd-MMM-yy"
        };
    }
}
