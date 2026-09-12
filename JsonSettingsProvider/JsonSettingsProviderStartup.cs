using System;
using unbooru.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using unbooru.Abstractions.Poco.Events;

namespace unbooru.JsonSettingsProvider
{
    public class JsonSettingsProviderStartup : IInfrastructureStartup
    {
        public string Id => "JsonSettingsProvider";
        public string Description => "lorem ipsum";
        public Version Version => new Version("0.0.0.1");

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IModule, JsonSettingsProviderModule>();
            // Must be a singleton: each job/request used to get its own scoped instance that
            // re-read the settings file independently, so concurrent jobs (e.g. the Pixiv
            // import/service jobs) would each save a stale in-memory snapshot and clobber
            // each other's writes (including freshly-rotated auth tokens).
            services.AddSingleton(typeof(ISettingsProvider<>), typeof(JsonSettingsProvider<>));
        }

        public void Main(StartupEventArgs args)
        {
            
        }
    }
}