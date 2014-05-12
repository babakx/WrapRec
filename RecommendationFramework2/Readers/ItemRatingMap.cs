using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace RF2.Readers
{
    public class ItemRatingMap : CsvClassMap<ItemRating>
    {
        public override void CreateMap()
        {
            Map(ir => ir.Item).Name("ItemId").TypeConverter<ItemConvertor>();
            Map(ir => ir.User).Name("UserId").TypeConverter<UserConvertor>();
            Map(ir => ir.Rating).Name("Rating");
        }
    }

    public class UserConvertor : ITypeConverter
    {

        public bool CanConvertFrom(Type type)
        {
            return true;
        }

        public bool CanConvertTo(Type type)
        {
            return true;
        }

        public object ConvertFromString(TypeConverterOptions options, string text)
        {
            return new User(text);
        }

        public string ConvertToString(TypeConverterOptions options, object value)
        {
            return ((User)value).Id;
        }
    }

    public class ItemConvertor : ITypeConverter
    {

        public bool CanConvertFrom(Type type)
        {
            return true;
        }

        public bool CanConvertTo(Type type)
        {
            return true;
        }

        public object ConvertFromString(TypeConverterOptions options, string text)
        {
            return new Item(text);
        }

        public string ConvertToString(TypeConverterOptions options, object value)
        {
            return ((Item)value).Id;
        }
    }

}
