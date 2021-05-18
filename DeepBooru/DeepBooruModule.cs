using System;
using System.Threading;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;

namespace ImageInfrastructure.DeepBooru
{
    public class DeepBooruModule : IModule
    {
        private DeepBooruSettings Settings { get; set; }
        private readonly ILogger<DeepBooruModule> _logger;
        
        public DeepBooruModule(DeepBooruSettings settings, ILogger<DeepBooruModule> logger)
        {
            Settings = settings;
            _logger = logger;
        }

        public Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            var settingsType = Settings.GetType();
            _logger.LogInformation("Running {ModuleType} module with settings of type {SettingsType}", typeof(DeepBooruModule), settingsType);
            return Task.CompletedTask;
        }
    }
}