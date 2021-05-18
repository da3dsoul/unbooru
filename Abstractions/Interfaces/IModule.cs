using System;
using System.Threading;
using System.Threading.Tasks;

namespace ImageInfrastructure.Abstractions.Interfaces
{
    public interface IModule
    {
        Task RunAsync(IServiceProvider provider, CancellationToken token);
    }
}