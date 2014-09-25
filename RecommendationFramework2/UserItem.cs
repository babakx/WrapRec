using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrapRec
{
    public class UserItem : EntityRelation
    {
        public User User { get; set; }
        public Item Item { get; set; }

        public UserItem() : base()
        { }

        public UserItem(User user, Item item)
            : base()
        {
            User = user;
            Item = item;
        }

        public UserItem(string userId, string itemId)
            : base()
        {
            User = new User(userId);
            Item = new Item(itemId);
        }
    }
}
