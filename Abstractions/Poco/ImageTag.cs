using System.Collections.Generic;
using System.Runtime.Serialization;
using ImageInfrastructure.Abstractions.Enums;

namespace ImageInfrastructure.Abstractions.Poco
{
    public class ImageTag
    {
        public int ImageTagId { get; set; }
        
        public string Name { get; set; }
        public string Description { get; set; }
        public TagSafety Safety { get; set; }
        public string Type { get; set; }

        [IgnoreDataMember]
        public List<Image> Images { get; set; }

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