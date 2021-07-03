using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ImageInfrastructure.Abstractions.Interfaces
{
    public interface IContext<T> : IAsyncDisposable where T : class
    {
        public bool DisableLogging { get; set; }
        Task<T> Get(T item, bool includeDepth = false, CancellationToken token = default);
        Task<List<T>> Get(IReadOnlyList<T> items, bool includeDepth = false, CancellationToken token = default);
        Task<List<T>> FindAll(T item, bool includeDepth = false, CancellationToken token = default);
    }
}
