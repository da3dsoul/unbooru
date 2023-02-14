using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using unbooru.Abstractions;
using unbooru.Abstractions.Attributes;
using unbooru.Abstractions.Enums;
using unbooru.Abstractions.Interfaces;
using unbooru.Abstractions.Poco.Events;
using ImageMagick;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using unbooru.Abstractions.Poco;

namespace unbooru.ImageCompressor
{
    public class ImageCompressorModule : IModule
    {
        private readonly ILogger<ImageCompressorModule> _logger;
        
        public ImageCompressorModule(ILogger<ImageCompressorModule> logger)
        {
            _logger = logger;
        }
        
        [ModulePostConfiguration(Priority = ModuleInitializationPriority.PreProcessing)]
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
            foreach (var img in e.Images)
            {
                try
                {
                    _logger.LogInformation("Compressing {Path}", img.GetPixivFilename());
                    using var image = new MagickImage(img.Blob) {Format = MagickFormat.Pjpeg, Quality = 100};
                    var data = image.ToByteArray();
                    if (data.LongLength > img.Blob.LongLength)
                    {
                        _logger.LogInformation("{Path} was already compressed better than ImageMagick. Keeping original", img.GetPixivFilename());
                        return;
                    }

                    var originalBlob = img.Blob;
                    img.Blob = data;

                    var histogram = image.Histogram().Where(a => !a.Key.IsCmyk).Select(a => new ImageHistogramColor
                        { RGBA = a.Key.ToByteArray(), Value = a.Value }).ToList();
                    var blackAndWhite = histogram.All(IsBlackOrWhite);
                    var grayscale = blackAndWhite || histogram.All(IsGray);
                    var monochrome = grayscale;
                    if (!monochrome)
                    {
                        var hsl = histogram.Select(a => HSLAFromRGB(a.RGBA)).ToList();
                        var deltaH = hsl.Max(a => a[0]) - hsl.Min(a => a[0]);
                        monochrome = deltaH < 0.1;
                    }

                    img.Composition = new ImageComposition
                    {
                        Image = img,
                        Histogram = histogram,
                        IsBlackAndWhite = blackAndWhite,
                        IsGrayscale = grayscale,
                        IsMonochrome = monochrome
                    };

                    img.Sources.ForEach(a => a.OriginalFilename = Path.GetFileNameWithoutExtension(a.OriginalFilename) + ".jpg");
                    _logger.LogInformation("Compressed {Path} from {Original} to {New}", img.GetPixivFilename(), originalBlob.LongLength, img.Blob.LongLength);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Unable to compress {File}", img.GetPixivFilename());
                }
            }
        }

        private static bool IsBlackOrWhite(ImageHistogramColor a)
        {
            return a.ColorKey is 0 or byte.MaxValue or (byte.MaxValue << 24 | byte.MaxValue << 16 | byte.MaxValue << 8 | byte.MaxValue) or (byte.MaxValue << 24 | byte.MaxValue << 16 | byte.MaxValue << 8);
        }

        private static bool IsGray(ImageHistogramColor a)
        {
            return a.RGBA[0] == a.RGBA[1] && a.RGBA[0] == a.RGBA[2];
        }

        public static double[] HSLAFromRGB(byte[] rgba)
        {
            var normR = rgba[0] / 255D;
            var normG = rgba[1] / 255D;
            var normB = rgba[2] / 255D;
            var normA = rgba[3] / 255D;

            var min = Math.Min(Math.Min(normR, normG), normB);
            var max = Math.Max(Math.Max(normR, normG), normB);
            var delta = max - min;

            var h = 0D;
            var s = 0D;
            var l = (max + min) / 2.0D;

            if (delta == 0) return new[] { h, s, l, normA };
            if (l < 0.5D)
            {
                s = delta / (max + min);
            }
            else
            {
                s = delta / (2.0D - max - min);
            }

            if (Math.Abs(normR - max) < 0.001D)
            {
                h = (normG - normB) / delta;
            }
            else if (Math.Abs(normG - max) < 0.001D)
            {
                h = 2D + (normB - normR) / delta;
            }
            else if (Math.Abs(normB - max) < 0.001D)
            {
                h = 4 + (normR - normG) / delta;
            }

            return new[] { h, s, l, normA };
        }
        private void ImageDiscovered(object sender, ImageDiscoveredEventArgs e)
        {
            foreach (var attachment in e.Attachments)
            {
                var path = attachment.Filename;
                var i = path.LastIndexOf(".", StringComparison.Ordinal);
                path = path[..i] + ".jpg";
                attachment.Filename = path;
            }
        }

        public Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            _logger.LogInformation("Running {ModuleType} module", this.GetType());
            return Task.CompletedTask;
        }
    }
}
