using System;
using unbooru.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using unbooru.Abstractions.Poco.Events;

namespace unbooru.ImageCompressor
{
    public class ImageCompressorStartup : IInfrastructureStartup
    {
        public string Id => "ImageCompressor";
        public string Description => "lorem ipsum";
        public Version Version => new Version("0.0.0.1");

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IModule, ImageCompressorModule>();
        }

        public void Main(StartupEventArgs args)
        {
            
        }
    }
}