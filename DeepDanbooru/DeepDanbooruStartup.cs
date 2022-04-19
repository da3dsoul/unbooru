using System;
using Microsoft.Extensions.DependencyInjection;
using unbooru.Abstractions.Interfaces;
using unbooru.Abstractions.Poco.Events;

namespace unbooru.DeepDanbooru
{
    public class DeepDanbooruStartup : IInfrastructureStartup
    {
        public string Id => "DeepDanbooru";
        public string Description => "lorem ipsum";
        public Version Version => new Version("0.0.0.1");

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IModule, DeepDanbooruModule>();
        }

        public void Main(StartupEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}