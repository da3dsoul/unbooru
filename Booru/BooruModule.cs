using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BooruSharp.Booru;
using BooruSharp.Search.Tag;
using unbooru.Abstractions;
using unbooru.Abstractions.Attributes;
using unbooru.Abstractions.Enums;
using unbooru.Abstractions.Interfaces;
using unbooru.Abstractions.Poco;
using unbooru.Abstractions.Poco.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchResult = BooruSharp.Search.Post.SearchResult;

namespace unbooru.Booru
{
    public class BooruModule : IModule
    {
        private readonly ILogger<BooruModule> _logger;
        
        public BooruModule(ILogger<BooruModule> logger)
        {
            _logger = logger;
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

        private Task FindTags(ImageProvidedEventArgs e)
        {
            return FindTags(e.ServiceProvider, e.Images, e.CancellationToken);
        }

        private async Task FindTags(IServiceProvider provider, IEnumerable<Image> images, CancellationToken token)
        {
            foreach (var image in images)
            {
                await FindTags(provider, image, token);
                if (token.IsCancellationRequested) return;
            }
        }

        private async Task FindTags(IServiceProvider provider, Image image, CancellationToken token)
        {
            _logger.LogInformation("Finding tags for {Images}", image.GetPixivFilename());
            var sources = image?.Sources?.Where(a => a.Source != "Pixiv").ToList() ?? new List<ImageSource>();
            foreach (var source in sources)
            {
                try
                {
                    var sw = Stopwatch.StartNew();
                    ABooru booru;
                    Func<string, Task<SearchResult>> getPost;

                    (booru, getPost) = GetPost(source);
                    
                    if (booru == null) continue;

                    var post = await getPost(source.PostUrl);
                    sw.Stop();
                    _logger.LogInformation("Got post from booru in {Time}", sw.Elapsed.ToString("g"));

                    var postTags = post.Tags.Where(a => a != null).Select(a => new ImageTag
                    {
                        Name = a.Replace("_", " "),
                        Images = new List<Image>()
                    }).ToList();

                    sw.Restart();
                    var tagContext = provider.GetRequiredService<IContext<ImageTag>>();
                    tagContext.DisableLogging = true;
                    var outputTags = await tagContext.Get(postTags, token: token);
                    if (token.IsCancellationRequested) return;
                    tagContext.DisableLogging = false;
                    sw.Stop();
                    _logger.LogInformation("Got tags from database in {Time}", sw.Elapsed.ToString("g"));

                    sw.Restart();
                    await WriteTagsToModel(image, outputTags, booru);

                    sw.Stop();
                    _logger.LogInformation("Post processing tags finished in {Time}", sw.Elapsed.ToString("g"));
                    _logger.LogInformation("Finished Getting {Count} tags from {Booru} for {Image}", outputTags.Count,
                        booru.GetType().Name, image.GetPixivFilename());
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Unable to get tags for {Image}: {Exception}", image.GetPixivFilename(),
                        exception);
                }
            }
        }

        private static (ABooru booru, Func<string, Task<SearchResult>> getPost) GetPost(ImageSource source)
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
                        string id;
                        if (i <= 3)
                        {
                            // no md5 found, try id
                            i = url.LastIndexOf("id=", StringComparison.Ordinal) + 3;
                            if (i <= 2)
                                throw new NullReferenceException(
                                    "no id or md5 found in gelbooru url. URL was: " + url);

                            id = url[i..];
                            i = id.IndexOf('&');
                            if (i > -1) id = id[..i];
                            return await booru.GetPostByIdAsync(int.Parse(id));
                        }

                        id = url[i..];
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
                    booru = null;
                    getPost = null;
                    break;
            }

            return (booru, getPost);
        }

        private async Task WriteTagsToModel(Image image, List<ImageTag> outputTags, ABooru booru)
        {
            foreach (var tag in outputTags)
            {
                var updateTag = string.IsNullOrEmpty(tag.Type);
                if (!image.Tags.Contains(tag))
                {
                    image.Tags.Add(tag);
                }

                if (!updateTag) continue;
                try
                {
                    var tagInfo = await booru.GetTagAsync(tag.Name.Replace(" ", "_"));
                    if (tagInfo.Type == TagType.Artist) continue;
                    tag.Type = tagInfo.Type.ToString();
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "{Exception}", exception.ToString());
                }
            }
        }

        public Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            return Task.CompletedTask;
            /*var context = provider.GetRequiredService<CoreContext>();
            var imageIds = context.Set<Image>().OrderByDescending(a => a.ImageId).Select(a => a.ImageId).Skip(1526).Take(6774);

            var i = 1;
            foreach (var imageId in imageIds)
            {
                var scope = provider.CreateScope();
                var scopeContext = scope.ServiceProvider.GetRequiredService<CoreContext>();
                var image = await scopeContext.Images.AsSplitQuery().Include(a => a.Tags).Include(a => a.Sources)
                    .Include(a => a.ArtistAccounts).ThenInclude(a => a.Images).Include(a => a.RelatedImages)
                    .OrderByDescending(a => a.ImageId).FirstOrDefaultAsync(a => a.ImageId == imageId, token);
                if (image == null || token.IsCancellationRequested) return;
                await FindTags(scope.ServiceProvider, image, token);
                if (token.IsCancellationRequested) return;

                await using var trans = await scopeContext.Database.BeginTransactionAsync(token);
                try
                {
                    await scopeContext.ImageTags.AddRangeAsync(image.Tags.Where(a => a.ImageTagId == 0).Distinct(), token);
                    await scopeContext.SaveChangesAsync(token);
                    await trans.CommitAsync(token);
                    _logger.LogInformation("Finished saving {Index}/6774 images to database for {Image}", i, image.GetPixivFilename());
                }
                catch (Exception exception)
                {
                    await trans.RollbackAsync(token);
                    _logger.LogError(exception, "Unable to write {File}: {Exception}", image.GetPixivFilename(), exception);
                }

                i++;
            }*/
        }
    }
}
