using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using unbooru.Abstractions;
using unbooru.Abstractions.Attributes;
using unbooru.Abstractions.Enums;
using unbooru.Abstractions.Interfaces;
using unbooru.Abstractions.Poco;
using unbooru.Abstractions.Poco.Events;
using ImageMagick;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace unbooru.SauceNao
{
    public class SauceNaoModule : IModule
    {
        private ISettingsProvider<SauceNaoSettings> SettingsProvider { get; set; }
        private readonly ILogger<SauceNaoModule> _logger;
        private readonly SimpleRateLimiter _limiter;
        
        public SauceNaoModule(ISettingsProvider<SauceNaoSettings> settingsProvider, ILogger<SauceNaoModule> logger, SimpleRateLimiter limiter)
        {
            SettingsProvider = settingsProvider;
            _logger = logger;
            _limiter = limiter;
        }
        
        [ModulePostConfiguration(Priority = ModuleInitializationPriority.SourceData)]
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
            FindSources(e).Wait(e.CancellationToken);
        }

        private async Task FindSources(ImageProvidedEventArgs e)
        {
            foreach (var image in e.Images)
            {
                try
                {
                    var lastBan = SettingsProvider.Get(a => a.LastDailyBan);
                    if (lastBan != null && lastBan.Value.Date >= DateTime.Today)
                    {
                        _logger.LogInformation("Reached Daily Limit for SauceNao. Skipping");
                        return;
                    }

                    _logger.LogInformation("Querying SauceNao for {Image}", image.GetPixivFilename());
                    // shrink to improve bandwidth and make searching easier
                    using var input = new MagickImage(image.Blob) {Format = MagickFormat.Jpeg, Quality = 70};
                    input.Resize(new MagickGeometry("x600>"));
                    var data = input.ToByteArray();
                    _limiter.EnsureRate();
                    var page = await Search(data, e.CancellationToken);
                    _logger.LogInformation("Finished Querying SauceNao for {Image}", image.GetPixivFilename());
                    if (page == null)
                    {
                        _logger.LogError("Unable to get sauce for {File}, but no message was given", image.GetPixivFilename());
                        continue;
                    }

                    _logger.LogInformation("Parsing SauceNao response for {Image}", image.GetPixivFilename());
                    var urls = ParsePage(page);
                    foreach (var url in urls)
                    {
                        image.Sources.Add(new ImageSource
                        {
                            Source = url.Contains("danbooru") ? "Danbooru" : url.Contains("gelbooru") ? "Gelbooru" : "Unknown",
                            Uri = url,
                            Image = image
                        });
                    }
                    _logger.LogInformation("Finished Parsing SauceNao Response for {Image}", image.GetPixivFilename());
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Unable to write {File}: {E}", image.GetPixivFilename(), exception);
                }
            }
        }

        public Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            _logger.LogInformation("Running {ModuleType} module with settings of type {SettingsType}", GetType(),
                SettingsProvider.GetType().GenericTypeArguments.FirstOrDefault());
            return Task.CompletedTask;
        }

        private async Task<string> Search(byte[] image, CancellationToken token)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Android Multipart HTTP Client 1.0");
            var content = new MultipartFormDataContent();
            var imageContent = new ByteArrayContent(image);
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
            content.Add(imageContent, "\"file\"", "\"file.jpg\"");
            var response = await client.PostAsync("https://saucenao.com/search.php", content, token);
            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();
                if (message.Contains("Daily Search Limit"))
                {
                    SettingsProvider.Update(a => a.LastDailyBan = DateTime.Now);
                    response.EnsureSuccessStatusCode();
                }
            }
            return await response.Content.ReadAsStringAsync();
        }

        private List<string> ParsePage(string page)
        {
            var output = new List<string>();
            try {
                var doc = new HtmlDocument();
                doc.LoadHtml(page);
                var results = doc.DocumentNode.SelectNodes("//div[@class='result']");
                var urls = new HashSet<string>();
                foreach (var e in results)
                {
                    try
                    {
                        var foundUrls = e.SelectNodes(".//a")?.Select(a => a.Attributes["href"]?.Value)
                            .Where(a => a != null && !urls.Contains(a) && (a.Contains("danbooru") || a.Contains("gelbooru"))).ToList() ?? new List<string>();
                        foreach (var url in foundUrls)
                        {
                            urls.Add(url);
                            output.Add(url);
                        }
                    } catch (Exception ex)
                    {
                        _logger.LogError(ex, "{Error}", ex);
                    }
                }
            } catch (Exception e) {
                _logger.LogError(e, "{Error}", e);
            }

            return output;
        }
    }
}