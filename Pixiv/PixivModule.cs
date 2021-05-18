using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Interfaces;
using ImageInfrastructure.Abstractions.Poco;
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
                    var image = iterator.Current;
                    foreach (var page in image.Pages)
                    {
                        if (i >= SettingsProvider.Get(a => a.MaxImagesToDownload)) goto outer;
                        var content = page.Original;

                        var disc = new ImageDiscoveredEventArgs
                        {
                            ImageUri = content.Uri,
                            // this is used for identity verification, so it being accurate isn't as important as being unique
                            Size = (long) image.SizePixels.Width << 32 | (uint) image.SizePixels.Height,
                            Title = image.Title,
                            Description = image.Description,
                            ArtistName = image.User.Name,
                            ArtistUrl = $"https://pixiv.net/users/{image.User.Id}"
                        };
                        ImageDiscovered?.Invoke(this, disc);
                        if (disc.Cancel)
                        {
                            _logger.LogInformation("Pixiv Image Discovered. Downloading Cancelled by Discovery Subscriber");
                            continue;
                        }
                        
                        var fileName = Path.GetFileName(content.Uri.AbsolutePath);
                        await using (var stream = await content.RequestStreamAsync(token))
                        {
                            await using var memoryStream = new MemoryStream();
                            _logger.LogInformation("Downloading {Index}/{Total} from {Uri}", i+1, SettingsProvider.Get(a => a.MaxImagesToDownload), content.Uri);
                            await stream.CopyToAsync(memoryStream, token);
                            var data = memoryStream.ToArray();
                            var prov = new ImageProvidedEventArgs
                            {
                                OriginalFilename = fileName,
                                Data = data,
                                Uri = content.Uri.AbsoluteUri,
                                Title = image.Title,
                                Description = image.Description,
                                ArtistName = image.User.Name,
                                ArtistUrl = $"https://pixiv.net/users/{image.User.Id}"
                            };
                            ImageProvided?.Invoke(this, prov);
                            if (prov.Cancel)
                            {
                                _logger.LogInformation("Further Pixiv Downloading cancelled by provider subscriber");
                                goto outer;
                            }

                            i++;
                        }
                    }
                } while (true);
                outer: ;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}