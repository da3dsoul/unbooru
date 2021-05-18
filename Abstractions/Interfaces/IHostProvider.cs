using Microsoft.Extensions.Hosting;

namespace ImageInfrastructure.Abstractions.Interfaces
{
    public interface IHostProvider
    {
        IHostBuilder Build(IHostBuilder hostBuilder);
    }
}