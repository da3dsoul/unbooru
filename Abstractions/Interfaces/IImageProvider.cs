using System;
using ImageInfrastructure.Abstractions.Poco;

namespace ImageInfrastructure.Abstractions.Interfaces
{
    public interface IImageProvider
    {
        EventHandler<ImageDiscoveredEventArgs> ImageDiscovered { get; set; }
        EventHandler<ImageProvidedEventArgs> ImageProvided { get; set; }
        string Source { get; }
    }
}