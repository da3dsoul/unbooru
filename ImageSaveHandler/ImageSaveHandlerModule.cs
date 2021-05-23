using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions;
using ImageInfrastructure.Abstractions.Attributes;
using ImageInfrastructure.Abstractions.Enums;
using ImageInfrastructure.Abstractions.Interfaces;
using ImageInfrastructure.Abstractions.Poco;
using ImageInfrastructure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImageInfrastructure.ImageSaveHandler
{
    public class ImageSaveHandlerModule : IModule
    {
        private ISettingsProvider<ImageSaveHandlerSettings> SettingsProvider { get; set; }
        private readonly ILogger<ImageSaveHandlerModule> _logger;
        private readonly CoreContext _context;
        
        public ImageSaveHandlerModule(ISettingsProvider<ImageSaveHandlerSettings> settingsProvider, ILogger<ImageSaveHandlerModule> logger, CoreContext context)
        {
            SettingsProvider = settingsProvider;
            _logger = logger;
            _context = context;
        }
        
        [ModulePostConfiguration(Priority = ModuleInitializationPriority.Saving)]
        public void PostConfigure(IServiceProvider provider)
        {
            var imageProviders = provider.GetServices<IImageProvider>().ToList();
            if (imageProviders.Count == 0) return;
            foreach (var imageProvider in imageProviders)
            {
                imageProvider.ImageDiscovered += ImageDiscovered;
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
                imageProvider.ImageDiscovered -= ImageDiscovered;
                imageProvider.ImageProvided -= ImageProvided;
            }
        }

        private void ImageProvided(object sender, ImageProvidedEventArgs e)
        {
            SaveImage(e).Wait();
        }

        private async Task SaveImage(ImageProvidedEventArgs e)
        {
            try
            {
                var tags = (await _context.ImageSources.Include(a => a.Image).ThenInclude(a => a.Tags)
                    .FirstOrDefaultAsync(a => a.Uri == e.Uri))?.Image?.Tags;
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

                var path = GetImagePath(e);
                if (string.IsNullOrEmpty(path)) return;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                _logger.LogInformation("Saving image to {Path}", path);
                await using var stream = File.OpenWrite(path);
                await stream.WriteAsync(e.Data);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to write {File}", e.OriginalFilename);
            }
        }

        private void ImageDiscovered(object sender, ImageDiscoveredEventArgs e)
        {
            var path = GetImagePath(e);
            if (!File.Exists(path)) return;
            _logger.LogInformation("Image already exists at {Path}. Skipping!", path);
            e.Cancel = true;
        }
        
        private string GetImagePath(object input)
        {
            string originalName;
            string title;
            double aspectRatio;
            if (input is ImageDiscoveredEventArgs d)
            {
                originalName = Path.GetFileName(d.ImageUri.AbsolutePath);
                title = d.Title;
                int width = (int) (d.Size >> 32);
                int height = (int) (d.Size << 32 >> 32);
                aspectRatio = (float) width / height;
            } else if (input is ImageProvidedEventArgs p)
            {
                originalName = p.OriginalFilename;
                title = p.Title;
                int width = (int) (p.Size >> 32);
                int height = (int) (p.Size << 32 >> 32);
                aspectRatio = (float) width / height;
            }
            else
            {
                return null;
            }

            var path = Path.Combine(Arguments.DataPath, "Images");
            if (SettingsProvider.Get(a => a.EnableAspectRatioSplitting))
            {
                path = Path.Combine(path, aspectRatio < 1 ? "Mobile" : "Desktop");
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