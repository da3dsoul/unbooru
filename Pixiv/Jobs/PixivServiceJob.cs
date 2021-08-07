using System;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Interfaces;
using Meowtrix.PixivApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace ImageInfrastructure.Pixiv.Jobs
{
    public class PixivServiceJob : IJob
    {
        private IServiceProvider Provider { get; }

        public PixivServiceJob(IServiceProvider provider)
        {
            Provider = provider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (PixivModule.CurrentlyImporting) return;

            var factory = Provider.GetRequiredService<ILoggerFactory>();
            var settingsProvider = Provider.GetRequiredService<ISettingsProvider<PixivSettings>>();

            using var pixivClient = new PixivClient(factory);
            await pixivClient.LoginAsync(settingsProvider.Get(a => a.Token) ??
                                         throw new InvalidOperationException("Settings can't be null"));

            var refreshToken = pixivClient.RefreshToken;
            var accessToken = pixivClient.AccessToken;
            settingsProvider.Update(a =>
            {
                a.Token = refreshToken;
                a.AccessToken = accessToken;
            });

            var pixivModule = Provider.GetRequiredService<PixivModule>();
            await pixivModule.DownloadBookmarks(Provider, pixivClient,
                settingsProvider.Get(a => a.MaxImagesToDownloadService), token: context.CancellationToken);
        }
    }
}
