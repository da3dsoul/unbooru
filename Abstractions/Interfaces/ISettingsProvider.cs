using System;

namespace unbooru.Abstractions.Interfaces
{
    public interface ISettingsProvider<T> where T : new()
    {
        TResult Get<TResult>(Func<T, TResult> func);
        void Update(Action<T> func);
    }
}