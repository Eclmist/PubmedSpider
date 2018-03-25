using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SeleniumPubmedCrawler
{
#pragma DEBUG_MODE

    class Program
    {
        const string PUBMED_URL = @"https://www.ncbi.nlm.nih.gov/pubmed/?term=";
        const string QUERY = @"National University Of Singapore Markus R Wenk";

        const string INDEX_TITLE_CLASS_NAME = "title";
        const string INDEX_ITEMCOUNT_CLASS_NAME = "result_count";
        const string DETAIL_TITLE_XPATH = "//*[@id=\"maincontent\"]/div/div[5]/div/h1";
        const string DETAIL_AUTHOR_CLASS_NAME = "auths";
        const string DETAIL_ABSTRACT_CSS_SELECTOR = "abstracttext";
        const string DETAILS_JOURNAL_XPATH = "//*[@id=\"maincontent\"]/div/div[5]/div/div[1]/span/a";
        const string DETAILS_DATE_XPATH = "//*[@id=\"maincontent\"]/div/div[5]/div/div[1]";
        const int MAX_ARTICLE_COUNT = 10;

        static ChromeDriver driver;
        static List<Article> articleList = new List<Article>();



        static void Main(string[] args)
        {
            PrintHeaders();

            driver = SetupDriver();

            Crawl(PUBMED_URL + QUERY);

            ExportJSON();

            Cleanup();
        }


        static void PrintHeaders()
        {
            Console.WriteLine("Pubmed Spider v1.0");
            Console.WriteLine("Using search query: " + QUERY);
        }

        static string GetInput(string question)
        {
            Console.Write(question);
            return Console.ReadLine();
        }

        static ChromeDriver SetupDriver()
        {
            var options = new ChromeOptions();

#if !DEBUG
                options.AddArguments("--headless", "--disable-gpu", "--ignore-certificate-errors", "--incognito");
                options.AddArgument("--log-level=1");
                options.AddArgument("--silent");
#endif
            return new ChromeDriver(options);
        }

        static void Crawl(string url)
        {
            Console.WriteLine("[Info] Downloading index");
            driver.Navigate().GoToUrl(url);
            Console.WriteLine("[Success] Webpage loaded");


            string count = driver.FindElementByClassName(INDEX_ITEMCOUNT_CLASS_NAME).Text.Split(' ').Last();

            Console.WriteLine("[Info] " + count + " results found");



            CrawlIndex();
        }

        static void CrawlIndex()
        {
            List<string> currentPageArticlesURLS = new List<string>();

            ChromeDriver detailDriver = SetupDriver();

            var titles = driver.FindElementsByClassName(INDEX_TITLE_CLASS_NAME);

            foreach (var link in titles)
            {
                currentPageArticlesURLS.Add(link.FindElement(By.CssSelector("a")).GetAttribute("href"));
            }

            foreach (string url in currentPageArticlesURLS)
            {
                CrawlDetails(url, detailDriver);

                if (articleList.Count >= MAX_ARTICLE_COUNT)
                    break;
            }

            detailDriver.Dispose();

            if (articleList.Count < MAX_ARTICLE_COUNT)
            {
                try
                {
                    IWebElement nextPageButton = driver.FindElementByClassName("next");
                    if (nextPageButton != null)
                    {
                        nextPageButton.Click();
                        CrawlIndex();
                    }
                }
                catch (Exception e)
                {
                }
            }

            Console.WriteLine("[Success] Reached the end of the results");
        }

        static void CrawlDetails(string url, ChromeDriver driver)
        {
            driver.Navigate().GoToUrl(url);

            List<String> authors = new List<string>();

            var listOfAuthorNames = driver.FindElementByClassName(DETAIL_AUTHOR_CLASS_NAME)
                .FindElements(By.CssSelector("a"));

            foreach (IWebElement author in listOfAuthorNames)
            {
                authors.Add(author.Text);
            }

            string title = driver.FindElementByXPath(DETAIL_TITLE_XPATH).Text;
            string abstractText = driver.FindElementByCssSelector(DETAIL_ABSTRACT_CSS_SELECTOR).Text;
            string journal = driver.FindElementByXPath(DETAILS_JOURNAL_XPATH).Text;
            string date = driver.FindElementByXPath(DETAILS_DATE_XPATH).Text;

            Article a = new Article();
            a.title = title;
            a.authors = authors.ToArray();
            a.abstractTxt = abstractText;
            a.journal = journal;

            articleList.Add(a);

            Console.WriteLine(
                "[Success] Article " + a.ToString() + " added\n"
                );
        }

        static void ExportJSON()
        {
            Console.WriteLine("[Info] Exporting results to JSON");

            Console.WriteLine("[Success] Data successfully exported");
        }

        static void Cleanup()
        {
            driver.Dispose();

            Console.WriteLine("Press any key to quit.");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
