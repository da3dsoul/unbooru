using System;
using unbooru.Abstractions.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Web;

namespace unbooru.Web
{
    public class WebStartup : IInfrastructureStartup, IHostProvider
    {
        public string Id => "Web";
        public string Description => "lorem ipsum";
        public Version Version => new Version("0.0.0.1");

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IModule, WebModule>();
            services.AddScoped<DatabaseHelper>();
        }

        public IHostBuilder Build(IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureWebHostDefaults(options =>
            {
                //
                options.UseStartup<AspNetCoreStartup>().UseUrls("http://*:9280", "https://*:9281").ConfigureLogging(a => a.AddNLogWeb());
            });
        }
    }
}
