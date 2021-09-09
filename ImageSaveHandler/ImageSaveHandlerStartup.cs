using System;
using unbooru.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace unbooru.ImageSaveHandler
{
    public class ImageSaveHandlerStartup : IInfrastructureStartup
    {
        public string Id => "ImageSaveHandler";
        public string Description => "Saves Images as files";
        public Version Version => new Version("0.0.0.1");

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IModule, ImageSaveHandlerModule>();
        }
    }
}