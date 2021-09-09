using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace unbooru.Abstractions.Poco.Ingest
{
    public class Attachment
    {
        [UsedImplicitly] public string Filename { get; set; }
        [UsedImplicitly] public string Uri { get; set; }
        [UsedImplicitly] public (int Width, int Height) Size { get; set; }
        [UsedImplicitly] public long Filesize { get; set; }
        [UsedImplicitly] public byte[] Data { get; set; }
        public bool Download { get; set; } = true;
        [IgnoreDataMember] public Post Post { get; set; }
    }
}