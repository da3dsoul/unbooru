using System;
using Microsoft.Extensions.DependencyInjection;
using unbooru.Abstractions.Poco.Events;

namespace unbooru.Abstractions.Interfaces
{
    /**
     * All modules that want to be loaded must have this on its main class
     */
    public interface IInfrastructureStartup
    {
        string Id { get; }
        string Description { get; }
        Version Version { get; }
        
        void ConfigureServices(IServiceCollection services);

        void Main(StartupEventArgs args);
    }
}