using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using unbooru.Abstractions.Poco.Ingest;

namespace unbooru.Abstractions.Poco.Events
{
    public class ImageDiscoveredEventArgs : CancelEventArgs
    {
        public IServiceProvider ServiceProvider { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public List<Attachment> Attachments { get; set; }
        public Post Post { get; set; }
    }
}