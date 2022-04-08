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

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.Shuffle(new Random());
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (rng == null) throw new ArgumentNullException(nameof(rng));

            return source.ShuffleIterator(rng);
        }

        private static IEnumerable<T> ShuffleIterator<T>(
            this IEnumerable<T> source, Random rng)
        {
            var buffer = source.ToList();
            for (var i = 0; i < buffer.Count; i++)
            {
                var j = rng.Next(i, buffer.Count);
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }
    }
}