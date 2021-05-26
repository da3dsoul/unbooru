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
            Save(e).Wait();
        }

        private async Task Save(ImageProvidedEventArgs e)
        {
            await using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Saving {Count} images to database for {Image}", e.Images.Count, e.Images.FirstOrDefault()?.GetPixivFilename());
                await _context.ArtistAccounts.AddRangeAsync(e.Images.SelectMany(a => a.ArtistAccounts).Where(a => a.ArtistAccountId == 0).Distinct());
                await _context.ImageTags.AddRangeAsync(e.Images.SelectMany(a => a.Tags).Where(a => a.ImageTagId == 0).Distinct());
                await _context.RelatedImages.AddRangeAsync(e.Images.SelectMany(a => a.RelatedImages).Where(a => a.RelatedImageId == 0).Distinct());
                await _context.Images.AddRangeAsync(e.Images);
                await _context.SaveChangesAsync();
                await trans.CommitAsync();
                _logger.LogInformation("Finished saving {Count} images to database for {Image}", e.Images.Count, e.Images.FirstOrDefault()?.GetPixivFilename());
            }
            catch (Exception exception)
            {
                await trans.RollbackAsync();
                _logger.LogError(exception, "Unable to write {File}", e.Images.FirstOrDefault()?.GetPixivFilename());
            }
        }

        private async void ImageDiscovered(object sender, ImageDiscoveredEventArgs e)
        {
            foreach (var attachment in e.Attachments.ToList())
            {
                var any = (await _context.ImageSources.OrderBy(a => a.ImageSourceId).FirstOrDefaultAsync(a => a.Uri == attachment.Uri)) != null;
                if (!any) return;
                _logger.LogInformation("Image already exists for {Uri}. Skipping!", attachment.Uri);
                attachment.Download = false;
            }
        }

        public Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}