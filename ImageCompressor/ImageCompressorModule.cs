using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Attributes;
using ImageInfrastructure.Abstractions.Enums;
using ImageInfrastructure.Abstractions.Interfaces;
using ImageInfrastructure.Abstractions.Poco;
using ImageMagick;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImageInfrastructure.ImageCompressor
{
    public class ImageCompressorModule : IModule
    {
        private readonly ILogger<ImageCompressorModule> _logger;
        
        public ImageCompressorModule(ILogger<ImageCompressorModule> logger)
        {
            _logger = logger;
        }
        
        [ModulePostConfiguration(Priority = ModuleInitializationPriority.PreDatabase)]
        public void PostConfigure(IServiceProvider provider)
        {
            var imageProviders = provider.GetServices<IImageProvider>().ToList();
            if (imageProviders.Count == 0) return;
            foreach (var imageProvider in imageProviders)
            {
                imageProvider.ImageProvided += ImageProvided;
                imageProvider.ImageDiscovered += ImageDiscovered;
            }
        }
        
        [ModuleShutdown]
        public void Shutdown(IServiceProvider provider)
        {
            _logger.LogInformation("Shutting Down. Unregistering File Event Handlers");
            var imageProviders = provider.GetServices<IImageProvider>().ToList();
            if (imageProviders.Count == 0) return;
            foreach (var imageProvider in imageProviders)
            {
                imageProvider.ImageProvided -= ImageProvided;
                imageProvider.ImageDiscovered -= ImageDiscovered;
            }
        }

        private void ImageProvided(object sender, ImageProvidedEventArgs e)
        {
            try
            {
                _logger.LogInformation("Compressing {Path}", Path.GetFileNameWithoutExtension(e.OriginalFilename) + ".jpg");
                using var image = new MagickImage(e.Data) { Format = MagickFormat.Pjpeg, Quality = 100};
                var data = image.ToByteArray();
                e.Data = data;
                e.OriginalFilename = Path.GetFileNameWithoutExtension(e.OriginalFilename) + ".jpg";
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to compress {File}", e.OriginalFilename);
            }
        }

        private void ImageDiscovered(object sender, ImageDiscoveredEventArgs e)
        {
            var path = e.ImageUri.AbsoluteUri;
            var i = path.LastIndexOf(".", StringComparison.Ordinal);
            path = path[..i] + ".jpg";
            e.ImageUri = new Uri(path);
        }

        public Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            _logger.LogInformation("Running {ModuleType} module", this.GetType());
            return Task.CompletedTask;
        }
    }
}