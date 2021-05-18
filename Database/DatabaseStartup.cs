using System;
using ImageInfrastructure.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ImageInfrastructure.Database
{
    public class DatabaseStartup : IInfrastructureStartup
    {
        public string Id => "Database";
        public string Description => "Saves images to a database";
        public Version Version => new Version("0.0.0.1");

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IModule, DatabaseModule>();
        }
    }
}