using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace unbooru.Abstractions.Interfaces
{
    public interface IDatabaseContext
    {
        IQueryable<T> Set<T>(IEnumerable<Expression<Func<T, IEnumerable>>> includes = null) where T : class;
    }
}
