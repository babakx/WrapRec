using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Core;
using WrapRec.Data;

namespace WrapRec.IO
{
	public enum DataType
	{
		Ratings,
		PosFeedback,
		NegFeedback,
		UserAttributes,
		ItemAttributes,
        CrossDomainUserData,
		Other
	}

	public abstract class DatasetReader 
	{
		public string Id { get; set; }
		public string Path { get; set; }
		public FeedbackSlice SliceType { get; set; }
		public DataType DataType { get; set; }
		public Dictionary<string, string> SetupParameters { get; set; }
		public abstract void Setup();
		public abstract void LoadData(DataContainer container);
	}
}
