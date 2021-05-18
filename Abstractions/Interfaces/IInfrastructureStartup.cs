using System;
using Microsoft.Extensions.DependencyInjection;

namespace ImageInfrastructure.Abstractions.Interfaces
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
    }
}