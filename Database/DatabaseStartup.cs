using System;
using unbooru.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using unbooru.Abstractions.Poco.Events;

namespace unbooru.Database
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

        public void Main(StartupEventArgs args)
        {
            
        }
    }
}