using System;
using ImageInfrastructure.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ImageInfrastructure.DeepBooru
{
    public class DeepBooruStartup : IInfrastructureStartup
    {
        public string Id => "DeepBooru";
        public string Description => "Tags with ML wizardry";
        public Version Version => new Version("0.0.0.1");

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IModule, DeepBooruModule>();
            services.AddSingleton<DeepBooruSettings>();
        }
    }
}