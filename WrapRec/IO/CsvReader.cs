using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Core;
using WrapRec.Data;

namespace WrapRec.IO
{

	public class CsvReader : DatasetReader
    {
        protected CsvHelper.CsvReader Reader { get; private set; }
        protected CsvConfiguration CsvConfig { get; private set; }
		
		public override void Setup()
		{
			bool hasHeader = (SetupParameters.ContainsKey("hasHeader") && SetupParameters["hasHeader"] == "false") ? false : true;
			string delimiter = SetupParameters.ContainsKey("delimiter") ? SetupParameters["delimiter"].Replace("\\t", "\t") : ",";
			CsvConfig = new CsvHelper.Configuration.CsvConfiguration()
			{
				Delimiter = delimiter,
				HasHeaderRecord = hasHeader
			};
		}

        public override void LoadData(DataContainer container)
        {
			Logger.Current.Trace("Loading data '{0}' into container...", Id);
			Reader = new CsvHelper.CsvReader(File.OpenText(Path), CsvConfig);

			switch (DataType)
			{
				case DataType.Ratings:
					LoadRatings(container);
					break;
				case DataType.PosFeedback:
					LoadPosFeedback(container);
					break;
				case DataType.UserContext:
					LoadUserContext(container);
					break;
				case DataType.ItemContext:
					LoadItemContext(container);
					break;
				case DataType.Other:
				default:
					throw new WrapRecException(
						string.Format("DataType (attribute 'contains' of data element '{0}) is not specified or not supported by CsvReader", Id));
			}
        }

		private void LoadPosFeedback(DataContainer container)
		{
			// Assuming the format: userId,itemId[,attributes]
			while (Reader.Read())
			{
				Feedback feedback = container.AddFeedback(Reader.GetField(0), Reader.GetField(1));
				feedback.SliceType = SliceType;

				// load additional attributes (if there are any)
				if (Reader.FieldHeaders != null)
					foreach (string attrName in Reader.FieldHeaders.Skip(2))
						feedback.AddAttribute(attrName, Reader.GetField(attrName));
			}
		}

		private void LoadRatings(DataContainer container)
		{
			// Assuming the format: userId,itemId[,rating][,attributes]
			while (Reader.Read())
			{
				Rating rating = container.AddRating(Reader.GetField(0), Reader.GetField(1), float.Parse(Reader.GetField(2)));
				rating.SliceType = SliceType;

				// load additional attributes (if there are any)
				if (Reader.FieldHeaders != null)
					foreach (string attrName in Reader.FieldHeaders.Skip(3))
						rating.AddAttribute(attrName, Reader.GetField(attrName));
			}
		}

		private void LoadUserContext(DataContainer container)
		{
			if (Reader.FieldHeaders == null)
				throw new WrapRecException(string.Format("Expect field headers for loading UserContext in reader '{0}'.", Id));

			// Assuming format userId,attr1,attr2,...
			while (Reader.Read())
			{
				var user = container.AddUser(Reader.GetField(0));

				// load additional attributes (if there are any)
				foreach (string attrName in Reader.FieldHeaders.Skip(1))
					user.AddAttribute(attrName, Reader.GetField(attrName));
			}
		}

		private void LoadItemContext(DataContainer container)
		{
			if (Reader.FieldHeaders == null)
				throw new WrapRecException(string.Format("Expect field headers for loading ItemContext in reader '{0}'.", Id));

			// Assuming format itemId,attr1,attr2,...
			while (Reader.Read())
			{
				var item = container.AddItem(Reader.GetField(0));

				foreach (string attrName in Reader.FieldHeaders.Skip(1))
					item.AddAttribute(attrName, Reader.GetField(attrName));
			}
		}
    }
}
