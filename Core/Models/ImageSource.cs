using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace ImageInfrastructure.Core.Models
{
    [Index(nameof(Uri), nameof(Source))]
    public class ImageSource
    {
        public int ImageSourceId { get; set; }
        
        public string Source { get; set; }
        public string OriginalFilename { get; set; }
        public string Uri { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        
        [JsonIgnore]
        [IgnoreDataMember]
        public Image Image { get; set; }
    }
}