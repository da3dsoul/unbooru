using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using unbooru.Abstractions.Poco.Ingest;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace unbooru.Abstractions.Poco.Events
{
    public class ImageProvidedEventArgs : CancelEventArgs
    {
        public IServiceProvider ServiceProvider { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public List<Attachment> Attachments { get; set; }
        public Post Post { get; set; }

        public List<Image> Images { get; set; }
    }
}