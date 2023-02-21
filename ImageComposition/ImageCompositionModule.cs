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
                var threshold = Math.Max(image.Width * image.Height * 0.000002D, 5);
                var histogram = hist.Where(a => !a.Key.IsCmyk && a.Value > threshold).Select(a => new ImageHistogramColor
                    { RGBA = a.Key.ToByteArray(), Value = a.Value }).ToList();
                var blackAndWhite = histogram.All(IsBlackOrWhite);
                // very desaturated or very dark or very bright
                var grayscale = blackAndWhite || histogram.All(IsGray) || histogram.Select(a => HSLAFromRGBA(a.RGBA)).All(a => a[1] < 0.05 || a[2] < 0.1 || a[2] > 0.9);
                var monochrome = grayscale;
                if (!monochrome)
                {
                    var hsl = histogram.Select(a => HSLAFromRGBA(a.RGBA)).ToList();
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

        public static double[] HSLAFromRGBA(byte[] rgba)
        {
            double h;
            double s;
            var modifiedR = rgba[0] / 255.0;
            var modifiedG = rgba[1] / 255.0;
            var modifiedB = rgba[2] / 255.0;
            var normA = rgba[3] / 255.0;

            var min = Math.Min(Math.Min(modifiedR, modifiedG), modifiedB);
            var max = Math.Max(Math.Max(modifiedR, modifiedG), modifiedB);
            var delta = max - min;
            var l = (min + max) / 2;

            if (delta == 0)
            {
                h = 0;
                s = 0;
            }
            else
            {
                s = l <= 0.5 ? delta / (min + max) : delta / (2 - max - min);

                if (Math.Abs(modifiedR - max) < 0.000001D)
                {
                    h = (modifiedG - modifiedB) / 6 / delta;
                }
                else if (Math.Abs(modifiedG - max) < 0.000001D)
                {
                    h = 1.0 / 3 + (modifiedB - modifiedR) / 6 / delta;
                }
                else
                {
                    h = 2.0 / 3 + (modifiedR - modifiedG) / 6 / delta;
                }

                h = h < 0 ? ++h : h;
                h = h > 1 ? --h : h;
            }

            return new [] {h, s, l, normA};
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
            Parallel.ForEach(imagesToProcess, new ParallelOptions{MaxDegreeOfParallelism = 8, CancellationToken = token}, (imageId, state) =>
            {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        state.Break();
                        return;
                    }

                    using var scope = provider.CreateScope();
                    using var sContext = scope.ServiceProvider.GetRequiredService<CoreContext>();
                    if (index % 20 == 1) _logger.LogInformation("Analyzed {Index}/{Count} images", index, count);
                    var image = sContext.Set<Image>().Include(a => a.Blobs).Include(a => a.Sources)
                        .Include(a => a.Composition).FirstOrDefault(a => a.ImageId == imageId);
                    if (token.IsCancellationRequested)
                    {
                        state.Break();
                        return;
                    }

                    AnalyzeImage(image);
                    if (token.IsCancellationRequested)
                    {
                        state.Break();
                        return;
                    }

                    index++;
                    sContext.SaveChanges();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unable to save composition: {Ex}", e);
                }
            });

            return Task.CompletedTask;
        }
    }
}
