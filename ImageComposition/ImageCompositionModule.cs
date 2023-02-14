using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageMagick;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using unbooru.Abstractions;
using unbooru.Abstractions.Attributes;
using unbooru.Abstractions.Enums;
using unbooru.Abstractions.Interfaces;
using unbooru.Abstractions.Poco;
using unbooru.Abstractions.Poco.Events;
using unbooru.Core;

namespace unbooru.ImageComposition
{
    public class ImageCompositionModule : IModule
    {
        private readonly ILogger<ImageCompositionModule> _logger;
        
        public ImageCompositionModule(ILogger<ImageCompositionModule> logger)
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
            }
        }
        
        
        private void ImageProvided(object sender, ImageProvidedEventArgs e)
        {
            foreach (var img in e.Images)
            {
                AnalyzeImage(img);
            }
        }

        private void AnalyzeImage(Image img)
        {
            try
            {
                _logger.LogInformation("Analyzing {Path}", img.GetPixivFilename());
                using var image = new MagickImage(img.Blob);

                var hist = image.Histogram();
                var histogram = hist.Where(a => !a.Key.IsCmyk && a.Value > 0).Select(a => new ImageHistogramColor
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

                img.Composition = new Abstractions.Poco.ImageComposition
                {
                    Image = img,
                    IsBlackAndWhite = blackAndWhite,
                    IsGrayscale = grayscale,
                    IsMonochrome = monochrome
                };
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to analyze {File}", img.GetPixivFilename());
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

        public Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            _logger.LogInformation("Running {ModuleType} module", GetType());
            using var context = provider.GetRequiredService<CoreContext>();
            var count = context.Set<Image>().Count(a => a.Composition == null);
            var imagesToProcess = context.Set<Image>().Where(a => a.Composition == null).Select(a => a.ImageId).ToList();

            if (count == 0) return Task.CompletedTask;
            if (token.IsCancellationRequested) return Task.CompletedTask;

            _logger.LogInformation("Analyzing {Count} images", count);
            var index = 0;
            foreach (var imageId in imagesToProcess)
            {
                try
                {
                    if (token.IsCancellationRequested) return Task.CompletedTask;
                    using var scope = provider.CreateScope();
                    using var sContext = scope.ServiceProvider.GetRequiredService<CoreContext>();
                    if (index % 20 == 1) _logger.LogInformation("Analyzed {Index}/{Count} images", index, count);
                    var image = sContext.Set<Image>().Include(a => a.Blobs).Include(a => a.Sources)
                        .Include(a => a.Composition).FirstOrDefault(a => a.ImageId == imageId);
                    if (token.IsCancellationRequested) return Task.CompletedTask;
                    AnalyzeImage(image);
                    if (token.IsCancellationRequested) return Task.CompletedTask;
                    index++;
                    sContext.SaveChanges();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unable to save composition: {Ex}", e);
                }
            }

            return Task.CompletedTask;
        }
    }
}
