using MyMediaLite.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec
{
    public class UserRankedList : RankedList<Item>
    {
        public User User { get; set; }
        public IList<Item> CorrectItems { get; set; }

        public UserRankedList()
            : base()
        { }

        public IList<int> GetMappedItemIds(Mapping itemMapper)
        {
            return Items.Keys.Select(i => i.GetMappedId(itemMapper)).ToList();
        }

        public IList<int> GetMappedCorrectItemIds(Mapping itemMapper)
        {
            if (CorrectItems == null)
                throw new Exception("Correct items should be specified.");

            return CorrectItems.Select(i => i.GetMappedId(itemMapper)).ToList();
        }
    }
}
