using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;

namespace ImageInfrastructure.Abstractions.Poco
{
    public class ImageSource
    {
        public int ImageSourceId { get; set; }
        
        public string Source { get; set; }
        public string OriginalFilename { get; set; }
        public string Uri { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        [IgnoreDataMember]
        public Image Image { get; set; }

        [IgnoreDataMember]
        public List<RelatedImage> RelatedImages { get; set; }

        [NotMapped] public List<int> RelatedImageIds => RelatedImages?.Select(a => a.Image.ImageId).ToList() ?? new List<int>();

        protected bool Equals(ImageSource other)
        {
            return Source == other.Source && Uri == other.Uri;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ImageSource) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Source != null ? Source.GetHashCode() : 0) * 397) ^ (Uri != null ? Uri.GetHashCode() : 0);
            }
        }

        public static bool operator ==(ImageSource left, ImageSource right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ImageSource left, ImageSource right)
        {
            return !Equals(left, right);
        }
    }
}