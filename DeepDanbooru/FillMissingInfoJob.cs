using System;
using System.Linq;
using System.Threading.Tasks;
using unbooru.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoreLinq;
using Quartz;
using unbooru.Abstractions.Poco;

namespace unbooru.DeepDanbooru
{
    public class FillMissingInfoJob : IJob
    {
        private IServiceProvider ServiceProvider { get; }

        public FillMissingInfoJob(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var logger = ServiceProvider.GetRequiredService<ILogger<FillMissingInfoJob>>();
            var module = ServiceProvider.GetRequiredService<DeepDanbooruModule>();

            logger.LogInformation("Running {ModuleType} module", GetType());

            try
            {
                var _context = ServiceProvider.GetRequiredService<IDatabaseContext>();
                var images = _context.ReadOnlySet<Image>().Where(a => !a.TagSources.Any(b => b.Source == "DeepDanbooruModule"))
                    .Select(a => a.ImageId).OrderByDescending(a => a).ToArray();

                int i = 0;
                foreach (var batch in images.Batch(20))
                {
                    if (i == 20) Console.WriteLine("Done 20 more");
                    if (context.CancellationToken.IsCancellationRequested) return;
                    var dbContext = ServiceProvider.GetRequiredService<IDatabaseContext>();
                    var imageBatch = dbContext.Set<Image>(a => a.Blobs, a => a.TagSources, a => a.Sources)
                        .Where(a => batch.Contains(a.ImageId)).ToList();
                    await module.FindTags(ServiceProvider, imageBatch, context.CancellationToken);
                    dbContext.Save();
                    i++;
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
    }
}
