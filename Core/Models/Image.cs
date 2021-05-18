using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ImageInfrastructure.Core.Models
{
    public class Image
    {
        public int ImageId { get; set; }
        
        [JsonIgnore]
        public byte[] Blob { get; set; }
        
        public List<ArtistAccount> ArtistAccounts { get; set; }

        public List<ImageSource> Sources { get; set; }
        public List<ImageTag> Tags { get; set; }
    }
}