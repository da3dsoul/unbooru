using System;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using unbooru.Abstractions.Interfaces;
using unbooru.Abstractions.Poco.Events;

namespace unbooru.JobDetailTest
{
    public class JobDetailTestStartup : IInfrastructureStartup
    {
        public string Id => "JobDetailTest";
        public string Description => "lorem ipsum";
        public Version Version => new Version("0.0.0.1");

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IModule, JobDetailTestModule>();
            services.AddTransient<TestJob>();
            services.AddOptions<QuartzOptions>().Configure(o =>
            {
                o.AddJob<TestJob>(builder =>
                {

                });
            });
        }

        public void Main(StartupEventArgs args)
        {
            
        }
    }
}