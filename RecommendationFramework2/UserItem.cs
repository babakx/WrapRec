using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF2
{
    public class UserItem
    {
        public User User { get; set; }
        public Item Item { get; set; }

        public UserItem()
        { }

        public UserItem(User user, Item item)
        {
            User = user;
            Item = item;
        }

        public UserItem(string userId, string itemId)
        {
            User = new User(userId);
            Item = new Item(itemId);
        }
    }
}
