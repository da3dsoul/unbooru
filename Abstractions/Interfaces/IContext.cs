using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ImageInfrastructure.Abstractions.Interfaces
{
    public interface IContext<T> where T : class
    {
        Task<T> Get(T item, bool includeDepth = false);
        Task<List<T>> FindAll(T item, bool includeDepth = false);
        void Remove(T item);

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken());
    }
}