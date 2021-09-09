using Microsoft.Extensions.Hosting;

namespace unbooru.Abstractions.Interfaces
{
    public interface IHostProvider
    {
        IHostBuilder Build(IHostBuilder hostBuilder);
    }
}