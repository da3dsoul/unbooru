using System.Collections.Generic;
using System.ComponentModel;
using ImageInfrastructure.Abstractions.Poco.Ingest;
using Microsoft.Extensions.Logging;

namespace ImageInfrastructure.Abstractions.Poco.Events
{
    public class ImageDiscoveredEventArgs : CancelEventArgs
    {
        public List<Attachment> Attachments { get; set; }
        public Post Post { get; set; }
        public List<Image> Images { get; set; }

        public void CancelAttachmentDownload<T>(ILogger<T> logger, Attachment a) where T : class
        {
            if (!Attachments.Contains(a)) return;
            logger.LogInformation("Downloading cancelled for {Attachment}", a.Uri);
            Attachments.Remove(a);
        }
    }
}