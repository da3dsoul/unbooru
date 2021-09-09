using System;
using unbooru.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace unbooru.SauceNao
{
    public class SauceNaoStartup : IInfrastructureStartup
    {
        public string Id => "SauceNao";
        public string Description => "lorem ipsum";
        public Version Version => new Version("0.0.0.1");

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IModule, SauceNaoModule>();
            services.AddSingleton<SimpleRateLimiter>();
        }
    }
}