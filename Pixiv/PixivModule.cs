using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Interfaces;
using ImageInfrastructure.Abstractions.Poco;
using ImageInfrastructure.Abstractions.Poco.Events;
using ImageInfrastructure.Abstractions.Poco.Ingest;
using ImageMagick;
using Meowtrix.PixivApi;
using Meowtrix.PixivApi.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
                    Sources = new List<ImageSource>
                    {
                        new()
                        {
                            Source = Source,
                            Title = image.Title,
                            Description = image.Description,
                            Uri = a.Uri,
                            PostUrl = $"https://pixiv.net/en/artworks/{image.Id}",
                            OriginalFilename = a.Filename
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
