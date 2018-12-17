using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
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
        static List<Article> articleList = new List<Article>();

        static int perQueryCounter = 0;

        static void Main(string[] args)
        {
            PrintHeaders();

            driver = SetupDriver();

            Crawl(Constants.PUBMED_URL + Constants.QUERY_PREFIX);

            ExportJSON();

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

                    Article a = new Article
                    {
                        PMID = node.SelectSingleNode("//MedlineCitation/PMID").InnerText,
                        journal = node.SelectSingleNode("//MedlineCitation/Article/Journal/Title").InnerText,
                        title = node.SelectSingleNode("//MedlineCitation/Article/ArticleTitle").InnerText,
                        abstractTxt = node.SelectSingleNode("//MedlineCitation/Article/Abstract").InnerText
                    };

                    // Dates
                    string pubYear = node.SelectSingleNode("//MedlineCitation/Article/ArticleDate/Year").InnerText;
                    string pubMonth = node.SelectSingleNode("//MedlineCitation/Article/ArticleDate/Month").InnerText;
                    string pubDay = node.SelectSingleNode("//MedlineCitation/Article/ArticleDate/Day").InnerText;

                    a.date = new DateTime(int.Parse(pubYear), int.Parse(pubMonth), int.Parse(pubDay));

                    // doi and whatnot
                    XmlNodeList elocation = node.SelectNodes("//MedlineCitation/Article/ELocationID");
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
                    a.doi = doi;
                    a.pii = pii;

                    // Authors
                    XmlNodeList authors = node.SelectNodes("//MedlineCitation/Article/AuthorList/Author");
                    string authorString = "";
                    foreach (XmlNode author in authors)
                    {
                        string LastName = author.ChildNodes[0].InnerText;
                        if (author.FirstChild.Name == "CollectiveName")
                        {
                            authorString += LastName + ";";
                            continue;
                        }

                        string firstName = author.ChildNodes[1].InnerText;

                        // Potentially store affliation per author here
                        // //AffiliationInfo/Affiliation -> Dept. of Bio Sciences, NUS, Singapore, Singapore

                        authorString += firstName + " " + LastName + ";";
                        //                                            ^ delimiter
                    }

                    a.authors = authorString;

                    articleList.Add(a);

                    Console.WriteLine("[Success] Article " + a.ToString() + " added");
                    return;
                }
            }
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
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
