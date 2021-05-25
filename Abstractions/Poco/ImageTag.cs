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
    }
}