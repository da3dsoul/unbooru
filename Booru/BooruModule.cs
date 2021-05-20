using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BooruSharp.Booru;
using BooruSharp.Search.Tag;
using ImageInfrastructure.Abstractions.Attributes;
using ImageInfrastructure.Abstractions.Enums;
using ImageInfrastructure.Abstractions.Interfaces;
using ImageInfrastructure.Abstractions.Poco;
using ImageInfrastructure.Core;
using ImageInfrastructure.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImageInfrastructure.Booru
{
    public class BooruModule : IModule
    {
        private ISettingsProvider<BooruSettings> SettingsProvider { get; set; }
        private readonly ILogger<BooruModule> _logger;
        private readonly CoreContext _context;
        
        public BooruModule(ISettingsProvider<BooruSettings> settingsProvider, ILogger<BooruModule> logger, CoreContext context)
        {
            SettingsProvider = settingsProvider;
            _logger = logger;
            _context = context;
        }
        
        [ModulePostConfiguration(Priority = ModuleInitializationPriority.Metadata)]
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
                var image = _context.Images.Include(a => a.Sources).Include(a => a.Tags).AsSplitQuery()
                    .FirstOrDefault(a => a.Sources.Any(b => b.Uri == e.Uri));
                if (image == null)
                {
                    _logger.LogError("Unable to get image for {Image} in {Type}", e.Uri, GetType().Name);
                    return;
                }

                var sources = image.Sources.Where(a => a.Source != "Pixiv").ToList();
                var tags = new HashSet<ImageTag>();
                foreach (var source in sources)
                {
                    switch (source.Source.ToLower())
                    {
                        case "gelbooru":
                        {
                            var gelbooru = new Gelbooru();
                            var i = source.Uri.LastIndexOf("md5=", StringComparison.Ordinal) + 4;
                            var id = source.Uri[i..];
                            i = id.IndexOf('&');
                            if (i > -1) id = id[..i];
                            try
                            {
                                var post = await gelbooru.GetPostByMd5Async(id);
                                foreach (var postTag in post.Tags)
                                {
                                    if (postTag == null) continue;
                                    var tagName = postTag.Replace("_", " ");
                                    var existingTag = tags.FirstOrDefault(a => a.Name == tagName);
                                    existingTag ??= await _context.ImageTags.FirstOrDefaultAsync(a => a.Name == tagName);
                                    if (existingTag == null)
                                    {
                                        try
                                        {
                                            var tagInfo = await gelbooru.GetTagAsync(postTag);
                                            if (tagInfo.Type == TagType.Artist) continue;
                                            existingTag = new ImageTag
                                            {
                                                Name = tagName,
                                                Type = tagInfo.Type.ToString()
                                            };
                                        }
                                        catch (Exception exception)
                                        {
                                            _logger.LogError(exception, "");
                                        }
                                    }

                                    if (existingTag != null && tags.All(a => a.Name != existingTag.Name)) tags.Add(existingTag);
                                }
                            }
                            catch (Exception exception)
                            {
                                _logger.LogError(exception, "");
                            }

                            break;
                        }
                        case "danbooru":
                        {
                            var danbooru = new DanbooruDonmai();
                            var i = source.Uri.LastIndexOf("/", StringComparison.Ordinal) + 1;
                            var idString = source.Uri[i..];
                            if (!int.TryParse(idString, out int id)) continue;
                            try
                            {
                                var post = await danbooru.GetPostByIdAsync(id);
                                foreach (var postTag in post.Tags)
                                {
                                    if (postTag == null) continue;
                                    var tagName = postTag.Replace("_", " ");
                                    var existingTag = tags.FirstOrDefault(a => a.Name == tagName);
                                    existingTag ??= await _context.ImageTags.FirstOrDefaultAsync(a => a.Name == tagName);
                                    if (existingTag == null)
                                    {
                                        try
                                        {
                                            var tagInfo = await danbooru.GetTagAsync(postTag);
                                            if (tagInfo.Type == TagType.Artist) continue;
                                            existingTag = new ImageTag
                                            {
                                                Name = tagName,
                                                Type = tagInfo.Type.ToString()
                                            };
                                        }
                                        catch (Exception exception)
                                        {
                                            _logger.LogError(exception, "");
                                        }
                                    }

                                    if (existingTag != null && tags.All(a => a.Name != existingTag.Name)) tags.Add(existingTag);
                                }
                            }
                            catch (Exception exception)
                            {
                                _logger.LogError(exception, "");
                            }

                            break;
                        }
                        default:
                        {
                            _logger.LogInformation("Found source that wasn't handled: {Source}", source.Source);
                            break;
                        }
                    }
                }

                image.Tags.AddRange(tags);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Finished Saving {Count} tags for {Image}", tags.Count, e.OriginalFilename);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to write {File}", e.OriginalFilename);
            }
        }

        public Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            _logger.LogInformation("Running {ModuleType} module with settings of type {SettingsType}", GetType(),
                SettingsProvider.GetType().GenericTypeArguments.FirstOrDefault());
            return Task.CompletedTask;
        }
    }
}