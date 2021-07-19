using System;
using System.Threading;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ImageInfrastructure.Web
{
    public class WebModule : IModule
    {
        public Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
