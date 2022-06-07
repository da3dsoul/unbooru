using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using unbooru.Abstractions.Interfaces;
using unbooru.Abstractions.Poco;
using unbooru.Pixiv;
using Meowtrix.PixivApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace unbooru.Reimport
{
    public class ReimportModule : IModule
    {
        private ISettingsProvider<ReimportSettings> SettingsProvider { get; set; }
        private readonly ILogger<ReimportModule> _logger;
        
        public ReimportModule(ISettingsProvider<ReimportSettings> settingsProvider, ILogger<ReimportModule> logger)
        {
            SettingsProvider = settingsProvider;
            _logger = logger;
        }

        public async Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            _logger.LogInformation("Running {ModuleType} module with settings of type {SettingsType}", GetType(),
                SettingsProvider.GetType().GenericTypeArguments.FirstOrDefault());
            try
            {
                var factory = provider.GetRequiredService<ILoggerFactory>();
                using var pixivClient = new PixivClient(factory);
                await pixivClient.LoginAsync(SettingsProvider.Get(a => a.Token) ??
                                             throw new InvalidOperationException("Settings can't be null"));

                var pixivModule = provider.GetService<PixivModule>();
                if (pixivModule == null)
                {
                    _logger.LogError("Unable to get Pixiv Module. Exiting!");
                    return;
                }

                var imagesToDownload = SettingsProvider.Get(a => a.ImagesToImport);
                foreach (var grouping in imagesToDownload.ToLookup(a => a[0], a => a[1]))
                {
                    if (!await ImportPage(provider, pixivModule, pixivClient, grouping, token))
                        break;
                }

                _logger.LogInformation("Done Reimporting!");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Error}", e);
            }
        }

        private async Task<bool> ImportPage(IServiceProvider provider, PixivModule pixivModule,
            PixivClient pixivClient, IGrouping<int, int> grouping,
            CancellationToken token)
        {
            var id = grouping.Key;
            var uri = $"https://app-api.pixiv.net/v1/illust/detail?illust_id={id}";
            var response = new ResponseCache
            {
                Uri = uri
            };
            using var scope = provider.CreateScope();
            var responseContext = scope.ServiceProvider.GetRequiredService<IReadWriteContext<ResponseCache>>();
            var imageContext = scope.ServiceProvider.GetRequiredService<IContext<Image>>();
            var responseCache = await responseContext.Get(response, token: token);
            if (responseCache != null)
            {
                if (responseCache.StatusCode == HttpStatusCode.NotFound ||
                    DateTime.Now - responseCache.LastUpdated < TimeSpan.FromDays(30)) return true;
                response = responseCache;
            }

            var dummy = new Image
            {
                Sources = new List<ImageSource>
                {
                    new()
                    {
                        PostUrl = $"https://pixiv.net/en/artworks/{grouping.Key}"
                    }
                }
            };
            var existing = imageContext.FindAll(dummy, token: token);
            if ((await existing).Any())
            {
                _logger.LogInformation("Image exists for Pixiv ID: {Id}. Skipping!", id);
                return true;
            }

            try
            {
                var indices = grouping.ToArray();
                _logger.LogInformation("Getting Info for Pixiv ID: {Id}", id);

                var image = await pixivClient.GetIllustDetailAsync(id, token);

                var pages = image.Pages.Where(a => indices.Contains(a.Index)).ToList();
                var disc = pixivModule.ImageDiscovery(scope.ServiceProvider, image, pages, token);
                if (disc.Cancel || disc.Attachments.All(a => !a.Download))
                {
                    _logger.LogInformation(
                        "Pixiv Image Discovered. Downloading Cancelled by Discovery Subscriber");
                    return true;
                }

                var prov = await pixivModule.ImageProviding(disc, image, pages, token);
                if (prov.Cancel)
                {
                    _logger.LogInformation("Further Pixiv Downloading cancelled by provider subscriber");
                    return false;
                }
            }
            catch (HttpRequestException e)
            {
                response.Response = e.Message;
                response.LastUpdated = DateTime.Now;
                response.StatusCode = e.StatusCode;
                if (responseCache == null) await responseContext.Add(response);
                else await responseContext.SaveChangesAsync(token);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Error}", e);
            }

            return false;
        }
    }
}
