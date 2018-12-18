using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeleniumPubmedCrawler.Models
{
    [Table("Journal")]
    public class Journal
    {
        [Key] public string nlmUniqueID { get; set; }
        public string name { get; set; }
        public string nameAbbreviation { get; set; }
        public float impactFactor { get; set; }
        public List<Article> articles { get; set; }

        public override bool Equals(object obj)
        {
            Journal other = obj as Journal;
            if (other == null)
            {
                return false;
            }
            return nlmUniqueID.Equals(other.nlmUniqueID);
        }

        public override int GetHashCode()
        {
            var hashCode = -1507036798;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(nlmUniqueID);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(nameAbbreviation);
            hashCode = hashCode * -1521134295 + impactFactor.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<List<Article>>.Default.GetHashCode(articles);
            return hashCode;
        }
    }
}
