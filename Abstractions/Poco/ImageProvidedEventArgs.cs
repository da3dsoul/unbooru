using System.ComponentModel;

namespace ImageInfrastructure.Abstractions.Poco
{
    public class ImageProvidedEventArgs : CancelEventArgs
    {
        public string OriginalFilename { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ArtistName { get; set; }
        public string ArtistUrl { get; set; }
        public string Uri { get; set; }
        public byte[] Data { get; set; }
    }
}