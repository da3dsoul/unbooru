using System;
using CommandLine;
using unbooru.Abstractions.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Web;
using Quartz;
using Quartz.Impl.Matchers;
using unbooru.Abstractions.Poco.Events;

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

        public void Main(StartupEventArgs args)
        {
            var parser = new Parser(o => o.IgnoreUnknownArguments = true);
            parser.ParseArguments<CLIModel>(args.Args)
                .WithParsed(o =>
                {
                    if (o.WebOnly)
                    {
                        var scheduler = args.Services.GetService<ISchedulerFactory>()?.GetScheduler().Result;
                        if (scheduler == null) return;
                        var triggers = scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup()).Result;
                        scheduler.UnscheduleJobs(triggers);
                        if (scheduler.IsStarted) scheduler.Shutdown();
                        else scheduler.PauseAll();
                    }
                });
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
