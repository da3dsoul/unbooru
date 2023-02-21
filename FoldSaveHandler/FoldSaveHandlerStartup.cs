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

namespace unbooru.FoldSaveHandler
{
    public class FoldSaveHandlerStartup : IInfrastructureStartup, ICommandLineParser
    {
        public string Id => "FoldSaveHandler";
        public string Description => "Saves Images as files";
        public Version Version => new Version("0.0.0.1");

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IModule, FoldSaveHandlerModule>();
        }

        public void Main(StartupEventArgs args)
        {
            
        }
        public ParserResult<object> ParseArguments(StartupEventArgs args, ParserResult<object> result)
        {
            return result.WithParsed<FoldCLIModel>(o =>
                {
                    var logger = args.Services.GetService<ILogger<FoldSaveHandlerStartup>>();
                    if (!o.Sync) return;
                    logger?.LogInformation("Folder sync starting");
                    var settingsProvider = args.Services.GetService<ISettingsProvider<FoldSaveHandlerSettings>>();
                    var exclude = settingsProvider?.Get(a => a.ExcludeTags);
                    if (exclude == null) return;

                    var token = new CancellationToken();
                    List<int> ids;
                    List<int> idsToRemove;
                    using (var context = args.Services.GetService<IDatabaseContext>())
                    {
                        idsToRemove = new List<int>();
                        ids = context?.ReadOnlySet<Image>().Where(i => i.TagSources.Any() && !i.TagSources.Select(a => a.Tag).Any(a => exclude.Contains(a.Name)) && i.Composition != null && !i.Composition.IsMonochrome).OrderByDescending(a => a.ImageId)
                            .Select(a => a.ImageId).ToList();
                    }
                    if (ids == null) return;

                    var module = (FoldSaveHandlerModule) args.Services.GetServices<IModule>().FirstOrDefault(a => a is FoldSaveHandlerModule);
                    if (module == null) return;

                    var includes = new Expression<Func<Image, object>>[]
                        { a => a.Sources, a => a.ArtistAccounts, a => a.TagSources, a => a.Blobs };

                    var index = 0;
                    var total = idsToRemove.Count;
                    
                    foreach (var id in idsToRemove)
                    {
                        using var scope = args.Services.CreateScope();
                        using var context = scope.ServiceProvider.GetRequiredService<IDatabaseContext>();
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

                    index = 0;
                    total = ids.Count;

                    foreach (var id in ids)
                    {
                        using var scope = args.Services.CreateScope();
                        using var context = scope.ServiceProvider.GetRequiredService<IDatabaseContext>();
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

                    args.Cancel = true;
                });
        }
    }
}
