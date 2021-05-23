using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using ImageInfrastructure.Abstractions.Enums;
using Microsoft.EntityFrameworkCore;

namespace ImageInfrastructure.Core.Models
{
    [Index(nameof(Name), nameof(Safety), nameof(Type))]
    public class ImageTag
    {
        public int ImageTagId { get; set; }
        
        public string Name { get; set; }
        public string Description { get; set; }
        public TagSafety Safety { get; set; }
        public string Type { get; set; }
        
        [JsonIgnore]
        [IgnoreDataMember]
        public List<Image> Images { get; set; }
    }
}