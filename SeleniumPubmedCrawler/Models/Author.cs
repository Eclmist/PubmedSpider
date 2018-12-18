using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumPubmedCrawler.Models
{
    [Table("Author")]
    public class Author
    {
        [Key] public Guid ID { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string initials { get; set; }
        public string affliation { get; set; }
        public List<Article> articles { get; set; }

        public Author (string fn, string ln)
        {
            ID = Guid.NewGuid();
            firstName = fn;
            lastName = ln;
        }

        public Author ()
        {
            ID = Guid.NewGuid();
        }

        public override bool Equals(object obj)
        {
            var other = obj as Author;

            if (other == null)
            {
                return false;
            }

            return ID.Equals(other.ID) || 
                (firstName == other.firstName && lastName == other.lastName);
        }

        public override int GetHashCode()
        {
            var hashCode = 2132695694;
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid>.Default.GetHashCode(ID);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(firstName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(lastName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(initials);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(affliation);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<Article>>.Default.GetHashCode(articles);
            return hashCode;
        }
    }
}
