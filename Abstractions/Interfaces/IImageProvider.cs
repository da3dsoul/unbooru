using System;
using unbooru.Abstractions.Poco.Events;

namespace unbooru.Abstractions.Interfaces
{
    public interface IImageProvider
    {
        EventHandler<ImageDiscoveredEventArgs> ImageDiscovered { get; set; }
        EventHandler<ImageProvidedEventArgs> ImageProvided { get; set; }
        string Source { get; }
    }
}