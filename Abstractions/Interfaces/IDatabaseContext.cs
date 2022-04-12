using System.Linq;

namespace unbooru.Abstractions.Interfaces;

public interface IDatabaseContext
{
    IQueryable<T> Set<T>() where T : class;
}