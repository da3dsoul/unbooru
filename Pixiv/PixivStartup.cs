using System;
using System.Diagnostics;
using CommandLine;
using unbooru.Abstractions.Interfaces;
using unbooru.Pixiv.Jobs;
using Meowtrix.PixivApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using unbooru.Abstractions.Poco.Events;

namespace unbooru.Pixiv
{
    public class PixivStartup : IInfrastructureStartup, ICommandLineParser
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

        public ParserResult<object> ParseArguments(StartupEventArgs args, ParserResult<object> result)
        {
            return result.WithParsed<PixivCLIModel>(o =>
                {
                    var logger = args.Services.GetRequiredService<ILogger<PixivStartup>>();
                    logger.LogInformation("---- Pixiv Login ----");
                    logger.LogInformation("A browser will open, F12 to capture the response.");

                    var loggerFactory = args.Services.GetRequiredService<ILoggerFactory>();
                    var settingsProvider = args.Services.GetRequiredService<ISettingsProvider<PixivSettings>>();

                    var client = new PixivClient(loggerFactory);
                    var refreshToken = client.LoginAsync(url =>
                    {
                        logger.LogInformation("Open this url");
                        logger.LogInformation(url);
                        try
                        {
                            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                        }
                        catch (Exception e)
                        {
                            logger.LogWarning(e, "Could not automatically open a browser, open the URL manually");
                        }
                        logger.LogInformation("Enter the URL or code:");
                        var readLine = Console.ReadLine()?.Trim();
                        if (readLine != null && !readLine.Contains("://"))
                        {
                            // a bare code was pasted instead of the full redirect URL
                            readLine = "pixiv://account/login?code=" + readLine;
                        }

                        return readLine;
                    }).Result;

                    settingsProvider.Update(a =>
                    {
                        a.Token = refreshToken;
                        a.AccessToken = client.AccessToken;
                    });

                    args.Cancel = true;
                });
        }
    }
}
