using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using unbooru.Abstractions.Interfaces;
using Meowtrix.PixivApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoreLinq;
using Quartz;
using unbooru.Abstractions.Poco;
using unbooru.Core;

namespace unbooru.Pixiv.Jobs
{
    public class PixivDownloadMissingAvatarsJob : IJob
    {
        private IServiceProvider ServiceProvider { get; }

        public PixivDownloadMissingAvatarsJob(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var logger = ServiceProvider.GetRequiredService<ILogger<PixivDownloadMissingAvatarsJob>>();
            var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
            var settingsProvider = ServiceProvider.GetRequiredService<ISettingsProvider<PixivSettings>>();
            var dbContext = ServiceProvider.GetRequiredService<CoreContext>();

            try
            {
                var artists = await dbContext.Set<ArtistAccount>()
                    .Where(a => a.Avatar == null || a.Avatar == Array.Empty<byte>()).Select(a => a.ArtistAccountId).ToListAsync();
                
                if (artists.Count == 0) return;
                
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

                foreach (var artistIds in artists.Batch(50))
                {
                    using var scope = ServiceProvider.CreateScope();
                    await using var scopeContext = scope.ServiceProvider.GetRequiredService<CoreContext>();
                    await using var trans = await scopeContext.Database.BeginTransactionAsync(context.CancellationToken);
                    try
                    {
                        foreach (var artistId in artistIds)
                        {
                            var artistAccount = await scopeContext.Set<ArtistAccount>().FirstOrDefaultAsync(a => a.ArtistAccountId == artistId);
                            await DownloadArtistAvatar(logger, pixivClient, artistAccount, context.CancellationToken);
                        }

                        await scopeContext.SaveChangesAsync(context.CancellationToken);
                        await trans.CommitAsync(context.CancellationToken);
                    }
                    catch (Exception e)
                    {
                        await trans.RollbackAsync(context.CancellationToken);
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception e)
            {
                logger.LogError(e, "{Message}", e);
            }
        }
        
        private static async Task DownloadArtistAvatar(ILogger logger, PixivClient client, ArtistAccount newArtist, CancellationToken token)
        {
            var retry = 3;
            while (retry > 0)
            {
                try
                {
                    logger.LogInformation("Getting User info for {id}", newArtist.Id);
                    var user = await client.GetUserDetailAsync(int.Parse(newArtist.Id), token);
                    var avatar = user.Avatar;
                    logger.LogInformation("Downloading from {Uri}", avatar.Uri);
                    await using var stream = await avatar.RequestStreamAsync(token);
                    await using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream, token);
                    var data = memoryStream.ToArray();
                    logger.LogInformation("Downloaded {Count} bytes from {Uri}", data.LongLength, avatar.Uri);
                    newArtist.Avatar = data;
                    retry = 0;
                }
                catch (Exception e)
                {
                    logger.LogError("Unable to download {Uri}, retry {Retry}: {E}", newArtist.Id,
                        4 - retry, e);
                    retry--;
                    if (retry == 0) return;
                    Thread.Sleep(2000);
                }
            }
        }
    }
}
