using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SeleniumPubmedCrawler.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
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

        static ArticleDBContext dbContext = new ArticleDBContext();

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
                    Console.WriteLine("[Error] " + e.Message + " on article " + url);
                    continue;
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
                    string pubmedID = node.SelectSingleNode(Constants.XPATH_PMID).InnerText;

                    // if we somehow crawled this article before during this current execution
                    if (masterArticleList.Find(x => x.pubmedID == pubmedID) != null)
                    {
                        continue;
                    }

                    string journalNlmUniqueID = node.SelectSingleNode(Constants.XPATH_JOURNAL_ID).InnerText;
                    // check if we have a journal already
                    Journal journal = masterJournalList.Find(x => x.nlmUniqueID == journalNlmUniqueID);

                    if (journal == null)
                    {
                        journal = new Journal();
                        journal.nlmUniqueID = journalNlmUniqueID;
                        journal.name = node.SelectSingleNode(Constants.XPATH_JOURNAL).InnerText;
                        journal.nameAbbreviation = node.SelectSingleNode(Constants.XPATH_JOURNAL_ABBREV).InnerText;
                        masterJournalList.Add(journal);
                    }

                    Article article = new Article();
                    article.pubmedID = pubmedID;
                    article.title = node.SelectSingleNode(Constants.XPATH_ARTICLE_TITLE).InnerText;
                    article.abstractTxt = node.SelectSingleNode(Constants.XPATH_ARTICLE_ABSTRACT).InnerText;
                    article.journal = journal;
                    article.journalID = journal.nlmUniqueID;

                    // Dates
                    article.publicationDate = parseDisgustingDates(node);

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
                        a.lastName = author.ChildNodes[0].InnerText;
                        if (author.FirstChild.Name == "CollectiveName") { continue; }
                        a.firstName = author.ChildNodes[1].InnerText;
                        a.initials = author.ChildNodes[2].InnerText;
                        a.affliation = author.ChildNodes[3].FirstChild.InnerText;

                        if (masterAuthorList.Contains(a))
                        {
                            a = masterAuthorList.Find(x => x.Equals(a));
                        }
                        else
                        {
                            masterAuthorList.Add(a);
                        }

                        aList.Add(a);
                    }

                    article.authors = aList;
                    masterArticleList.Add(article);

                    Console.WriteLine("[Success] Article " + article.ToString() + " added");
                    return;
                }
            }
        }

        static DateTime parseDisgustingDates(XmlNode node)
        {
            string dateTimeString = "";

            // try get journal publication month/year
            try
            {
                // some don't include day
                string day;

                try
                {
                    day = node.SelectSingleNode(Constants.XPATH_ARTICLE_DATE_DAY_LOC1).InnerText;
                }
                catch
                {
                    day = "01";
                }

                dateTimeString += day + "-";
                dateTimeString += node.SelectSingleNode(Constants.XPATH_ARTICLE_DATE_MONTH_LOC1).InnerText + "-";
                dateTimeString += node.SelectSingleNode(Constants.XPATH_ARTICLE_DATE_YEAR_LOC1).InnerText;

            }
            catch
            {
                dateTimeString = "";
                dateTimeString += node.SelectSingleNode(Constants.XPATH_ARTICLE_DATE_DAY_LOC2).InnerText + "-";
                dateTimeString += node.SelectSingleNode(Constants.XPATH_ARTICLE_DATE_MONTH_LOC2).InnerText + "-";
                dateTimeString += node.SelectSingleNode(Constants.XPATH_ARTICLE_DATE_YEAR_LOC2).InnerText;
            }

            // actually parsing the damn thing into a date time

            foreach (string format in Constants.KNOWN_DATE_FORMATS)
            {
                try
                {
                    return DateTime.ParseExact(dateTimeString, format, CultureInfo.InvariantCulture);
                }
                catch
                {
                    continue;
                }
            }

            Console.WriteLine("[Info] Gave up parsing date \"" + dateTimeString + "\" and will substitute with current time.");
            Console.WriteLine("\tYou may want to add this format into the array of known formats");

            return DateTime.Now;
        }

        static void saveToDb()
        {
            Console.WriteLine("\n[Info] Saving to database... ");
            try
            {
                // remove all articles that already exist in db
                masterArticleList.RemoveAll(x => dbContext.Article.Any(o => o.pubmedID == x.pubmedID));

                foreach (Article article in masterArticleList)
                {

                    // make sure no dup authors
                    for (int i = 0; i < article.authors.Count; i++)
                    {
                        string fn = article.authors[i].firstName;
                        string ln = article.authors[i].lastName;
                        Author existingRecord = dbContext.Author
                            .Where(a => a.firstName == fn && a.lastName == ln)
                            .FirstOrDefault();

                        if (existingRecord != null)
                        {
                            // make sure it doesn't make a new record
                            article.authors[i] = existingRecord;
                        }
                    }

                    // make sure no dup journals
                    Journal dbJournal = dbContext.Journal.Find(article.journal.nlmUniqueID);

                    if (dbJournal != null)
                    {
                        article.journal = dbJournal;
                    }
                }

                dbContext.Article.AddRange(masterArticleList);
                dbContext.SaveChanges();
                Console.WriteLine("[Success] Data successfully saved to database");
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("[SQL Error] Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
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
