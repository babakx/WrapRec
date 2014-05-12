using MyMediaLite.Data;
using MyMediaLite.DataType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF2
{
    public static class IEnumerableExtensions 
    {
        public static PosOnlyFeedback<SparseBooleanMatrix> ToPosOnlyFeedback(this IEnumerable<ItemRanking> source, Mapping usersMap, Mapping itemsMap)
        {
            var feedback = new PosOnlyFeedback<SparseBooleanMatrix>();

            // Convert items to MyMediaLite PositiveOnly format
            foreach (var itemRanking in source)
            {
                feedback.Add(usersMap.ToInternalID(itemRanking.User.Id), itemsMap.ToInternalID(itemRanking.Item.Id));
            }

            return feedback;
        }

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
    }
}
