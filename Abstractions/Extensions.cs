using System;
using System.Collections.Generic;
using System.Linq;
using unbooru.Abstractions.Poco;
using JetBrains.Annotations;

namespace unbooru.Abstractions
{
    public static class Extensions
    {
        [UsedImplicitly]
        public static bool TryFirst<T>(this IEnumerable<T> seq, Func<T,bool> filter, out T result) 
        {
            result=default;
            foreach(var item in seq)
            {
                if (!filter(item)) continue;
                result = item;
                return true;
            }
            return false;
        }

        public static string GetPixivFilename(this Image image) => image?.Sources?.FirstOrDefault(a => a.Source == "Pixiv")?.OriginalFilename;
    }
}