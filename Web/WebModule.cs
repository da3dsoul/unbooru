using System;
using System.Threading;
using System.Threading.Tasks;
using unbooru.Abstractions.Interfaces;

namespace unbooru.Web
{
    public class WebModule : IModule
    {
        public Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
