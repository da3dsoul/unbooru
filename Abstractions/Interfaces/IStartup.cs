using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace unbooru.Abstractions.Interfaces
{
    public interface IStartup
    {
        Task Main(string[] args);

        IHostBuilder CreateHostBuilder(string[] args);
    }
}