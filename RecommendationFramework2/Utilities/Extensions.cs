using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Utilities
{
    public static class Extensions
    {
        public static TValue GetValueOrDefault<TKey, TValue> (this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static double ToUnixEpoch(this DateTime datetime)
        {
            return (datetime - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime()).TotalSeconds;
        }

        public static string GetDirectoryPath(this string path)
        {
            return path.Substring(0, path.LastIndexOf('\\'));
        }

        public static string GetFileName(this string path)
        {
            return path.Substring(path.LastIndexOf('\\') + 1, path.LastIndexOf('.') - path.LastIndexOf('\\') - 1);
        }

        public static string GetFileExtension(this string path)
        {
            return path.Substring(path.LastIndexOf('.') + 1);
        }
    }
}
