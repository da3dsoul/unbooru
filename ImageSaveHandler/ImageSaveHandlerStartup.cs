using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                    var logger = args.Services.GetService<ILogger>();
                    if (!o.Sync) return;
                    logger?.LogInformation("Folder sync starting");
                    var context = args.Services.GetService<IContext<Image>>();
                    var settingsProvider = args.Services.GetService<ISettingsProvider<ImageSaveHandlerSettings>>();
                    var exclude = settingsProvider?.Get(a => a.ExcludeTags);
                    if (exclude == null) return;

                    var token = new CancellationToken();
                    var eventArgsList = context?.Execute(c => c.Set<Image>().Where(i =>
                        !i.Tags.Select(a => a.Name).Any(a =>
                            exclude.Any(b => a.Equals(b, StringComparison.InvariantCultureIgnoreCase)))).Select(image =>
                        new ImageProvidedEventArgs
                        {
                            ServiceProvider = args.Services,
                            CancellationToken = token,
                            Images = new List<Image> { image }
                        }).ToList());
                    if (eventArgsList == null) return;

                    var module = args.Services.GetService<ImageSaveHandlerModule>();
                    if (module == null) return;

                    foreach (var eventArgs in eventArgsList)
                    {
                        module.ImageProvided(null, eventArgs);
                        if (eventArgs.Cancel) return;
                    }

                    args.Cancel = true;
                });
        }
    }
}