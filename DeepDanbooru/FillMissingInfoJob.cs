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
            var evaluator = ServiceProvider.GetRequiredService<Evaluator>();

            logger.LogInformation("Running {ModuleType} module", GetType());

            try
            {
                int[] images;
                using (var scope1 = ServiceProvider.CreateScope())
                {
                    var dbContext = scope1.ServiceProvider.GetRequiredService<IDatabaseContext>();
                    images = dbContext.ReadOnlySet<Image>()
                        .Where(a => !a.TagSources.Any(b => b.Source == "DeepDanbooruModule"))
                        .Select(a => a.ImageId).OrderByDescending(a => a).ToArray();
                }

                foreach (var batch in images.Batch(20))
                {
                    if (context.CancellationToken.IsCancellationRequested) return;
                    using (var scope2 = ServiceProvider.CreateScope())
                    {
                        var dbContext = scope2.ServiceProvider.GetRequiredService<IDatabaseContext>();
                        var imageBatch = dbContext.Set<Image>(a => a.Blobs, a => a.TagSources, a => a.Sources)
                            .Where(a => batch.Contains(a.ImageId)).ToList();
                        await evaluator.FindTags(scope2.ServiceProvider, imageBatch, context.CancellationToken);
                        dbContext.Save();
                    }

                    GC.Collect();
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
