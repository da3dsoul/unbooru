using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions;
using ImageInfrastructure.Abstractions.Attributes;
using ImageInfrastructure.Abstractions.Enums;
using ImageInfrastructure.Abstractions.Interfaces;
using ImageInfrastructure.Abstractions.Poco.Events;
using ImageInfrastructure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImageInfrastructure.Database
{
    public class DatabaseModule : IModule
    {
        private readonly ILogger<DatabaseModule> _logger;
        private readonly CoreContext _context;
        
        public DatabaseModule(ILogger<DatabaseModule> logger, CoreContext context)
        {
            _logger = logger;
            _context = context;
        }
        
        [ModulePostConfiguration(Priority = ModuleInitializationPriority.Database)]
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
            foreach (var image in e.Images)
            {
                try
                {
                    _logger.LogInformation("Saving {Image} to database", image.GetPixivFilename());
                    _context.Images.Add(image);
                    _context.SaveChanges();
                    _logger.LogInformation("Finished saving {Image} to database", image.GetPixivFilename());
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Unable to write {File}", image.GetPixivFilename());
                }
            }
        }

        private void ImageDiscovered(object sender, ImageDiscoveredEventArgs e)
        {
            foreach (var image in e.Attachments)
            {
                var any = _context.Images.Include(a => a.Sources).Any(a => a.Sources.Any(b => b.Uri == image.Uri));
                if (!any) return;
                _logger.LogInformation("Image already exists for {Uri}. Skipping!", image.Uri);
                image.Cancelled = true;
            }
        }

        public Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}