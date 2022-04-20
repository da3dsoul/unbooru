using System;
using System.Linq;
using System.Threading;
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
                var images = _context.Set<Image>().Where(a => !a.TagSources.Any(b => b.Source == "DeepDanbooru")).OrderBy(a => a.ImportDate).ToList()
                    .Batch(20);

                foreach (var batch in images)
                {
                    await module.FindTags(ServiceProvider, batch.ToList(), context.CancellationToken);
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
