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
            using var trans = _context.Database.BeginTransaction();
            try
            {
                _logger.LogInformation("Saving {Count} to database for {Image}", e.Images.Count, e.Images.FirstOrDefault()?.GetPixivFilename());
                _context.Images.AddRange(e.Images);
                _context.SaveChanges();
                trans.Commit();
                _logger.LogInformation("Finished saving {Count} to database for {Image}", e.Images.Count, e.Images.FirstOrDefault()?.GetPixivFilename());
            }
            catch (Exception exception)
            {
                trans.Rollback();
                _logger.LogError(exception, "Unable to write {File}", e.Images.FirstOrDefault()?.GetPixivFilename());
            }
        }

        private void ImageDiscovered(object sender, ImageDiscoveredEventArgs e)
        {
            foreach (var attachment in e.Attachments)
            {
                var any = _context.Images.Include(a => a.Sources).Any(a => a.Sources.Any(b => b.Uri == attachment.Uri));
                if (!any) return;
                _logger.LogInformation("Image already exists for {Uri}. Skipping!", attachment.Uri);
                e.CancelAttachmentDownload(_logger, attachment);
            }
        }

        public Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}