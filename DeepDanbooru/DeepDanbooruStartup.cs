using System;
using System.Linq;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Matchers;
using unbooru.Abstractions.Interfaces;
using unbooru.Abstractions.Poco;
using unbooru.Abstractions.Poco.Events;

namespace unbooru.DeepDanbooru
{
    public class DeepDanbooruStartup : IInfrastructureStartup
    {
        public string Id => "DeepDanbooru";
        public string Description => "lorem ipsum";
        public Version Version => new Version("0.0.0.1");

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<DeepDanbooruModule>();
            services.AddSingleton<IModule>(x => x.GetRequiredService<DeepDanbooruModule>());
            services.AddOptions<QuartzOptions>().Configure(options =>
            {
                var importKey = new JobKey("FillMissingInfo", "DeepDanbooru");
                options.AddJob<FillMissingInfoJob>(builder => builder.WithIdentity(importKey));
                options.AddTrigger(builder =>
                    builder.WithIdentity("FillMissingInfoTrigger", "DeepDanbooru").ForJob(importKey)
                        .StartAt(DateTimeOffset.Now + TimeSpan.FromSeconds(5)));
            });
        }

        public void Main(StartupEventArgs args)
        {
            var parser = new Parser(o => o.IgnoreUnknownArguments = true);
            parser.ParseArguments<CLIModel>(args.Args)
                .WithParsed(o =>
                {
                    StopQuartz(args);
                    try
                    {
                        var logger = args.Services.GetService<ILogger<DeepDanbooruStartup>>();
                        for (var i = 0; i < 5; i++)
                        {

                            logger?.LogInformation("Tagging Image ID: {ID}", o.ID);
                            var context = args.Services.GetRequiredService<IDatabaseContext>();
                            var module = args.Services.GetRequiredService<DeepDanbooruModule>();
                            var imageBatch = context.ReadOnlySet<Image>(a => a.Blobs, a => a.TagSources, a => a.Sources)
                                .Where(a => a.ImageId == o.ID).ToList();
                            var tags = module.PredictMultipleImages(imageBatch);
                            logger?.LogInformation("Found Tags: \n{Tags}",
                                string.Join("\n", tags.FirstOrDefault().Tags));
                        }
                    }
                    catch
                    {
                        // swallow
                    }
                    args.Cancel = true;
                });
        }

        private static void StopQuartz(StartupEventArgs args)
        {
            var scheduler = args.Services.GetService<ISchedulerFactory>()?.GetScheduler().Result;
            if (scheduler == null) return;
            var triggers = scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup()).Result;
            scheduler.UnscheduleJobs(triggers);
            if (scheduler.IsStarted) scheduler.Shutdown();
            else scheduler.PauseAll();
        }
    }
}