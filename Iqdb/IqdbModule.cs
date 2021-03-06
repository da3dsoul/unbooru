using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using unbooru.Abstractions;
using unbooru.Abstractions.Attributes;
using unbooru.Abstractions.Enums;
using unbooru.Abstractions.Interfaces;
using unbooru.Abstractions.Poco;
using unbooru.Abstractions.Poco.Events;
using ImageMagick;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MatchType = IqdbApi.Enums.MatchType;

namespace unbooru.Iqdb
{
    public class IqdbModule : IModule
    {
        private ISettingsProvider<IqdbSettings> SettingsProvider { get; set; }
        private readonly ILogger<IqdbModule> _logger;

        public IqdbModule(ISettingsProvider<IqdbSettings> settingsProvider, ILogger<IqdbModule> logger)
        {
            SettingsProvider = settingsProvider;
            _logger = logger;
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
            FindSources(e).Wait();
        }

        private async Task FindSources(ImageProvidedEventArgs e)
        {
            foreach (var image in e.Images)
            {
                try
                {
                    _logger.LogInformation("Querying iqdb for {Image}", image.GetPixivFilename());
                    using var client = new IqdbApi.IqdbClient();
                    // shrink to improve bandwidth and make searching easier
                    using var input = new MagickImage(image.Blob) {Format = MagickFormat.Jpeg, Quality = 70};
                    input.Resize(new MagickGeometry("x600>"));
                    var data = input.ToByteArray();
                    await using var stream = new MemoryStream(data);
                    var results = await client.SearchFile(stream, e.CancellationToken);
                    if (!results.IsFound) return;
                    var matches = results.Matches.Where(a => a.MatchType == MatchType.Best).ToList();
                    if (!matches.Any()) return;
                    _logger.LogInformation("iqdb found match for {Image}", image.GetPixivFilename());

                    foreach (var match in matches)
                    {
                        image.Sources.Add(new ImageSource
                        {
                            Source = match.Source.ToString(),
                            Uri = match.PreviewUrl,
                            PostUrl = match.Url,
                            Image = image
                        });
                    }
                    _logger.LogInformation("Finished saving sources from iqdb for {Image}", image.GetPixivFilename());
                }
                catch (IqdbApi.Exceptions.ImageTooLargeException)
                {
                    _logger.LogInformation("Not getting sources from Iqdb. {Image} is too large", image.GetPixivFilename());
                }
                catch (IqdbApi.Exceptions.HttpRequestFailed)
                {
                    _logger.LogInformation("Not getting sources from Iqdb. Http Exception for {Image}", image.GetPixivFilename());
                }
                catch (IqdbApi.Exceptions.InvalidFileFormatException)
                {
                    _logger.LogInformation("Not getting sources from Iqdb. {Image} had an invalid format", image.GetPixivFilename());
                }
                catch (TaskCanceledException)
                {}
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Unable to write {File}: {Exception}", image.GetPixivFilename(), exception);
                }
            }
        }

        public Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            _logger.LogInformation("Running {ModuleType} module with settings of type {SettingsType}", GetType(), SettingsProvider.GetType().GenericTypeArguments.FirstOrDefault());
            return Task.CompletedTask;
        }
    }
}
