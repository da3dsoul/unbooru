using System.Threading;
using System.Threading.Tasks;

namespace ImageInfrastructure.Abstractions.Interfaces
{
    public interface IReadWriteContext<T> : IContext<T> where T : class
    {
        Task<T> Add(T item);
        void Remove(T item);

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken());

        void RollbackChanges();
    }
}