using System.Runtime.Serialization;

namespace ImageInfrastructure.Abstractions.Poco
{
    public class ImageBlob
    {
        public int ImageBlobId { get; set; }
        
        public byte[] Data { get; set; }
        
        [IgnoreDataMember]
        public virtual Image Image { get; set; }
    }
}