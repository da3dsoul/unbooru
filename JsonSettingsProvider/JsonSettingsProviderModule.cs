using System;
using System.Threading;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;

namespace ImageInfrastructure.JsonSettingsProvider
{
    public class JsonSettingsProviderModule : IModule
    {
        private readonly ILogger<JsonSettingsProviderModule> _logger;
        
        public JsonSettingsProviderModule(ILogger<JsonSettingsProviderModule> logger)
        {
            _logger = logger;
        }

        public Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            _logger.LogInformation("Running {ModuleType} module", GetType());
            return Task.CompletedTask;
        }
    }
}