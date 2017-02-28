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
		protected Dictionary<string, string> Header { get; private set; }
		protected Dictionary<string, int> FieldIndices { get; private set; }

	    private Dictionary<string, int> _userAuxCount = new Dictionary<string, int>();

		public override void Setup()
		{
			bool hasHeader = (SetupParameters.ContainsKey("hasHeader") && SetupParameters["hasHeader"] == "false") ? false : true;
			string delimiter = SetupParameters.ContainsKey("delimiter") ? SetupParameters["delimiter"].Replace("\\t", "\t") : ",";
			CsvConfig = new CsvConfiguration()
			{
				Delimiter = delimiter,
				HasHeaderRecord = hasHeader
			};

			if (SetupParameters.ContainsKey("header"))
			{
				Header = SetupParameters["header"].Split(',').Select(h =>
					{
						var parts = h.Split(':');

						if (parts.Length == 1)
							return new { Header = parts[0], Type = "d" };

						return new { Header = parts[0], Type = parts[1] };

					}).ToDictionary(h => h.Header, h => h.Type);

				int i = 0;
				FieldIndices = SetupParameters["header"].Split(',').ToDictionary(h => h.Split(':')[0], h => i++);
			}
		}

        public override void LoadData(DataContainer container)
        {
			Logger.Current.Info("Loading data '{0}' into container...", Id);
			Reader = new CsvHelper.CsvReader(File.OpenText(Path), CsvConfig);

			Reader.Read();
			if (Header == null && Reader.FieldHeaders != null)
			{
				Header = Reader.FieldHeaders.ToDictionary(h => h, h => "d");
				int i = 0;
				FieldIndices = Reader.FieldHeaders.ToDictionary(h => h, h => i++);
			}

			switch (DataType)
			{
				case DataType.Ratings:
					LoadRatings(container);
					break;
				case DataType.PosFeedback:
				// TODO its now working fine, but make sure you change the type of negative feedback to negativex
				case DataType.NegFeedback:
					LoadPosFeedback(container);
					break;
				case DataType.UserAttributes:
					LoadUserContext(container);
					break;
				case DataType.ItemAttributes:
					LoadItemContext(container);
					break;
                case DataType.CrossDomainUserData:
                    LoadCrossDomainUserData(container);
                    break;
                case DataType.Other:
				default:
					throw new WrapRecException(
						string.Format("dataType is not specified or not supported by CsvReader", Id));
			}
        }

		protected IEnumerable<Core.Attribute> GetAttributes(KeyValuePair<string, string> header)
		{
			string fieldValue = Reader[FieldIndices[header.Key]];

            if (header.Value == "md")
				foreach (string attr in fieldValue.Split('|'))
					yield return new Core.Attribute() { Name = header.Key, Value = attr, Type = AttributeType.Discrete };
			else if (header.Value == "r")
				yield return new Core.Attribute() { Name = header.Key, Value = fieldValue, Type = AttributeType.RealValued };
            else if (header.Value == "b")
				yield return new Core.Attribute() { Name = header.Key, Value = fieldValue, Type = AttributeType.Binary };
			else if (header.Value == "n")
				yield break;
			else // by default attributes are considered as discrete (d)
				yield return new Core.Attribute() { Name = header.Key, Value = fieldValue, Type = AttributeType.Discrete };
		}

		protected virtual void EnrichFeedback(Feedback feedback)
		{ 
			// empty body
		}
		
		protected virtual void LoadPosFeedback(DataContainer container)
		{
			// Assuming the format: userId,itemId[,attributes]
			do
			{
				Feedback feedback = container.AddFeedback(Reader.GetField(0), Reader.GetField(1));
				feedback.SliceType = SliceType;

				// load additional attributes (if there are any)
				if (Header != null)
					foreach (var h in Header.Skip(2))
						foreach (var attr in GetAttributes(h))
                            if (!feedback.Attributes.ContainsKey(attr.Name))
                                feedback.Attributes.Add(attr.Name, attr);
				
				EnrichFeedback(feedback);
			}
			while (Reader.Read());
		}

		protected virtual void LoadRatings(DataContainer container)
		{
			// Assuming the format: userId,itemId,rating[,attributes]
			do
			{
				Rating rating = container.AddRating(Reader.GetField(0), Reader.GetField(1), float.Parse(Reader.GetField(2)));
				rating.SliceType = SliceType;

				// load additional attributes (if there are any)
				if (Header != null)
					foreach (var h in Header.Skip(3))
						foreach (var attr in GetAttributes(h))
							rating.Attributes.Add(attr.Name, attr);

				EnrichFeedback(rating);
			}
			while (Reader.Read());

		}

	    public virtual void LoadUserContext(DataContainer container)
	    {
            if (Header == null)
                throw new WrapRecException(string.Format("Expect field headers or attribute 'header' in reader '{0}'.", Id));

            // Assuming format userId,attr1,attr2,...
            do
            {
                var user = container.AddUser(Reader.GetField(0));

                foreach (var h in Header.Skip(1))
                    foreach (var attr in GetAttributes(h))
                        user.Attributes.Add(attr.Name, attr);

            }
            while (Reader.Read());
        }

        protected virtual void LoadCrossDomainUserData(DataContainer container)
        {
            if (Header == null)
                throw new WrapRecException(string.Format("Expect field headers or attribute 'header' in reader '{0}'.", Id));

            int maxNumFeatures = SetupParameters.ContainsKey("maxAuxFeatures")
                ? int.Parse(SetupParameters["maxAuxFeatures"])
                : 5;
            
            // Assuming format userId,xdItem[,rating]
            do
            {
                var user = container.AddUser(Reader.GetField(0));

                if (!_userAuxCount.ContainsKey(user.Id))
                    _userAuxCount.Add(user.Id, 1);
                else
                    _userAuxCount[user.Id]++;

                //if (user.Attributes.Count < maxNumFeatures)
                if (_userAuxCount[user.Id] <= maxNumFeatures)
                {
                    var header = Header.Skip(1).Take(1).Single();
                    string fieldValue = Reader[FieldIndices[header.Key]] + Id;
                    string nextFieldValue = Reader[FieldIndices[header.Key] + 1];

                    user.Attributes.Add(fieldValue,
                        new Core.Attribute()
                        {
                            Name = fieldValue,
                            Value = nextFieldValue,
                            Type = AttributeType.RealValued
                        });
                }
            }
            while (Reader.Read());
        }

        protected virtual void LoadItemContext(DataContainer container)
		{
			if (Header == null)
				throw new WrapRecException(string.Format("Expect field headers or attribute 'header' in reader '{0}'.", Id));

			// Assuming format itemId,attr1,attr2,...
			do
			{
				var item = container.AddItem(Reader.GetField(0));

				foreach (var h in Header.Skip(1))
					foreach (var attr in GetAttributes(h))
						item.Attributes.Add(attr.Name, attr);
			}
			while (Reader.Read());
		}
    }
}
