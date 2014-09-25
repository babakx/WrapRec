using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace WrapRec.Readers
{
    public class ItemRankingMap : CsvClassMap<ItemRanking>
    {
        public override void CreateMap()
        {
            Map(ir => ir.Item).Name("ItemId").TypeConverter<ItemConvertor>();
            Map(ir => ir.User).Name("UserId").TypeConverter<UserConvertor>();
        }
    }
}
