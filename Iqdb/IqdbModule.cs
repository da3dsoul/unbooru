using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Attributes;
using ImageInfrastructure.Abstractions.Enums;
using ImageInfrastructure.Abstractions.Interfaces;
using ImageInfrastructure.Abstractions.Poco;
using ImageInfrastructure.Abstractions.Poco.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MatchType = IqdbApi.Enums.MatchType;

namespace ImageInfrastructure.Iqdb
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
                    _logger.LogInformation("Querying iqdb for {Image}", image.ImageId);
                    using var client = new IqdbApi.IqdbClient();
                    await using var stream = new MemoryStream(image.Blob);
                    var results = await client.SearchFile(stream);
                    if (!results.IsFound) return;
                    var matches = results.Matches.Where(a => a.MatchType == MatchType.Best).ToList();
                    if (!matches.Any()) return;
                    _logger.LogInformation("iqdb found match for {Image}", image.ImageId);

                    foreach (var match in matches)
                    {
                        image.Sources.Add(new ImageSource
                        {
                            Source = match.Source.ToString(),
                            Uri = match.Url
                        });
                    }
                    _logger.LogInformation("Finished saving sources from iqdb for {Image}", image.ImageId);
                }
                catch (IqdbApi.Exceptions.ImageTooLargeException)
                {

                }
                catch (IqdbApi.Exceptions.HttpRequestFailed)
                {

                }
                catch (IqdbApi.Exceptions.InvalidFileFormatException)
                {

                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Unable to write {File}", image.ImageId);
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