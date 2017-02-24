using System;
using System.ComponentModel.DataAnnotations.Schema;
using SiteParserCore.Repository;

namespace SiteParserCore.Models
{
    public enum State
    {
        IsAwaiting,
        IsInTree
    }

    public class Url : Entity, IEquatable<Url>
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public string Name { get; set; }
        public string ParentName { get; set; }
        public int NestingLevel { get; set; }
        public int HtmlSize { get; set; }
        public bool IsExternal { get; set; }
        public double ResponseTime { get; set; }
        public DateTime? DateTimeOfLastScan { get; set; }
        [NotMapped]
        public State State { get; set; }

        public bool Equals(Url other)
        {
            return string.Equals(Name, other.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}



