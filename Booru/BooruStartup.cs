using System;
using unbooru.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace unbooru.Booru
{
    public class BooruStartup : IInfrastructureStartup
    {
        public string Id => "Booru";
        public string Description => "lorem ipsum";
        public Version Version => new Version("0.0.0.1");

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IModule, BooruModule>();
        }
    }
}