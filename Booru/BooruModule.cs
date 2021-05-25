using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BooruSharp.Booru;
using BooruSharp.Search.Tag;
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
        private readonly ITagContext _tagContext;
        
        public BooruModule(ISettingsProvider<BooruSettings> settingsProvider, ILogger<BooruModule> logger, ITagContext tagContext)
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
                        Func<object, Task<SearchResult>> getPost;
                        object arg;
                        
                        switch (source.Source.ToLower())
                        {
                            case "gelbooru":
                            {
                                booru = new Gelbooru();
                                var i = source.Uri.LastIndexOf("md5=", StringComparison.Ordinal) + 4;
                                var id = source.Uri[i..];
                                i = id.IndexOf('&');
                                if (i > -1) id = id[..i];
                                arg = id;
                                getPost = o => booru.GetPostByMd5Async((string) o);
                                break;
                            }
                            case "danbooru":
                            {
                                booru = new DanbooruDonmai();
                                var i = source.Uri.LastIndexOf("/", StringComparison.Ordinal) + 1;
                                var idString = source.Uri[i..];
                                if (!int.TryParse(idString, out int id)) continue;
                                arg = id;
                                getPost = o => booru.GetPostByIdAsync((int) o);
                                break;
                            }
                            default:
                            {
                                _logger.LogInformation("Found source that wasn't handled: {Source}", source.Source);
                                return;
                            }
                        }
                        
                        try
                        {
                            var post = await getPost(arg);
                            foreach (var postTag in post.Tags)
                            {
                                if (postTag == null) continue;
                                var tagName = postTag.Replace("_", " ");

                                bool updateTag = false;
                                var existingTag = new ImageTag
                                {
                                    Name = tagName
                                };

                                if (!_tagContext.GetTag(existingTag, out existingTag)) updateTag = true;
                                if (!image.Tags.Contains(existingTag)) image.Tags.Add(existingTag);
                                count++;

                                if (!updateTag) continue;
                                try
                                {
                                    var tagInfo = await booru.GetTagAsync(postTag);
                                    if (tagInfo.Type == TagType.Artist) continue;

                                    existingTag.Type = tagInfo.Type.ToString();
                                }
                                catch (Exception exception)
                                {
                                    _logger.LogError(exception, "");
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            _logger.LogError(exception, "");
                        }
                    }
                    _logger.LogInformation("Finished Saving {Count} tags for {Image}", count, image.ImageId);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Unable to get tags for {ImageId}", image.ImageId);
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