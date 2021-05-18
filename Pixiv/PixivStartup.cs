using System;
using ImageInfrastructure.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ImageInfrastructure.Pixiv
{
    public class PixivStartup : IInfrastructureStartup
    {
        public string Id => "BooruDownloader";
        public string Description => "Downloads images from boorus with via various filters";
        public Version Version => new Version("0.0.0.1");

        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddSingleton<PixivModule>();
            services.AddSingleton<IModule>(x => x.GetRequiredService<PixivModule>());
            services.AddSingleton<IServiceModule>(x => x.GetRequiredService<PixivModule>());
            services.AddSingleton<IImageProvider>(x => x.GetRequiredService<PixivModule>());
        }
    }
}