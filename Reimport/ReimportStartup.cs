using System;
using unbooru.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace unbooru.Reimport
{
    public class ReimportStartup : IInfrastructureStartup
    {
        public string Id => "Reimport";
        public string Description => "lorem ipsum";
        public Version Version => new Version("0.0.0.1");

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IModule, ReimportModule>();
        }
    }
}