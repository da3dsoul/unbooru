using System;
using ImageInfrastructure.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ImageInfrastructure.SourceMerger
{
    public class SourceMergerStartup : IInfrastructureStartup
    {
        public string Id => "SourceMerger";
        public string Description => "lorem ipsum";
        public Version Version => new Version("0.0.0.1");

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IModule, SourceMergerModule>();
        }
    }
}