using System.Text.Json.Serialization;

namespace ImageInfrastructure.Core.Models
{
    public class ImageSource
    {
        public int ImageSourceId { get; set; }
        
        public string Source { get; set; }
        public string OriginalFilename { get; set; }
        public string Uri { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        
        [JsonIgnore]
        public Image Image { get; set; }
    }
}