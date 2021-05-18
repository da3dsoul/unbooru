using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Attributes;
using ImageInfrastructure.Abstractions.Enums;
using ImageInfrastructure.Abstractions.Interfaces;
using ImageInfrastructure.Abstractions.Poco;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImageInfrastructure.ImageSaveHandler
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
        
        [ModulePostConfiguration(Priority = ModuleInitializationPriority.Last)]
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

        private async void ImageProvided(object sender, ImageProvidedEventArgs e)
        {
            try
            {
                var path = Path.Combine(SettingsProvider.Get(a => a.BasePath), e.OriginalFilename[..3]);
                Directory.CreateDirectory(path);
                var i = e.OriginalFilename.LastIndexOf('.');
                var ext = Path.GetExtension(e.OriginalFilename);
                var name = e.OriginalFilename[..i] + " - " + ReplaceInvalidPathCharacters(e.Title) + ext;
                
                path = Path.Combine(path, name);
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
            var name = Path.GetFileName(e.ImageUri.AbsolutePath);
            var ext = Path.GetExtension(e.ImageUri.AbsolutePath);
            var i = name.LastIndexOf('.');   
            name = name[..i] + " - " + ReplaceInvalidPathCharacters(e.Title) + ext;
            var path = Path.Combine(SettingsProvider.Get(a => a.BasePath), name[..3], name);
            if (!File.Exists(path)) return;
            _logger.LogInformation("Image already exists at {Path}. Skipping!", path);
            e.Cancel = true;
        }
        
        public static string ReplaceInvalidPathCharacters(string path)
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