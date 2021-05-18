using System;
using System.ComponentModel;

namespace ImageInfrastructure.Abstractions.Poco
{
    public class ImageDiscoveredEventArgs : CancelEventArgs
    {
        public Uri ImageUri { get; set; }
        public long Size { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ArtistName { get; set; }
        public string ArtistUrl { get; set; }
    }
}