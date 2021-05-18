using System.Collections.Generic;
using System.Text.Json.Serialization;
using ImageInfrastructure.Abstractions.Enums;

namespace ImageInfrastructure.Core.Models
{
    public class ImageTag
    {
        public int ImageTagId { get; set; }
        
        public string Name { get; set; }
        public string Description { get; set; }
        public TagSafety Safety { get; set; }
        
        [JsonIgnore]
        public List<Image> Images { get; set; }
    }
}