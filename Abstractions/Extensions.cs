using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImageInfrastructure.Abstractions.Poco;

namespace ImageInfrastructure.Abstractions
{
    public static class Extensions
    {
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

        public static string GetPixivFilename(this Image image) =>
            Path.GetFileName(image?.Sources?.FirstOrDefault(a => a.Source == "Pixiv")?.Uri);
    }
}