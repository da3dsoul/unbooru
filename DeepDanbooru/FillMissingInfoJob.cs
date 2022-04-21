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
        private readonly IDatabaseContext _context;

        public FillMissingInfoJob(IServiceProvider serviceProvider, IDatabaseContext context)
        {
            ServiceProvider = serviceProvider;
            _context = context;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var logger = ServiceProvider.GetRequiredService<ILogger<FillMissingInfoJob>>();
            var module = ServiceProvider.GetRequiredService<DeepDanbooruModule>();

            logger.LogInformation("Running {ModuleType} module", GetType());

            try
            {
                var images = _context.Set<Image>().Where(a => !a.TagSources.Any(b => b.Source == "DeepDanbooru"))
                    .Select(a => a.ImageId).OrderByDescending(a => a).ToList().Batch(20);

                foreach (var batch in images)
                {
                    if (context.CancellationToken.IsCancellationRequested) return;
                    var imageBatch = _context.Set<Image>(a => a.Blobs, a => a.TagSources, a => a.Sources)
                        .Where(a => batch.Contains(a.ImageId)).ToList();
                    await module.FindTags(ServiceProvider, imageBatch, context.CancellationToken);
                    _context.Save();
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
