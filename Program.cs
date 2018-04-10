using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace SeleniumPubmedCrawler
{

    class Program
    {

        static ChromeDriver driver;
        static List<Article> articleList = new List<Article>();

        static String[] journalsThatMatters;

        static int perQueryCounter = 0;

        static void Main(string[] args)
        {
            PrintHeaders();

            driver = SetupDriver();

            journalsThatMatters = GetJournalsThatMatters();
            
            if (journalsThatMatters.Length <= 0)
            {
                Console.WriteLine("[Error] IF.txt empty! Exiting..");
                Cleanup();
                return;
            }

            Crawl(Constants.PUBMED_URL + Constants.QUERY_PREFIX);

            ExportJSON();

            Cleanup();
        }


        static void PrintHeaders()
        {
            Console.WriteLine("Pubmed Spider");
        }

        static string GetInput(string question)
        {
            Console.Write(question);
            return Console.ReadLine();
        }

        static ChromeDriver SetupDriver()
        {
            var options = new ChromeOptions();

            // ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            // service.SuppressInitialDiagnosticInformation = true;


#if !DEBUG
                options.AddArguments("--headless", "--disable-gpu", "--ignore-certificate-errors", "--incognito");
                options.AddArgument("--log-level=3");
#endif

            return new ChromeDriver(Directory.GetCurrentDirectory(), options);
        }

        static void Crawl(string url)
        {
            string[] names = GetNamesToCrawl();

            if (names.Length == 0)
                Console.WriteLine("[Error] Name.txt empty! Exiting..");

            foreach (string name in names)
            {
                string query = url + name + "[Author - First])";

                Console.WriteLine(query);
                
                driver.Navigate().GoToUrl(query);
                Console.WriteLine("[Success] Webpage loaded");

                perQueryCounter = 0;
                CrawlIndex();
            }
        }

        static string[] GetNamesToCrawl()
        {
            string[] lines = { };

            try
            {
                lines = System.IO.File.ReadAllLines(@"Names.txt");
            }
            catch
            {
            
                Console.WriteLine("[Error] Names.txt does not exist! Creating empty Names.txt file.");
                System.IO.File.Create(@"Names.txt");
            }

            return lines;
        }

        static string[] GetJournalsThatMatters()
        {

            string[] lines = { };

            try
            {
                lines = System.IO.File.ReadAllLines(@"IF.txt");
            }
            catch
            {
            
                Console.WriteLine("[Error] IF.txt does not exist! Creating empty IF.txt file.");
                System.IO.File.Create(@"IF.txt");
            }

            return lines;
        }

        static void CrawlIndex()
        {
            List<string> currentPageArticlesURLS = new List<string>();

            ChromeDriver detailDriver = SetupDriver();

            var titles = driver.FindElementsByClassName(Constants.INDEX_TITLE_CLASS_NAME);

            foreach (var link in titles)
            {
                // check if is part of journals that matters
                var followingSibling = link.FindElement(By.XPath("following-sibling::*")).FindElement(By.XPath("p[2]/span")).Text;

                foreach(string j in journalsThatMatters)
                {
                    if (followingSibling == j)
                    {
                        currentPageArticlesURLS.Add(link.FindElement(By.CssSelector("a")).GetAttribute("href"));
                        break;
                    }
                }
            }

            foreach (string url in currentPageArticlesURLS)
            {
                CrawlDetails(url, detailDriver);
                
                perQueryCounter++;

                if (perQueryCounter >= Constants.MAX_ARTICLE_COUNT_PER_QUERY)
                    break;
            }

            detailDriver.Dispose();

            if (!(perQueryCounter >= Constants.MAX_ARTICLE_COUNT_PER_QUERY))
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
                catch
                {
                    // cannot find next button, ignore error.
                    Console.WriteLine("[Success] Reached the end of the results");
                }
            }

            Console.WriteLine("[Success] Reached max article per query count");
        }

        // returns true if article has sufficient impact factor
        static void CrawlDetails(string url, ChromeDriver driver)
        {
            driver.Navigate().GoToUrl(url);

            List<String> authors = new List<string>();

            var listOfAuthorNames = driver.FindElementByClassName(Constants.DETAIL_AUTHOR_CLASS_NAME)
                .FindElements(By.CssSelector("a"));

            foreach (IWebElement author in listOfAuthorNames)
            {
                authors.Add(author.Text);
            }

            string title = driver.FindElementByXPath(Constants.DETAIL_TITLE_XPATH).Text;
            string abstractText = driver.FindElementByCssSelector(Constants.DETAIL_ABSTRACT_CSS_SELECTOR).Text;
            string journal = driver.FindElementByXPath(Constants.DETAILS_JOURNAL_XPATH).Text.Split('.')[0];
            string date = driver.FindElementByXPath(Constants.DETAILS_DATE_XPATH).Text.Split('.')[1].TrimStart(' ');

            Article a = new Article();
            a.title = title;
            a.authors = authors.ToArray();
            a.abstractTxt = abstractText;
            a.journal = journal;
            a.date = date;

            articleList.Add(a);

            Console.WriteLine("[Success] Article " + a.ToString() + " added, from " + journal);
        }

        static void ExportJSON()
        {
            Console.WriteLine("\n[Info] Exporting results to JSON");

            if (!Directory.Exists("dump")) 
                Directory.CreateDirectory("dump");

            using (StreamWriter file = File.CreateText(@"dump\dump_" + DateTime.Now.ToString("yyyy_MM_dd_HHmm") + ".json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(file, articleList);
            }

            Console.WriteLine("[Success] Data successfully exported");
        }

        static void Cleanup()
        {
            driver.Dispose();

            Console.WriteLine("Press any key to quit.");
            Console.Read();
            Environment.Exit(0);
        }
    }
}