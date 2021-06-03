using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BooruSharp.Booru;
using BooruSharp.Search.Tag;
using ImageInfrastructure.Abstractions;
using ImageInfrastructure.Abstractions.Attributes;
using ImageInfrastructure.Abstractions.Enums;
using ImageInfrastructure.Abstractions.Interfaces;
using ImageInfrastructure.Abstractions.Poco;
using ImageInfrastructure.Abstractions.Poco.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchResult = BooruSharp.Search.Post.SearchResult;

namespace ImageInfrastructure.Booru
{
    public class BooruModule : IModule
    {
        private ISettingsProvider<BooruSettings> SettingsProvider { get; set; }
        private readonly ILogger<BooruModule> _logger;
        private readonly IContext<ImageTag> _tagContext;
        
        public BooruModule(ISettingsProvider<BooruSettings> settingsProvider, ILogger<BooruModule> logger, IContext<ImageTag> tagContext)
        {
            SettingsProvider = settingsProvider;
            _logger = logger;
            _tagContext = tagContext;
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
            foreach (var image in e.Images)
            {
                try
                {
                    int count = 0;
                    var sources = image.Sources.Where(a => a.Source != "Pixiv").ToList();
                    foreach (var source in sources)
                    {
                        ABooru booru;
                        Func<string, Task<SearchResult>> getPost;

                        switch (source.Source.ToLower())
                        {
                            case "gelbooru":
                            {
                                booru = new Gelbooru();
                                getPost = async url =>
                                {
                                    var i = url.LastIndexOf("md5=", StringComparison.Ordinal) + 4;
                                    var id = url[i..];
                                    i = id.IndexOf('&');
                                    if (i > -1) id = id[..i];    
                                    return await booru.GetPostByMd5Async(id);
                                };
                                break;
                            }
                            case "danbooru":
                            {
                                booru = new DanbooruDonmai();
                                getPost = async url =>
                                {
                                    var i = url.LastIndexOf("/", StringComparison.Ordinal) + 1;
                                    var idString = url[i..];
                                    var id = int.Parse(idString);
                                    return await booru.GetPostByIdAsync(id);
                                };
                                break;
                            }
                            default:
                            {
                                return;
                            }
                        }
                        
                        try
                        {
                            var post = await getPost(source.Uri);
                            foreach (var postTag in post.Tags)
                            {
                                if (postTag == null) continue;
                                var tagName = postTag.Replace("_", " ");

                                try
                                {
                                    var updateTag = false;
                                    var existingTag = new ImageTag
                                    {
                                        Name = tagName,
                                        Images = new List<Image>()
                                    };

                                    var outputTag = await _tagContext.Get(existingTag);
                                    if (outputTag == null) updateTag = true;
                                    else existingTag = outputTag;

                                    if (!image.Tags.Contains(existingTag))
                                    {
                                        existingTag.Images.Add(image);
                                        image.Tags.Add(existingTag);
                                        if (string.IsNullOrEmpty(existingTag.Type)) updateTag = true;
                                    }
                                    count++;

                                    if (!updateTag) continue;
                                
                                    var tagInfo = await booru.GetTagAsync(postTag);
                                    if (tagInfo.Type == TagType.Artist) continue;

                                    existingTag.Type = tagInfo.Type.ToString();
                                }
                                catch (Exception exception)
                                {
                                    _logger.LogError(exception, "{Exception}", exception.ToString());
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            _logger.LogError(exception, "{Exception}", exception.ToString());
                        }
                    }
                    _logger.LogInformation("Finished Saving {Count} tags for {Image}", count, image.GetPixivFilename());
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Unable to get tags for {Image}", image.GetPixivFilename());
                }
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