using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Attributes;
using ImageInfrastructure.Abstractions.Interfaces;
using ImageInfrastructure.Abstractions.Poco;
using ImageInfrastructure.Core;
using ImageInfrastructure.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MatchType = IqdbApi.Enums.MatchType;

namespace ImageInfrastructure.Iqdb
{
    public class IqdbModule : IModule
    {
        private ISettingsProvider<IqdbSettings> SettingsProvider { get; set; }
        private readonly ILogger<IqdbModule> _logger;
        private readonly CoreContext _context;
        
        public IqdbModule(ISettingsProvider<IqdbSettings> settingsProvider, ILogger<IqdbModule> logger, CoreContext context)
        {
            SettingsProvider = settingsProvider;
            _logger = logger;
            _context = context;
        }
        
        [ModulePostConfiguration]
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
            FindTags(e).Wait();
        }

        private async Task FindTags(ImageProvidedEventArgs e)
        {
            try
            {
                _logger.LogInformation("Querying iqdb for {Image}", e.OriginalFilename);
                using var client = new IqdbApi.IqdbClient();
                await using var stream = new MemoryStream(e.Data);
                var results = await client.SearchFile(stream);
                if (!results.IsFound) return;
                var match = results.Matches.FirstOrDefault(a => a.MatchType == MatchType.Best);
                if (match == null) return;
                _logger.LogInformation("iqdb found match for {Image}", e.OriginalFilename);
                var image = await _context.Images.Include(a => a.Tags)
                    .Include(a => a.Sources.Where(b => b.Uri == e.Uri).Take(1)).AsSplitQuery().FirstOrDefaultAsync(a => a.Sources.Any());
                if (image == null)
                {
                    _logger.LogError("Unable to get image for {Image} in {Type}", e.OriginalFilename, GetType().Name);
                    return;
                }
                foreach (var matchTag in match.Tags)
                {
                    var imageTag = await _context.ImageTags.Include(a => a.Images).FirstOrDefaultAsync(a => a.Name == matchTag);
                    if (imageTag != null)
                    {
                        image.Tags.Add(imageTag);
                        imageTag.Images.Add(image);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        imageTag = new ImageTag()
                        {
                            Name = matchTag
                        };
                        imageTag.Images ??= new List<Image>();
                        imageTag.Images.Add(image);
                        image.Tags.Add(imageTag);
                        await _context.SaveChangesAsync();
                    }
                }
                _logger.LogInformation("Finished saving iqdb tags for {Image}", e.OriginalFilename);
            }
            catch (IqdbApi.Exceptions.ImageTooLargeException)
            {
                
            }
            catch (IqdbApi.Exceptions.HttpRequestFailed)
            {
                
            }
            catch (IqdbApi.Exceptions.InvalidFileFormatException)
            {
                
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to write {File}", e.OriginalFilename);
            }
        }

        public Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            _logger.LogInformation("Running {ModuleType} module with settings of type {SettingsType}", GetType(), SettingsProvider.GetType().GenericTypeArguments.FirstOrDefault());
            return Task.CompletedTask;
        }
    }
}