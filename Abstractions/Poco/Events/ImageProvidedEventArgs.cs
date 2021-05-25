using System.Collections.Generic;
using System.ComponentModel;

namespace ImageInfrastructure.Abstractions.Poco.Events
{
    public class ImageProvidedEventArgs : CancelEventArgs
    {
        public List<Attachment> Attachments { get; set; }
        public Post Post { get; set; }

        public List<Image> Images { get; set; }
    }
}