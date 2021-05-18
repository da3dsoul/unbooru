using System;
using System.Threading;
using System.Threading.Tasks;
using ImageInfrastructure.Abstractions.Interfaces;

namespace ImageInfrastructure.Booru
{
    public class BooruModule : IModule, IServiceModule
    {
        public Task RunAsync(IServiceProvider provider, CancellationToken token)
        {
            try
            {
               return Task.CompletedTask;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}