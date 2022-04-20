using System;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using unbooru.Abstractions.Interfaces;
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
            throw new NotImplementedException();
        }
    }
}