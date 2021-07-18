using System;
using System.Threading;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ImageInfrastructure.Web
{
    public class WebModule : IModule
    {
        public async Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            var db = provider.GetRequiredService<DatabaseHelper>();
            await db.FixSizes(provider);
        }
    }
}