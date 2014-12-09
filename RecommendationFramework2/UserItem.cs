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
        public bool IsTest { get; set; }

        public bool IsActive { get; set; }

        public UserItem() : base()
        {
            IsActive = false;
        }

        public UserItem(User user, Item item)
            : this()
        {
            User = user;
            Item = item;
        }

        public UserItem(string userId, string itemId)
            : this()
        {
            User = new User(userId);
            Item = new Item(itemId);
        }
    }
}
