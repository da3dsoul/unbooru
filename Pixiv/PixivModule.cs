using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Interfaces;
using ImageInfrastructure.Abstractions.Poco;
using ImageInfrastructure.Abstractions.Poco.Events;
using Meowtrix.PixivApi;
using Microsoft.Extensions.Logging;

namespace ImageInfrastructure.Pixiv
{
    public class PixivModule : IModule, IServiceModule, IImageProvider
    {
        private readonly ILogger<PixivModule> _logger;
        
        public EventHandler<ImageDiscoveredEventArgs> ImageDiscovered { get; set; }
        public EventHandler<ImageProvidedEventArgs> ImageProvided { get; set; }
        
        private ISettingsProvider<PixivSettings> SettingsProvider { get; set; }

        public string Source => "Pixiv";

        public PixivModule(ILogger<PixivModule> logger, ISettingsProvider<PixivSettings> settingsProvider)
        {
            _logger = logger;
            SettingsProvider = settingsProvider;
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
                var userBookmarks = pixivClient.GetMyBookmarksAsync(cancellation:token);
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
                    
                    var disc = new ImageDiscoveredEventArgs
                    {
                        Post = new Post
                        {
                            Title = image.Title,
                            Description = image.Description,
                            ArtistName = image.User.Name,
                            ArtistUrl = $"https://pixiv.net/users/{image.User.Id}",
                        },
                        Attachments = image.Pages.OrderBy(a => a.Index).Select(a =>
                        {
                            var content = a.Original;
                            return new Attachment
                            {
                                Uri = content.Uri.AbsoluteUri,
                                // this is used for identity verification, so it being accurate isn't as important as being unique
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
                            ArtistAccounts = new List<ArtistAccount>
                            {
                                new()
                                {
                                    Name = image.User.Name,
                                    Url = image.User.Account
                                }
                            },
                            Tags = new List<ImageTag>()
                        }).ToList()
                    };
                    disc.Attachments.ForEach(a => a.Post = disc.Post);
                    
                    ImageDiscovered?.Invoke(this, disc);
                    if (disc.Cancel || disc.Attachments.All(a => a.Cancelled))
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
                        var content = page.Original;
                        if (disc.Attachments[page.Index].Cancelled)
                        {
                            _logger.LogInformation("Downloading Pixiv Url Cancelled by Discovery Subscriber: {Url}", content.Uri);
                            continue;
                        }

                        await using var stream = await content.RequestStreamAsync(token);
                        await using var memoryStream = new MemoryStream();
                        _logger.LogInformation("Downloading from {Uri}", content.Uri);
                        await stream.CopyToAsync(memoryStream, token);
                        var data = memoryStream.ToArray();
                        prov.Attachments[page.Index].Data = data;
                        prov.Attachments[page.Index].Filesize = data.LongLength;
                        prov.Images[page.Index].Blob = data;
                    }
                    
                    ImageProvided?.Invoke(this, prov);
                    if (prov.Cancel)
                    {
                        _logger.LogInformation("Further Pixiv Downloading cancelled by provider subscriber");
                        break;
                    }

                    i++;
                } while (true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}