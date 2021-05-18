using System;
using ImageInfrastructure.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ImageInfrastructure.Booru
{
    public class Startup : IInfrastructureStartup
    {
        public string Id => "BooruDownloader";
        public string Description => "Downloads images from boorus with via various filters";
        public Version Version => new Version("0.0.0.1");

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IModule, BooruModule>();
            services.AddSingleton<Settings>();
        }
    }
}