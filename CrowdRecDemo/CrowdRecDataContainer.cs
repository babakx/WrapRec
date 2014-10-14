﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WrapRec.Data;

namespace CrowdRecDemo
{
    public class CrowdRecDataContainer : DataContainer
    {

        public CrowdRecDataContainer()
            : base()
        { }

        // Todo: think of making the following two methods as one method to prevent duplication
        public Item CreateItem(string itemId, double? timestamp, string properties)
        {
            var i = new Item(itemId);

            i.Timestamp = timestamp;

            var jObject = JObject.Parse(properties);
            jObject.Properties().ToList().ForEach(p => i.AddProperty(p.Name, p.Value.ToString()));

            return i;
        }

        public User CreateUser(string userId, double? timestamp, string properties)
        {
            var u = new User(userId);

            u.Timestamp = timestamp;

            var jObject = JObject.Parse(properties);
            jObject.Properties().ToList().ForEach(p => u.AddProperty(p.Name, p.Value.ToString()));

            return u;
        }

        public ItemRating CreateItemRating(string relationId, double? timestamp, string properties, string linkedEntities)
        {
            var ir = new ItemRating();
            ir.Id = relationId;
            ir.Timestamp = timestamp;


            var jProperties = JObject.Parse(properties);

            // add general properties
            jProperties.Properties().ToList().ForEach(p => ir.AddProperty(p.Name, p.Value.ToString()));


            // try to extract rating from properties
            try
            {
                ir.Rating = float.Parse(jProperties["rating"].ToString());
            }
            catch (Exception)
            {
                throw new Exception("Can not find / parse rating in the relation.");
            }

            // parse linked entities
            var jLinkedEntities = JObject.Parse(linkedEntities);

            // user
            string user = jLinkedEntities["subject"].ToString();

            if (!user.Contains("user"))
                throw new Exception("Expect subject of type user in the linked entities.");

            string userId = user.Substring(user.IndexOf(':') + 1);

            User u;

            if (!Users.TryGetValue(userId, out u))
            {
                Console.WriteLine(string.Format("User with id {0} is not defined in the entities file.", userId));
                u = new User(userId);
            }

            u.Ratings.Add(ir);

            // item
            string item = jLinkedEntities["object"].ToString();

            if (!item.Contains("movie") && !item.Contains("item"))
                throw new Exception("Expect object of type movie or item in the linked entities.");

            string itemId = item.Substring(item.IndexOf(':') + 1);

            Item i;

            if (!Items.TryGetValue(itemId, out i))
            {
                Console.WriteLine(string.Format("Item with id {0} is not defined in the entities file.", itemId));
                i = new Item(itemId);
            }

            i.Ratings.Add(ir);

            ir.User = u;
            ir.Item = i;

            return ir;
        }


    }
}
