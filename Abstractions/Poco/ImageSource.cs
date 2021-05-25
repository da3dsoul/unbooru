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

        [NotMapped] public List<int> RelatedImageIds => RelatedImages.Select(a => a.Image.ImageId).ToList();
    }
}