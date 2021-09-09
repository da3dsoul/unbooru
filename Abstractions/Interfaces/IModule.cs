using System;
using System.Threading;
using System.Threading.Tasks;

namespace unbooru.Abstractions.Interfaces
{
    public interface IModule
    {
        Task RunAsync(IServiceProvider provider, CancellationToken token);
    }
}