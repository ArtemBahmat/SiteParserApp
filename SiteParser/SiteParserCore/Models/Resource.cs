using System.ComponentModel.DataAnnotations.Schema;

namespace SiteParserCore.Models
{
    public enum ResourceType
    {
        Image,
        Css
    }

    public class Resource
    {
        public int Id { get; set; }
        public int ParentSiteId { get; set; }
        public string Name { get; set; }
        public ResourceType Type { get; set; }
        public string ParentName { get; set; }
        [NotMapped]
        public State State { get; set; }
    }
}
