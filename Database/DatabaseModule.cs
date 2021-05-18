using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Attributes;
using ImageInfrastructure.Abstractions.Enums;
using ImageInfrastructure.Abstractions.Interfaces;
using ImageInfrastructure.Abstractions.Poco;
using ImageInfrastructure.Core;
using ImageInfrastructure.Core.Models;
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
        
        [ModulePostConfiguration(Priority = ModuleInitializationPriority.High)]
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
            try
            {
                _logger.LogInformation("Saving {Image} to database", e.OriginalFilename);
                var image = new Image()
                {
                    Blob = e.Data,
                    Sources = new List<ImageSource>
                    {
                        new()
                        {
                            Source = sender.GetType().Name,
                            OriginalFilename = e.OriginalFilename,
                            Uri = e.Uri,
                            Title = e.Title,
                            Description = e.Description
                        }
                    },
                    ArtistAccounts = new List<ArtistAccount>
                    {
                        new()
                        {
                            Name = e.ArtistName,
                            Url = e.ArtistUrl
                        }
                    }
                };
                _context.Images.Add(image);
                _context.SaveChanges();
                _logger.LogInformation("Finished saving {Image} to database", e.OriginalFilename);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to write {File}", e.OriginalFilename);
            }
        }

        private void ImageDiscovered(object sender, ImageDiscoveredEventArgs e)
        {
            var count = _context.Images.Include(a => a.Sources).Count(a => a.Sources.Where(b => b.Uri == e.ImageUri.AbsoluteUri).Take(1).Any());
            if (count == 0) return;
            _logger.LogInformation("Image already exists for {Uri}. Skipping!", e.ImageUri.AbsoluteUri);
            e.Cancel = true;
        }

        public Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}