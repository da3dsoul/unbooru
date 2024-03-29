using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using CommandLine;
using unbooru.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using unbooru.Abstractions.Poco;
using unbooru.Abstractions.Poco.Events;

namespace unbooru.ImageSaveHandler
{
    public class ImageSaveHandlerStartup : IInfrastructureStartup, ICommandLineParser
    {
        public string Id => "ImageSaveHandler";
        public string Description => "Saves Images as files";
        public Version Version => new Version("0.0.0.1");

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IModule, ImageSaveHandlerModule>();
        }

        public void Main(StartupEventArgs args)
        {
        }
        public ParserResult<object> ParseArguments(StartupEventArgs args, ParserResult<object> result)
        {
            return result.WithParsed<ImageSaveCLIModel>(o =>
                {
                    var logger = args.Services.GetService<ILogger<ImageSaveHandlerStartup>>();
                    if (!o.Sync) return;
                    logger?.LogInformation("Folder sync starting");
                    var context = args.Services.GetService<IDatabaseContext>();
                    var settingsProvider = args.Services.GetService<ISettingsProvider<ImageSaveHandlerSettings>>();
                    var exclude = settingsProvider?.Get(a => a.ExcludeTags);
                    if (exclude == null) return;

                    var token = new CancellationToken();
                    var ids = context?.Set<Image>().Where(i => (i.TagSources.Any() && !i.TagSources.Select(a => a.Tag.Name).Any(a => exclude.Contains(a))) && (i.Composition == null || !i.Composition.IsMonochrome))
                        .Select(a => a.ImageId).ToList();
                    var idsToRemove = context?.Set<Image>().Where(i => !i.TagSources.Any() || i.TagSources.Select(a => a.Tag.Name).Any(a => exclude.Contains(a)) || (i.Composition != null && i.Composition.IsMonochrome))
                        .Select(a => a.ImageId).ToList();
                    if (ids == null) return;
                    ids.Reverse();

                    var module = (ImageSaveHandlerModule) args.Services.GetServices<IModule>().FirstOrDefault(a => a is ImageSaveHandlerModule);
                    if (module == null) return;

                    var includes = new Expression<Func<Image, object>>[]
                        { a => a.Sources, a => a.ArtistAccounts, a => a.TagSources, a => a.Blobs };

                    var index = 0;
                    var total = 0;//ids.Count;
                    foreach (var id in ids)
                    {
                        var image = context.Set(includes).FirstOrDefault(a => a.ImageId == id);

                        var eventArgs = new ImageProvidedEventArgs
                        {
                            Images = new List<Image> { image },
                            CancellationToken = token
                        };
                        module.ImageProvided(null, eventArgs);
                        if (eventArgs.Cancel) return;
                        index++;
                        var percent = Math.Floor(1000D * index / total);
                        if (percent > Math.Floor(1000D * (index - 1D) / total)) logger?.LogInformation("{Percent}% done processing files", Math.Round(percent / 10D, 1));
                    }

                    index = 0;
                    total = idsToRemove.Count;
                    foreach (var id in idsToRemove)
                    {
                        var image = context.Set(includes).FirstOrDefault(a => a.ImageId == id);
                        var path = module.GetImagePath(image);
                        double percent;
                        if (!File.Exists(path))
                        {
                            index++;
                            percent = Math.Floor(1000D * index / total);
                            if (percent > Math.Floor(1000D * (index - 1D) / total)) logger?.LogInformation("{Percent}% done processing files", Math.Round(percent / 10D, 1));
                            continue;
                        }

                        logger?.LogInformation("Deleting {Image}", path);
                        File.Delete(path);

                        index++;
                        percent = Math.Floor(1000D * index / total);
                        if (percent > Math.Floor(1000D * (index - 1D) / total)) logger?.LogInformation("{Percent}% done processing files", Math.Round(percent / 10D, 1));
                    }

                    args.Cancel = true;
                });
        }
    }
}
