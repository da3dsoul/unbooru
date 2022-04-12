using System;
using System.Collections;
using System.Collections.Generic;
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
    public class ImageSaveHandlerStartup : IInfrastructureStartup
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
            Parser.Default.ParseArguments<CLIModel>(args.Args)
                .WithParsed(o =>
                {
                    var logger = args.Services.GetService<ILogger<ImageSaveHandlerStartup>>();
                    if (!o.Sync)
                    {
                        logger?.LogInformation("Sync not enabled");
                        return;
                    }
                    logger?.LogInformation("Folder sync starting");
                    var context = args.Services.GetService<IContext<Image>>();
                    var settingsProvider = args.Services.GetService<ISettingsProvider<ImageSaveHandlerSettings>>();
                    var exclude = settingsProvider?.Get(a => a.ExcludeTags);
                    if (exclude == null) return;

                    var token = new CancellationToken();
                    var ids = context?.Execute(c =>
                        c.Set<Image>()
                            .Where(i => i.Tags.Any() && !i.Tags.Select(a => a.Name).Any(a => exclude.Contains(a)))
                            .Select(a => a.ImageId).ToList());
                    if (ids == null) return;

                    var module = (ImageSaveHandlerModule) args.Services.GetServices<IModule>().FirstOrDefault(a => a is ImageSaveHandlerModule);
                    if (module == null) return;

                    var includes = new Expression<Func<Image, IEnumerable>>[]
                        { a => a.Sources, a => a.ArtistAccounts, a => a.Tags, a => a.Blobs };

                    foreach (var id in ids)
                    {
                        var image = context.Execute(c => c.Set(includes).FirstOrDefault(a => a.ImageId == id));

                        var eventArgs = new ImageProvidedEventArgs
                        {
                            Images = new List<Image> { image },
                            CancellationToken = token
                        };
                        module.ImageProvided(null, eventArgs);
                        if (eventArgs.Cancel) return;
                    }

                    args.Cancel = true;
                });
        }
    }
}
