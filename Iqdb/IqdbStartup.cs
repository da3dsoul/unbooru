using System;
using ImageInfrastructure.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ImageInfrastructure.Iqdb
{
    public class IqdbStartup : IInfrastructureStartup
    {
        public string Id => "Iqdb";
        public string Description => "lorem ipsum";
        public Version Version => new Version("0.0.0.1");

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IModule, IqdbModule>();
        }
    }
}