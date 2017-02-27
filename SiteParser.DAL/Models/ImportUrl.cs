using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SiteParser.DAL.Models
{
    public class ImportUrl : Entity
    {
        [NotMapped]
        public int Id { get; set; }
        [Key]
        public string Name { get; private set; }

        public ImportUrl() { }

        public ImportUrl(string name)
        {
            Name = name;
        }
    }
}
