using System;
using unbooru.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddScoped(typeof(ISettingsProvider<>), typeof(JsonSettingsProvider<>));
        }
    }
}