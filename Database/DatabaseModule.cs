using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using unbooru.Abstractions;
using unbooru.Abstractions.Attributes;
using unbooru.Abstractions.Enums;
using unbooru.Abstractions.Interfaces;
using unbooru.Abstractions.Poco.Events;
using unbooru.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using unbooru.Abstractions.Poco;

namespace unbooru.Database
{
    public class DatabaseModule : IModule
    {
        private readonly ILogger<DatabaseModule> _logger;

        public DatabaseModule(ILogger<DatabaseModule> logger)
        {
            _logger = logger;
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
            Save(e).Wait(e.CancellationToken);
        }

        private async Task Save(ImageProvidedEventArgs e)
        {
            var context = e.ServiceProvider.GetRequiredService<CoreContext>();
            await using var trans = await context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Saving {Count} images to database for {Image}", e.Images.Count, e.Images.FirstOrDefault()?.GetPixivFilename());
                context.Set<ArtistAccount>().AddRange(e.Images.SelectMany(a => a.ArtistAccounts).Where(a => a.ArtistAccountId == 0).Distinct());
                context.Set<ImageTag>().AddRange(e.Images.SelectMany(a => a.Tags).Where(a => a.ImageTagId == 0).Distinct());
                context.Set<RelatedImage>().AddRange(e.Images.SelectMany(a => a.RelatedImages).Where(a => a.RelatedImageId == 0).Distinct());
                context.Set<Image>().AddRange(e.Images.Where(a => a.ImageId == 0).Distinct());
                await context.SaveChangesAsync(e.CancellationToken);
                await trans.CommitAsync(e.CancellationToken);
                _logger.LogInformation("Finished saving {Count} images to database for {Image}", e.Images.Count, e.Images.FirstOrDefault()?.GetPixivFilename());
            }
            catch (Exception exception)
            {
                await trans.RollbackAsync();
                _logger.LogError(exception, "Unable to write {File}: {Exception}", e.Images?.FirstOrDefault()?.GetPixivFilename(), exception);
            }
        }

        private void ImageDiscovered(object sender, ImageDiscoveredEventArgs e)
        {
            DiscoverAsync(e).Wait(e.CancellationToken);
        }

        private async Task DiscoverAsync(ImageDiscoveredEventArgs e)
        {
            var context = e.ServiceProvider.GetRequiredService<CoreContext>();
            foreach (var attachment in e.Attachments.ToList())
            {
                var any = await context.ImageSources.OrderBy(a => a.ImageSourceId)
                    .FirstOrDefaultAsync(a => a.Uri == attachment.Uri, e.CancellationToken) != null;
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
