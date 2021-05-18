using System;
using System.Threading;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;

namespace ImageInfrastructure.Reimport
{
    public class ReimportModule : IModule
    {
        private ReimportSettings Settings { get; set; }
        private readonly ILogger<ReimportModule> _logger;
        
        public ReimportModule(ReimportSettings settings, ILogger<ReimportModule> logger)
        {
            Settings = settings;
            _logger = logger;
        }

        public Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            _logger.LogInformation("Running {ModuleType} module with settings of type {SettingsType}", this.GetType(), Settings.GetType());
            return Task.CompletedTask;
        }
    }
}