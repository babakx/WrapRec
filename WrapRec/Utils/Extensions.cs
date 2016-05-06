using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Utils
{
    public static class Extensions
    {
        /// <summary>
        /// Convert a comma separated IEnumerable of lines to field value dictionary
        /// </summary>
        /// <param name="source">IEnumerable of csv including header</param>
        /// <returns></returns>
        public static IEnumerable<Dictionary<string, string>> ToCsvDictionary(this IEnumerable<string> source)
        {
            return ToCsvDictionary(source, ',');
        }

        /// <summary>
        /// Convert a [deliminator] separated IEnumerable of lines to field value dictionary
        /// </summary>
        /// <param name="source">IEnumerable of csv including header</param>
        /// <param name="deliminator">Deliminator character</param>
        /// <returns></returns>
        public static IEnumerable<Dictionary<string, string>> ToCsvDictionary(this IEnumerable<string> source, char deliminator)
        {
            var fields = source.Take(1).FirstOrDefault().Split(deliminator);
            return source.Skip(1)
                .Select(line => line.Split(deliminator)
                    .Zip(fields, (value, field) => new KeyValuePair<string, string>(field, value))
                    .ToDictionary(i => i.Key, i => i.Value));
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }

		public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
		{
			IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
			return sequences.Aggregate(
				emptyProduct,
				(accumulator, sequence) =>
					from accseq in accumulator
					from item in sequence
					select accseq.Concat(new[] { item })
				);
		}

        public static double ToUnixEpoch(this DateTime datetime)
        {
            return (datetime - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime()).TotalSeconds;
        }
        public static DateTime FromUnixEpoch(this long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }

        public static string GetDirectoryPath(this string path)
        {
            return path.Substring(0, path.LastIndexOf('\\'));
        }

        public static string GetFileName(this string path)
        {
            return path.Substring(path.LastIndexOf('\\') + 1, path.LastIndexOf('.') - path.LastIndexOf('\\') - 1);
        }
        public static string GetPathWithoutExtension(this string path)
        {
            return path.Substring(0, path.LastIndexOf('.'));
        }
        public static string GetFileExtension(this string path)
        {
            return path.Substring(path.LastIndexOf('.') + 1);
        }

		// The following four extensions are added to support lazyLoad taking and skipping
		// http://stackoverflow.com/questions/35209540/c-sharp-is-it-possible-to-lazy-load-function-parameter-after-calling-the-functio
		public static IEnumerable<T> Take<T>(this IEnumerable<T> source, Lazy<int> count)
		{
			var takeSequence = source.Take(count.Value);
			foreach (var item in takeSequence) 
				yield return item;
		}
		
		public static IEnumerable<T> Take<T>(this IEnumerable<T> source, Func<int> getCount)
		{
			var takeSequence = source.Take(getCount());
			foreach (var item in takeSequence)
				yield return item;
		}

		public static IEnumerable<T> Skip<T>(this IEnumerable<T> source, Lazy<int> count)
		{
			var skipSeq = source.Skip(count.Value);
			foreach (var item in skipSeq)
				yield return item;
		}

		public static IEnumerable<T> Skip<T>(this IEnumerable<T> source, Func<int> getCount)
		{
			var skipSeq = source.Skip(getCount());
			foreach (var item in skipSeq)
				yield return item;
		}

		public static IEnumerable<Dictionary<string, string>> JoinDictionaries(this IEnumerable<Dictionary<string, string>> source)
		{
			var joinedFields = source.SelectMany(dic => dic.Keys).Distinct().ToList();

			foreach (Dictionary<string, string> sourceDic in source)
			{
				var joinedDic = new Dictionary<string, string>();

				for (int i = 0; i < joinedFields.Count; i++)
				{
					if (sourceDic.ContainsKey(joinedFields[i]))
						joinedDic.Add(joinedFields[i], sourceDic[joinedFields[i]]);
					else
						joinedDic.Add(joinedFields[i], "NA");
				}

				yield return joinedDic;
			}
		}

	}
}
