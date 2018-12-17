using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SeleniumPubmedCrawler.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;

namespace SeleniumPubmedCrawler
{

    class Program
    {
        static ChromeDriver driver;
        static List<Article> masterArticleList = new List<Article>();
        static List<Journal> masterJournalList = new List<Journal>();
        static List<Author> masterAuthorList = new List<Author>();

        static int perQueryCounter = 0;

        static void Main(string[] args)
        {
            PrintHeaders();

            driver = SetupDriver();

            Crawl(Constants.PUBMED_URL + Constants.QUERY_PREFIX);

            saveToDb();

            Cleanup();
        }


        static void PrintHeaders()
        {
            Console.WriteLine("Pubmed Spider");
            Console.WriteLine("Using search query: " + Constants.QUERY_PREFIX);
        }

        static string GetInput(string question)
        {
            Console.Write(question);
            return Console.ReadLine();
        }

        static ChromeDriver SetupDriver()
        {
            var options = new ChromeOptions();

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;


#if !DEBUG
                options.AddArguments("--headless", "--disable-gpu", "--ignore-certificate-errors", "--incognito");
                options.AddArgument("--log-level=3");
#endif
            return new ChromeDriver(service, options);
        }

        static void Crawl(string url)
        {
            string[] names = GetNamesToCrawl();

            foreach (string name in names)
            {
                Console.WriteLine("\n[Info] Downloading index for " + name);
                driver.Navigate().GoToUrl(url + " " + name);
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
            catch (FileNotFoundException e)
            {
                Console.WriteLine("[Error] Names.txt is empty!");
                System.IO.File.Create(@"Names.txt");
            }

            return lines;
        }

        static void CrawlIndex()
        {
            List<string> currentPageArticlesURLS = new List<string>();

            var titles = driver.FindElementsByClassName(Constants.INDEX_TITLE_CLASS_NAME);

            foreach (var link in titles)
            {
                currentPageArticlesURLS.Add(link.FindElement(By.CssSelector("a")).GetAttribute("href"));
            }

            foreach (string url in currentPageArticlesURLS)
            {
                try
                {
                    CrawlDetails(url);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                perQueryCounter++;

                if (perQueryCounter >= Constants.MAX_ARTICLE_COUNT_PER_QUERY)
                    break;
            }

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
                catch (Exception e)
                {
                    // cannot find next button, ignore error.
                    Console.WriteLine("[Success] Reached the end of the results");
                }
            }

            Console.WriteLine("[Success] Reached max article per query count");
        }

        static void CrawlDetails(string url)
        {
            string xmlPath = url + "?report=xml&format=text";
            // download xml
            string xmlString;
            using (var wc = new WebClient())
            {
                xmlString = wc.DownloadString(xmlPath);
                // HACKKKKK I WASTED 3 HOURS HERE SOMEONE HALP ME
                xmlString = xmlString.Replace("&lt;", "<");
                xmlString = xmlString.Replace("&gt;", ">");
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);

            foreach(XmlNode node in doc.DocumentElement)
            {
                if (node.Name == "PubmedArticle")
                {
                    Journal journal = new Journal();
                    journal.name = node.SelectSingleNode(Constants.XPATH_JOURNAL).InnerText;
                    journal.nameAbbreviation = node.SelectSingleNode(Constants.XPATH_JOURNAL_ABBREV).InnerText;

                    Article article = new Article();
                    article.PMID = node.SelectSingleNode(Constants.XPATH_PMID).InnerText;
                    article.title = node.SelectSingleNode(Constants.XPATH_ARTICLE_TITLE).InnerText;
                    article.abstractTxt = node.SelectSingleNode(Constants.XPATH_ARTICLE_ABSTRACT).InnerText;
                    article.journal = journal;

                    // Dates
                    string pubYear = node.SelectSingleNode(Constants.XPATH_ARTICLE_DATE_YEAR).InnerText;
                    string pubMonth = node.SelectSingleNode(Constants.XPATH_ARTICLE_DATE_MONTH).InnerText;
                    string pubDay = node.SelectSingleNode(Constants.XPATH_ARTICLE_DATE_DAY).InnerText;

                    article.date = new DateTime(int.Parse(pubYear), int.Parse(pubMonth), int.Parse(pubDay));

                    // doi and whatnot
                    XmlNodeList elocation = node.SelectNodes(Constants.XPATH_ELOCATION);
                    string pii = "", doi = "";
                    foreach (XmlNode eLoc in elocation)
                    {
                        string attrib = eLoc.Attributes[0].InnerText;
                        if (attrib == "pii")
                        {
                            pii = eLoc.InnerText;
                        }
                        else if (attrib == "doi")
                        {
                            doi = eLoc.InnerText;
                        }
                    }
                    article.doi = doi;
                    article.pii = pii;

                    // Authors
                    XmlNodeList authors = node.SelectNodes(Constants.XPATH_ARTICLE_AUTHORS);
                    List<Author> aList = new List<Author>();
                    foreach (XmlNode author in authors)
                    {
                        Author a = new Author();
                        aList.Add(a);
                        masterAuthorList.Add(a);
                        a.lastName = author.ChildNodes[0].InnerText;
                        if (author.FirstChild.Name == "CollectiveName") { continue; }
                        a.firstName = author.ChildNodes[1].InnerText;
                        a.initials = author.ChildNodes[2].InnerText;
                        a.affliation = author.ChildNodes[3].FirstChild.InnerText;
                    }

                    article.authors = aList;
                    masterArticleList.Add(article);

                    Console.WriteLine("[Success] Article " + article.ToString() + " added");
                    return;
                }
            }
        }

        static void saveToDb()
        {
            Console.WriteLine("\n[Info] Saving to database");
            ArticleDBContext context = new ArticleDBContext();
            context.Article.AddRange(masterArticleList);
            context.Author.AddRange(masterAuthorList);
            context.Journal.AddRange(masterJournalList);
            Console.WriteLine("[Success] Data successfully saved to database");
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
