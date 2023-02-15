using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageMagick;
using unbooru.Abstractions;
using unbooru.Abstractions.Attributes;
using unbooru.Abstractions.Enums;
using unbooru.Abstractions.Interfaces;
using unbooru.Abstractions.Poco;
using unbooru.Abstractions.Poco.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace unbooru.ImageSaveHandler
{
    public class ImageSaveHandlerModule : IModule
    {
        private ISettingsProvider<ImageSaveHandlerSettings> SettingsProvider { get; set; }
        private readonly ILogger<ImageSaveHandlerModule> _logger;
        
        public ImageSaveHandlerModule(ISettingsProvider<ImageSaveHandlerSettings> settingsProvider, ILogger<ImageSaveHandlerModule> logger)
        {
            SettingsProvider = settingsProvider;
            _logger = logger;
        }
        
        [ModulePostConfiguration(Priority = ModuleInitializationPriority.Saving)]
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

        public void ImageProvided(object sender, ImageProvidedEventArgs e)
        {
            SaveImage(e).Wait(e.CancellationToken);
        }

        public async Task SaveImage(ImageProvidedEventArgs e)
        {
            foreach (var image in e.Images)
            {
                try
                {
                    if (image.Composition is { IsMonochrome: true }) return;
                    var tags = image.Tags;
                    if (tags == null || !tags.Any())
                    {
                        if (SettingsProvider.Get(a => a.ExcludeMissingInfo))
                        {
                            _logger.LogInformation(
                                "ImageSaveHandler set to exclude images with missing info, and no tags were found. Skipping!");
                            return;
                        }
                    }
                    else
                    {
                        var exclude = SettingsProvider.Get(a => a.ExcludeTags);
                        if (tags.Select(a => a.Name).Any(a =>
                            exclude.Any(b => a.Equals(b, StringComparison.InvariantCultureIgnoreCase))))
                        {
                            _logger.LogInformation(
                                "ImageSaveHandler set to exclude images with certain tags, and tags matched. Skipping!");
                            return;
                        }
                    }

                    var path = GetImagePath(image);
                    if (string.IsNullOrEmpty(path)) return;
                    Directory.CreateDirectory(Path.GetDirectoryName(path) ??
                                              throw new NullReferenceException("Image path cannot be null"));
                    if (File.Exists(path)) return;
                    _logger.LogInformation("Saving image to {Path}", path);
                    using var img = new MagickImage(image.Blob);
                    if (img.Height > 3840 || img.Width > 3840) img.Resize(new MagickGeometry("3840x3840>"));
                    await using var stream = File.OpenWrite(path);
                    await img.WriteAsync(stream, e.CancellationToken);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Unable to write {File}", image.GetPixivFilename());
                }
            }
        }
        
        public string GetImagePath(Image image)
        {
            var source = image.Sources.FirstOrDefault(a => a.Source == "Pixiv");
            if (source == null) return null;

            var originalName = source.OriginalFilename;
            var title = source.Title;
            var aspectRatio = (float) image.Width / image.Height;
            var path = GetImagePath(originalName, title, aspectRatio);
            return path;
        }

        private string GetImagePath(string originalName, string title, double aspectRatio)
        {
            var path = SettingsProvider.Get(a => a.ImagePath);
            if (string.IsNullOrEmpty(path))
            {
                path = Path.Combine(Arguments.DataPath, "Images");
                var update = path;
                SettingsProvider.Update(a => a.ImagePath = update);
            }

            if (SettingsProvider.Get(a => a.EnableAspectRatioSplitting))
            {
                var subpath = aspectRatio < 0.8 ? "Mobile" : aspectRatio > 1.25 ? "Desktop" : null;
                if (string.IsNullOrEmpty(subpath)) return null;
                path = Path.Combine(path, subpath);
            }

            if (SettingsProvider.Get(a => a.UseFilesystemFriendlyTree))
                path = Path.Combine(path, originalName[..3]);
            var i = originalName.LastIndexOf('.');
            var ext = Path.GetExtension(originalName);
            var name = originalName[..i] + " - " + ReplaceInvalidPathCharacters(title) + ext;

            path = Path.Combine(path, name);
            return path;
        }

        private static string ReplaceInvalidPathCharacters(string path)
        {
            string ret = path.Replace(@"*", "\u2605"); // ★ (BLACK STAR)
            ret = ret.Replace(@"|", "\u00a6"); // ¦ (BROKEN BAR)
            ret = ret.Replace(@"\", "\u29F9"); // ⧹ (BIG REVERSE SOLIDUS)
            ret = ret.Replace(@"/", "\u2044"); // ⁄ (FRACTION SLASH)
            ret = ret.Replace(@":", "\u0589"); // ։ (ARMENIAN FULL STOP)
            ret = ret.Replace("\"", "\u2033"); // ″ (DOUBLE PRIME)
            ret = ret.Replace(@">", "\u203a"); // › (SINGLE RIGHT-POINTING ANGLE QUOTATION MARK)
            ret = ret.Replace(@"<", "\u2039"); // ‹ (SINGLE LEFT-POINTING ANGLE QUOTATION MARK)
            ret = ret.Replace(@"?", "\uff1f"); // ？ (FULL WIDTH QUESTION MARK)
            ret = ret.Replace(@"...", "\u2026"); // … (HORIZONTAL ELLIPSIS)
            if (ret.StartsWith(".", StringComparison.Ordinal)) ret = "․" + ret.Substring(1, ret.Length - 1);
            if (ret.EndsWith(".", StringComparison.Ordinal)) // U+002E
                ret = ret.Substring(0, ret.Length - 1) + "․"; // U+2024
            return ret.Trim();
        }

        public Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
