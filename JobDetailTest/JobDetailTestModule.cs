using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using unbooru.Abstractions.Interfaces;

namespace unbooru.JobDetailTest
{
    public class JobDetailTestModule : IModule
    {
        private ISettingsProvider<JobDetailTestSettings> SettingsProvider { get; set; }
        private readonly ILogger<JobDetailTestModule> _logger;
        
        public JobDetailTestModule(ISettingsProvider<JobDetailTestSettings> settingsProvider, ILogger<JobDetailTestModule> logger)
        {
            SettingsProvider = settingsProvider;
            _logger = logger;
        }

        public Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            _logger.LogInformation("Running {ModuleType} module with settings of type {SettingsType}", GetType(),
                SettingsProvider.GetType().GenericTypeArguments.FirstOrDefault());
            return Task.CompletedTask;
        }
    }
}