using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace ImageInfrastructure.Abstractions.Poco.Events
{
    public class Attachment
    {
        [UsedImplicitly] public string Uri { get; set; }
        [UsedImplicitly] public (int Width, int Height) Size { get; set; }
        [UsedImplicitly] public long Filesize { get; set; }
        [UsedImplicitly] public byte[] Data { get; set; }
        [UsedImplicitly] public bool Cancelled { get; set; }
        [IgnoreDataMember]
        public Post Post { get; set; }
    }
}