using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec.Entities
{
   public class ItemRatingWithRelations : ItemRating
    {
       ItemRating _itemRating;
       Dictionary<string, List<string>> _relations;

       public ItemRatingWithRelations(ItemRating itemRating, Dictionary<string, List<string>> relations)
           : base(itemRating.User, itemRating.Item, itemRating.Rating)
       {
           _relations = relations;
       }

       public override string ToLibFmFeatureVector(MyMediaLite.Data.Mapping usersItemsMap)
       {
           string relationsVector = "";

           if (_relations.ContainsKey(User.Id))
           {
               int relCount = _relations[User.Id].Count;
               relationsVector = _relations[User.Id].Take(5)
                   .Aggregate("", (cur, next) => string.Format("{0} {1}:{2:0.00} ", cur, usersItemsMap.ToInternalID(next), 0.1));
           }

           return string.Format("{0} {1}:1 {2}:1 {3}", 
               Rating, 
               usersItemsMap.ToInternalID(User.Id), 
               usersItemsMap.ToInternalID(Item.Id), 
               relationsVector);
       }
    }
}
