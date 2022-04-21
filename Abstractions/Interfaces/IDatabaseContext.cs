using System;
using System.Linq;
using System.Linq.Expressions;

namespace unbooru.Abstractions.Interfaces
{
    public interface IDatabaseContext
    {
        IQueryable<T> Set<T>(params Expression<Func<T, object>>[] includes) where T : class;
        void Save();
    }
}
