using System.Collections.Generic;
using System.ComponentModel;
using ImageInfrastructure.Abstractions.Poco.Ingest;

namespace ImageInfrastructure.Abstractions.Poco.Events
{
    public class ImageDiscoveredEventArgs : CancelEventArgs
    {
        public List<Attachment> Attachments { get; set; }
        public Post Post { get; set; }
    }
}