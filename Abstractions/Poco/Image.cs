using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ImageInfrastructure.Abstractions.Poco
{
    public class Image
    {
        public int ImageId { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        [IgnoreDataMember]
        public byte[] Blob { get; set; }
        
        public virtual List<ArtistAccount> ArtistAccounts { get; set; }

        public virtual List<ImageSource> Sources { get; set; }
        public virtual List<ImageTag> Tags { get; set; }

        [IgnoreDataMember]
        public virtual List<RelatedImage> RelatedImages { get; set; }
    }
}