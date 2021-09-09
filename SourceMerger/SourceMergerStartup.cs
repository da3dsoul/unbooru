using System;
using unbooru.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace unbooru.SourceMerger
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