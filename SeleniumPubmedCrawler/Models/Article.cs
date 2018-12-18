using SeleniumPubmedCrawler.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeleniumPubmedCrawler
{
    [Table("Article")]
    public class Article
    {
        [Key] public string pubmedID { get; set; }
        public List<Author> authors { get; set; }
        public string title { get; set; }
        public string abstractTxt { get; set; }

        [ForeignKey("journalID")]
        public Journal journal { get; set; }
        [Required]
        public string journalID { get; set; }

        public DateTime publicationDate { get; set; }
        public DateTime dateCrawled { get; set; }

        public string doi { get; set; }
        public string pii { get; set; }

        public Article()
        {
            dateCrawled = DateTime.Now;
        }

        public override string ToString()
        {
            return
                pubmedID + " - " +
                title.Substring(0, 36) +
                "... (" + journal.nameAbbreviation +
                ") by " + authors[0].lastName + " and friends";
        }

        public override bool Equals(object obj)
        {
            var other = obj as Article;
            if (other == null)
            {
                return false;
            }
            return pubmedID.Equals(other.pubmedID);
        }

        public override int GetHashCode()
        {
            var hashCode = 1977392251;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(pubmedID);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<Author>>.Default.GetHashCode(authors);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(title);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(abstractTxt);
            hashCode = hashCode * -1521134295 + EqualityComparer<Journal>.Default.GetHashCode(journal);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(journalID);
            hashCode = hashCode * -1521134295 + publicationDate.GetHashCode();
            hashCode = hashCode * -1521134295 + dateCrawled.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(doi);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(pii);
            return hashCode;
        }
    }
}
