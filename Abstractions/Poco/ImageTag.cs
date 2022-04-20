using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using MoreLinq;
using unbooru.Abstractions.Enums;
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace unbooru.Abstractions.Poco
{
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public class ImageTag
    {
        public int ImageTagId { get; set; }
        
        public string Name { get; set; }
        public string Description { get; set; }
        public TagSafety Safety { get; set; }
        public string Type { get; set; }

        [IgnoreDataMember]
        [NotMapped]
        public IReadOnlyList<Image> Images => TagSources.Select(a => a.Image).DistinctBy(a => a.ImageId).ToList();
        [IgnoreDataMember]
        public virtual List<ImageTagSource> TagSources { get; set; }

        protected bool Equals(ImageTag other)
        {
            return Name == other.Name && Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ImageTag) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Type != null ? Type.GetHashCode() : 0);
            }
        }

        public static bool operator ==(ImageTag left, ImageTag right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ImageTag left, ImageTag right)
        {
            return !Equals(left, right);
        }
    }
}