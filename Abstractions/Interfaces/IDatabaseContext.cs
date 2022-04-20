using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;

namespace unbooru.Abstractions.Interfaces
{
    public interface IDatabaseContext
    {
        IQueryable<T> Set<T>(params Expression<Func<T, IEnumerable>>[] includes) where T : class;
    }
}
