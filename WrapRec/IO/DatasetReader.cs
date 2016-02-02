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
		UserContext,
		ItemContext,
		Other
	}

	public abstract class DatasetReader 
	{
		public string DatasetId { get; set; }
		public string Path { get; set; }
		
		// in case the reader does not read any feedback, the value of this property should be NOT_APPLICABLE
		public FeedbackSlice ReaderSliceType { get; protected set; }
		public DataType DataType { get; protected set; }
		public Dictionary<string, string> SetupParameters { get; set; }
		public abstract void Setup();
		public abstract void LoadData(DataContainer container);
	}
}
