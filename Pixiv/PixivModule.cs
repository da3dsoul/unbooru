using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Interfaces;
using ImageInfrastructure.Abstractions.Interfaces.Contexts;
using ImageInfrastructure.Abstractions.Poco;
using ImageInfrastructure.Abstractions.Poco.Events;
using ImageInfrastructure.Abstractions.Poco.Ingest;
using Meowtrix.PixivApi;
using Microsoft.Extensions.Logging;

namespace ImageInfrastructure.Pixiv
{
    public class PixivModule : IModule, IServiceModule, IImageProvider
    {
        private readonly ILogger<PixivModule> _logger;
        private readonly IArtistContext _artistContext;
        
        public EventHandler<ImageDiscoveredEventArgs> ImageDiscovered { get; set; }
        public EventHandler<ImageProvidedEventArgs> ImageProvided { get; set; }
        
        private ISettingsProvider<PixivSettings> SettingsProvider { get; set; }

        public string Source => "Pixiv";

        public PixivModule(ILogger<PixivModule> logger, ISettingsProvider<PixivSettings> settingsProvider, IArtistContext artistContext)
        {
            _logger = logger;
            SettingsProvider = settingsProvider;
            _artistContext = artistContext;
        }
        
        public async Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            try
            {
                using var pixivClient = new PixivClient();
                await pixivClient.LoginAsync(SettingsProvider.Get(a => a.Token) ?? throw new InvalidOperationException("Settings can't be null"));
                /*var refresh = await pixivClient.LoginAsync(s =>
                {
                    Logger.LogError(s);
                    return Task.FromResult(new Uri("pixiv://account/login?code=-YitIvIn34acMvakDx0xIVR0HKnpmGZintYzKKyuAcg&via=login"));
                });*/
                await DownloadBookmarks(token, pixivClient);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task DownloadBookmarks(CancellationToken token, PixivClient pixivClient)
        {
            var userBookmarks = pixivClient.GetMyBookmarksAsync(cancellation: token);
            var iterator = userBookmarks.GetAsyncEnumerator(token);
            var i = 0;

            do
            {
                if (i >= SettingsProvider.Get(a => a.MaxImagesToDownload)) break;
                if (!await iterator.MoveNextAsync()) break;
                if (i >= SettingsProvider.Get(a => a.MaxImagesToDownload)) break;
                var image = iterator.Current;

                _logger.LogInformation("Processing {Index}/{Total} from Pixiv: {Image} - {Title}", i + 1,
                    SettingsProvider.Get(a => a.MaxImagesToDownload), image.Id, image.Title);

                var newArtist = new ArtistAccount
                {
                    Id = image.User.Id.ToString(),
                    Name = image.User.Name,
                    Url = image.User.Account,
                    Images = new List<Image>()
                };
                var existingArtist = await _artistContext.GetArtist(newArtist);
                if (existingArtist != null) newArtist = existingArtist;

                var disc = new ImageDiscoveredEventArgs
                {
                    Post = new Post
                    {
                        Title = image.Title,
                        Description = image.Description,
                        ArtistName = image.User.Name,
                        ArtistUrl = $"https://pixiv.net/users/{image.User.Id}",
                        PostDate = image.Created.DateTime
                    },
                    Attachments = image.Pages.OrderBy(a => a.Index).Select(a =>
                    {
                        var content = a.Original;
                        return new Attachment
                        {
                            Uri = content.Uri.AbsoluteUri,
                            Size = (image.SizePixels.Width, image.SizePixels.Height),
                        };
                    }).ToList(),
                    Images = image.Pages.OrderBy(a => a.Index).Select(a => new Image
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
                                Uri = a.Original.Uri.AbsoluteUri,
                                OriginalFilename = Path.GetFileName(a.Original.Uri.AbsoluteUri)
                            }
                        },
                        ArtistAccounts = new List<ArtistAccount> { newArtist },
                        Tags = new List<ImageTag>()
                    }).ToList()
                };

                // add images to the Artist mapping
                newArtist.Images.AddRange(disc.Images);

                // Generate Related Images. These are downloaded as pixiv collections, so they are related by nature
                foreach (var tempImage in disc.Images)
                {
                    foreach (var source in tempImage.Sources)
                    {
                        source.RelatedImages = disc.Images
                            .Select(a => new RelatedImage {Image = a, ImageSource = source}).ToList();
                    }

                    tempImage.RelatedImages = tempImage.Sources.FirstOrDefault()?.RelatedImages;
                }

                disc.Attachments.ForEach(a => a.Post = disc.Post);

                ImageDiscovered?.Invoke(this, disc);
                if (disc.Cancel || disc.Attachments.All(a => !a.Download))
                {
                    _logger.LogInformation("Pixiv Image Discovered. Downloading Cancelled by Discovery Subscriber");
                    continue;
                }

                var prov = new ImageProvidedEventArgs
                {
                    Post = disc.Post,
                    Attachments = disc.Attachments,
                    Images = disc.Images
                };

                foreach (var page in image.Pages)
                {
                    if (!prov.Attachments[page.Index].Download) continue;
                    var content = page.Original;

                    await using var stream = await content.RequestStreamAsync(token);
                    await using var memoryStream = new MemoryStream();
                    _logger.LogInformation("Downloading from {Uri}", content.Uri);
                    await stream.CopyToAsync(memoryStream, token);
                    var data = memoryStream.ToArray();
                    prov.Attachments[page.Index].Data = data;
                    prov.Attachments[page.Index].Filesize = data.LongLength;
                    prov.Images[page.Index].Blob = data;
                }
                
                // filter out the ones we won't download and don't exist
                var indicesToRemove = prov.Attachments.Select((a, i1) => (a, i1)).Where(a => !a.a.Download)
                    .Select(a => a.i1).ToList();
                indicesToRemove.ForEach(a =>
                {
                    if (prov.Images[a].ImageId != 0) return;
                    prov.Attachments.RemoveAt(a);
                    prov.Images.RemoveAt(a);
                });

                ImageProvided?.Invoke(this, prov);
                if (prov.Cancel)
                {
                    _logger.LogInformation("Further Pixiv Downloading cancelled by provider subscriber");
                    break;
                }

                i++;
            } while (true);
        }
    }
}