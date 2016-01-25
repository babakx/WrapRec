using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Models;
using WrapRec.Evaluation;
using WrapRec.Data;

namespace WrapRec.Core
{
	public class ExperimentResult
	{
		public Model Model { get; set; }
		public ISplit Split { get; set; }
		public EvaluationContext EvaluationContext { get; set; }
		public int TrainTime { get; set; }
		public int EvaluationTime { get; set; }
		public int PureTrainTime { get; set; }
		public int PureEvaluationTime { get; set; }
		public int RepeatNum { get; set; }
	}
}
