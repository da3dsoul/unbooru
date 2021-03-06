using System;
using unbooru.Abstractions.Interfaces;
using unbooru.Pixiv.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using unbooru.Abstractions.Poco.Events;

namespace unbooru.Pixiv
{
    public class PixivStartup : IInfrastructureStartup
    {
        public string Id => "Pixiv";
        public string Description => "Downloads images from Pixiv";
        public Version Version => new("0.0.0.1");

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<PixivModule>();
            services.AddSingleton<IModule>(x => x.GetRequiredService<PixivModule>());
            services.AddSingleton<IServiceModule>(x => x.GetRequiredService<PixivModule>());
            services.AddSingleton<IImageProvider>(x => x.GetRequiredService<PixivModule>());
            services.AddOptions<QuartzOptions>().Configure(options =>
            {
                var importKey = new JobKey("PixivImport", "Pixiv");
                options.AddJob<PixivImportJob>(builder => builder.WithIdentity(importKey));
                options.AddTrigger(builder =>
                    builder.WithIdentity("PixivImportTrigger", "Pixiv").ForJob(importKey)
                        .WithSchedule(SimpleScheduleBuilder.Create().WithIntervalInHours(168).RepeatForever())
                        .StartAt(DateTimeOffset.Now + TimeSpan.FromSeconds(5)));

                var serviceKey = new JobKey("PixivService", "Pixiv");
                options.AddJob<PixivServiceJob>(builder => builder.WithIdentity(serviceKey));
                options.AddTrigger(builder =>
                    builder.WithIdentity("PixivServiceTrigger", "Pixiv").ForJob(serviceKey)
                        .WithSchedule(SimpleScheduleBuilder.Create().WithIntervalInMinutes(30).RepeatForever())
                        .StartAt(DateTimeOffset.Now + TimeSpan.FromHours(1)));
                
                var avatarKey = new JobKey("PixivAvatar", "Pixiv");
                options.AddJob<PixivDownloadMissingAvatarsJob>(builder => builder.WithIdentity(avatarKey));
                options.AddTrigger(builder =>
                    builder.WithIdentity("PixivAvatarTrigger", "Pixiv").ForJob(avatarKey)
                        .StartAt(DateTimeOffset.Now + TimeSpan.FromSeconds(5)));
            });
        }

        public void Main(StartupEventArgs args)
        {
            
        }
    }
}
