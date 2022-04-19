using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using unbooru.Abstractions.Interfaces;

namespace unbooru.Template
{
    public class TemplateModule : IModule
    {
        private ISettingsProvider<TemplateSettings> SettingsProvider { get; set; }
        private readonly ILogger<TemplateModule> _logger;
        
        public TemplateModule(ISettingsProvider<TemplateSettings> settingsProvider, ILogger<TemplateModule> logger)
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