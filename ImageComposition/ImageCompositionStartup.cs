using System;
using Microsoft.Extensions.DependencyInjection;
using unbooru.Abstractions.Interfaces;
using unbooru.Abstractions.Poco.Events;

namespace unbooru.ImageComposition
{
    public class ImageCompositionStartup : IInfrastructureStartup
    {
        public string Id => "ImageComposition";
        public string Description => "lorem ipsum";
        public Version Version => new Version("0.0.0.1");

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IModule, ImageCompositionModule>();
        }

        public void Main(StartupEventArgs args)
        {
            
        }
    }
}