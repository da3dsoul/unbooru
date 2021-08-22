using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Interfaces;
using ImageInfrastructure.Abstractions.Poco;
using ImageInfrastructure.Abstractions.Poco.Events;
using ImageInfrastructure.Abstractions.Poco.Ingest;
using ImageInfrastructure.Core;
using ImageMagick;
using Meowtrix.PixivApi;
using Meowtrix.PixivApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoreLinq.Extensions;

namespace ImageInfrastructure.Pixiv
{
    public class PixivModule : IModule, IServiceModule, IImageProvider
    {
        private readonly ILogger<PixivModule> _logger;

        public EventHandler<ImageDiscoveredEventArgs> ImageDiscovered { get; set; }
        public EventHandler<ImageProvidedEventArgs> ImageProvided { get; set; }

        private ISettingsProvider<PixivSettings> SettingsProvider { get; }

        public string Source => "Pixiv";
        private const int RetryCount = 3;
        private const int SleepTime = 1500;

        public static bool CurrentlyImporting;

        public PixivModule(ILogger<PixivModule> logger, ISettingsProvider<PixivSettings> settingsProvider)
        {
            _logger = logger;
            SettingsProvider = settingsProvider;
        }

        public Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            _logger.LogInformation("Running {ModuleType} module with settings of type {SettingsType}", GetType(),
                SettingsProvider.GetType().GenericTypeArguments.FirstOrDefault());

            var discoveryInvocationList = ImageDiscovered?.GetInvocationList();
            var providerInvocationList = ImageProvided?.GetInvocationList();
            discoveryInvocationList?.ToList().ForEach(a => _logger.LogInformation("Pixiv is providing images to {Item} for cancellation", a.Target?.GetType().FullName));
            providerInvocationList?.ToList().ForEach(a => _logger.LogInformation("Pixiv is providing images to {Item} for consuming", a.Target?.GetType().FullName));
            return Task.CompletedTask;
        }

        [Obsolete("This is a backup method for if the database existed before post dates were implemented")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public async Task DownloadPostDates(IServiceProvider provider, PixivClient pixivClient, CancellationToken token = default)
        {
            try
            {
                CurrentlyImporting = true;
                var i = 0;

                var imageContext = provider.GetRequiredService<CoreContext>();
                var maxPosts = await imageContext.Set<ImageSource>()
                    .Where(a => a.Source == "Pixiv" && a.PostDate == null).Select(a => a.PostId).Distinct().CountAsync(token) - 550;
                var images = await imageContext.Set<ImageSource>()
                    .Where(a => a.Source == "Pixiv" && a.PostDate == null).OrderBy(a => a.Image.ImageId).ThenBy(a => a.ImageSourceId)
                    .Select(a => a.PostId).Skip(550).ToListAsync(token);

                foreach (var postIds in images.Batch(50))
                {
                    using var scope = provider.CreateScope();
                    var scopeContext = scope.ServiceProvider.GetRequiredService<CoreContext>();
                    await using var trans = await scopeContext.Database.BeginTransactionAsync(token);
                    try
                    {
                        int lastError;
                        (i, lastError) = await GetValue(pixivClient, token, postIds, scopeContext, maxPosts, i);
                        if (lastError > 2) goto exit;

                        await scopeContext.SaveChangesAsync(token);
                        await trans.CommitAsync(token);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, e.ToString());
                        await trans.RollbackAsync(token);
                    }
                }
                exit: ;
            }
            finally
            {
                CurrentlyImporting = false;
            }
        }

        private async Task<(int i, int lastError)> GetValue(PixivClient pixivClient, CancellationToken token, IEnumerable<string> postIds,
            CoreContext scopeContext, int maxPosts, int i)
        {
            string lastId = null;
            int lastError = 0;
            foreach (var postId in postIds)
            {
                try
                {
                    if (postId.Equals(lastId)) continue;
                    var imageSources = await scopeContext.Set<ImageSource>().Include(a => a.Image)
                        .Where(a => a.PostId == postId).ToListAsync(token);

                    var source = imageSources.FirstOrDefault();
                    if (source == null) continue;
                    _logger.LogInformation("Processing {Index}/{Total} from Pixiv: {Image} - {Title}",
                        i + 1,
                        maxPosts,
                        source.PostId, source.Title);
                    var image = await pixivClient.GetIllustDetailAsync(int.Parse(source.PostId), token);
                    imageSources.ForEach(a =>
                    {
                        a.PostDate = image.Created.DateTime;
                        a.Image.ImportDate = DateTime.Now;
                    });
                    lastError = 0;
                }
                catch (HttpRequestException e)
                {
                    _logger.LogError(e, e.ToString());
                    if (e.StatusCode != HttpStatusCode.NotFound)
                    {
                        lastError++;
                        if (lastError > 2) return (i, lastError);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.ToString());
                    lastError++;
                    if (lastError > 2) return (i, lastError);
                }

                Thread.Sleep(2000);
                lastId = postId;
                i++;
            }

            return (i, lastError);
        }

        public async Task DownloadBookmarks(IServiceProvider provider, PixivClient pixivClient, int maxPosts, Uri continueFrom = null, bool stopAtFirstCancellation = true, CancellationToken token = default)
        {
            try
            {
                CurrentlyImporting = true;
                var userBookmarks = pixivClient.GetMyBookmarksAsync(cancellation: token, continueFrom: continueFrom);
                var iterator = userBookmarks.GetAsyncEnumerator(token);
                var i = 0;

                do
                {
                    if (i >= maxPosts) break;
                    if (!await iterator.MoveNextAsync()) break;
                    var image = iterator.Current;

                    _logger.LogInformation("Processing {Index}/{Total} from Pixiv: {Image} - {Title}", i + 1, maxPosts,
                        image.Id, image.Title);

                    using var scope = provider.CreateScope();
                    var disc = ImageDiscovery(scope.ServiceProvider, image, token: token);
                    if (disc.Cancel || disc.Attachments.All(a => !a.Download))
                    {
                        _logger.LogInformation("Pixiv Image Discovered. Downloading Cancelled by Discovery Subscriber");
                        if (stopAtFirstCancellation)
                        {
                            _logger.LogInformation("{PixivModule} set to exit on first cancellation. Exiting!",
                                nameof(PixivModule));
                            break;
                        }

                        i++;
                        continue;
                    }

                    var prov = await ImageProviding(disc, image, token: token);
                    if (prov.Cancel)
                    {
                        _logger.LogInformation("Further Pixiv Downloading cancelled by provider subscriber");
                        break;
                    }

                    i++;
                } while (true);

            }
            finally
            {
                CurrentlyImporting = false;
            }
        }

        public ImageDiscoveredEventArgs ImageDiscovery(IServiceProvider provider, Illust image, IList<IllustPage> pages = null, CancellationToken token = default)
        {
            pages ??= image.Pages.ToList();
            var disc = new ImageDiscoveredEventArgs
            {
                ServiceProvider = provider,
                CancellationToken = token,
                Post = new Post
                {
                    Title = image.Title,
                    Description = image.Description,
                    ArtistName = image.User.Name,
                    ArtistUrl = $"https://pixiv.net/users/{image.User.Id}",
                    PostDate = image.Created.DateTime
                },
                Attachments = pages.OrderBy(a => a.Index).Select(a =>
                {
                    var content = a.Original;
                    return new Attachment
                    {
                        Filename = Path.GetFileName(content.Uri.AbsoluteUri),
                        Uri = content.Uri.AbsoluteUri,
                        Size = (image.SizePixels.Width, image.SizePixels.Height)
                    };
                }).ToList()
            };

            disc.Attachments.ForEach(a => a.Post = disc.Post);

            ImageDiscovered?.Invoke(this, disc);

            return disc;
        }

        public async Task<ImageProvidedEventArgs> ImageProviding(ImageDiscoveredEventArgs disc, Illust image, IList<IllustPage> pages = null, CancellationToken token = default)
        {
            pages ??= image.Pages.ToList();
            var newArtist = new ArtistAccount
            {
                Id = image.User.Id.ToString(),
                Name = image.User.Name,
                Url = $"https://pixiv.net/users/{image.User.Id}",
                Images = new List<Image>()
            };
            var imageContext = disc.ServiceProvider.GetRequiredService<IContext<Image>>();
            var artistContext = disc.ServiceProvider.GetRequiredService<IContext<ArtistAccount>>();
            var existingArtist = await artistContext.Get(newArtist, token:token);
            if (existingArtist != null) newArtist = existingArtist;
            var prov = new ImageProvidedEventArgs
            {
                ServiceProvider = disc.ServiceProvider,
                CancellationToken = token,
                Post = disc.Post,
                Attachments = disc.Attachments,
                Images = disc.Attachments.Select(a => new Image
                {
                    Width = image.SizePixels.Width,
                    Height = image.SizePixels.Height,
                    ImportDate = DateTime.Now,
                    Sources = new List<ImageSource>
                    {
                        new()
                        {
                            Source = Source,
                            Title = image.Title,
                            Description = image.Description,
                            Uri = a.Uri,
                            PostUrl = $"https://pixiv.net/en/artworks/{image.Id}",
                            PostId = image.Id.ToString(),
                            OriginalFilename = a.Filename,
                            PostDate = image.Created.DateTime
                        }
                    },
                    ArtistAccounts = new List<ArtistAccount> {newArtist},
                    Tags = new List<ImageTag>()
                }).ToList()
            };

            for (var i = 0; i < pages.Count; i++)
            {
                var existingImage = await imageContext.Get(prov.Images[i], includeDepth:true, token:token);
                if (existingImage != null)
                {
                    prov.Images[i] = existingImage;
                    continue;
                }

                if (!prov.Attachments[i].Download)
                    continue;

                var page = pages[i];
                var content = page.Original;

                var retry = RetryCount;
                while (retry > 0)
                {
                    try
                    {
                        _logger.LogInformation("Downloading from {Uri}", content.Uri);
                        await using var stream = await content.RequestStreamAsync(token);
                        await using var memoryStream = new MemoryStream();
                        await stream.CopyToAsync(memoryStream, token);
                        var data = memoryStream.ToArray();
                        _logger.LogInformation("Downloaded {Count} bytes from {Uri}", data.LongLength, content.Uri);
                        prov.Attachments[i].Data = data;
                        prov.Attachments[i].Filesize = data.LongLength;
                        var pic = new MagickImage(data);
                        prov.Images[i].Width = pic.Width;
                        prov.Images[i].Height = pic.Height;
                        prov.Attachments[i].Size = (pic.Width, pic.Height);
                        prov.Images[i].Size = data.LongLength;
                        prov.Images[i].Blob = data;
                        retry = 0;
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Unable to download {Uri}, retry {Retry}: {E}", content.Uri, RetryCount - retry + 1, e);
                        retry--;
                        if (retry == 0) return prov;
                        Thread.Sleep(SleepTime);
                    }
                }
            }

            // filter out the ones we won't download and don't exist
            if (prov.Attachments.Any(a => !a.Download))
            {
                var newAttachments = new List<Attachment>();
                var newImages = new List<Image>();
                for (var i = 0; i < prov.Attachments.Count; i++)
                {
                    if (!prov.Attachments[i].Download && prov.Images[i].ImageId == 0) continue;
                    newAttachments.Add(prov.Attachments[i]);
                    newImages.Add(prov.Images[i]);
                }

                prov.Attachments = newAttachments;
                prov.Images = newImages;
            }

            // add images to the Artist mapping
            newArtist.Images.AddRange(prov.Images.Where(a => !newArtist.Images.Contains(a)).ToList());

            // Generate Related Images. These are downloaded as pixiv collections, so they are related by nature
            foreach (var tempImage in prov.Images.Where(a => a.ImageId == 0))
            {
                foreach (var source in tempImage.Sources)
                {
                    source.Image = tempImage;
                    source.RelatedImages = prov.Images
                        .Select(a => new RelatedImage {Image = a, ImageSource = source}).ToList();
                }

                tempImage.RelatedImages = tempImage.Sources.FirstOrDefault()?.RelatedImages;
            }

            ImageProvided?.Invoke(this, prov);
            return prov;
        }
    }
}
