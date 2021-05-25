using System;
using ImageInfrastructure.Abstractions.Poco.Events;

namespace ImageInfrastructure.Abstractions.Interfaces
{
    public interface IImageProvider
    {
        EventHandler<ImageDiscoveredEventArgs> ImageDiscovered { get; set; }
        EventHandler<ImageProvidedEventArgs> ImageProvided { get; set; }
        string Source { get; }
    }
}