using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumPubmedCrawler.Models
{
    [Table("Journal")]
    public class Journal
    {
        public string name { get; set; }
        [Key()] public string nameAbbreviation { get; set; }
        public float impactFactor { get; set; }
        public List<Article> articles { get; set; }
    }
}
