using System;
using System.Linq;
using System.Threading.Tasks;
using unbooru.Abstractions.Interfaces;
using Meowtrix.PixivApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace unbooru.Pixiv.Jobs
{
    public class PixivImportJob : IJob
    {
        private IServiceProvider ServiceProvider { get; }

        public PixivImportJob(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var logger = ServiceProvider.GetRequiredService<ILogger<PixivImportJob>>();
            var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
            var settingsProvider = ServiceProvider.GetRequiredService<ISettingsProvider<PixivSettings>>();
            var pixivModule = ServiceProvider.GetRequiredService<PixivModule>();

            logger.LogInformation("Running {ModuleType} module with settings of type {SettingsType}", GetType(),
                settingsProvider.GetType().GenericTypeArguments.FirstOrDefault());

            try
            {
                using var pixivClient = new PixivClient(loggerFactory);
                await pixivClient.LoginAsync(settingsProvider.Get(a => a.Token) ??
                                             throw new InvalidOperationException("Settings can't be null"));

                var refreshToken = pixivClient.RefreshToken;
                var accessToken = pixivClient.AccessToken;
                settingsProvider.Update(a =>
                {
                    a.Token = refreshToken;
                    a.AccessToken = accessToken;
                });
                Uri continueFrom = null;
                if (!string.IsNullOrEmpty(settingsProvider.Get(a => a.ContinueFrom)))
                    continueFrom = new Uri(settingsProvider.Get(a => a.ContinueFrom));
                await pixivModule.DownloadBookmarks(ServiceProvider, pixivClient,
                    settingsProvider.Get(a => a.MaxImagesToDownloadImport), 0, continueFrom,
                    token: context.CancellationToken);
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception e)
            {
                logger.LogError(e, "{Message}", e);
            }
        }
    }
}
