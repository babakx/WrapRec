using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;
using WrapRec.Models;
using WrapRec.Data;
using WrapRec.Evaluation;

namespace WrapRec.Core
{
    public class ExperimentManager
    {
        public XElement ConfigRoot { get; private set; }

        public ExperimentManager(string configFile)
        {
            ConfigRoot = XDocument.Load(configFile).Root;

            try
            {
                XElement allExpEl = ConfigRoot.Descendants("experiments").Single();
                XAttribute runAttr = allExpEl.Attribute("run");

                IEnumerable<XElement> expEls = allExpEl.Descendants("experiment");
                if (runAttr != null)
                {
                    var expIds = runAttr.Value.Split(',');
                    expEls = expEls.Where(el => expIds.Contains(el.Attribute("id").Value));
                }



            }
            catch (Exception  ex)
            {
                Logger.Current.Error(ex.Message);
            }

        }

        private Experiment ParseExperiment(XElement expEl)
        {
            XAttribute expClass = expEl.Attribute("class");
            Experiment exp;

            if (expClass != null)
            {
                var expType = Type.GetType(expClass.Value);
                if (expType == null)
                    throw new WrapRecException(string.Format("Can not resolve Experiment type: '{0}'", expClass.Value));

                if(!typeof(Experiment).IsAssignableFrom(expType))
                    throw new WrapRecException(string.Format("Experiment type '{0}' should inherit class 'WrapRec.Core.Experiment'", expClass.Value));

                exp = (Experiment) expType.GetConstructor(Type.EmptyTypes).Invoke(null);
            }
            else
                exp = new Experiment();

            exp.Id = expEl.Attribute("id").Value;

            expEl.Attribute("models").Value.Split(',').ToList()
				.ForEach(mId => exp.Models.AddRange(ParseModelsSet(mId)));

            expEl.Attribute("splits").Value.Split(',').ToList()
                .ForEach(sId => exp.Splits.Add(ParseSplit(sId)));

            exp.EvaluationContext = ParseEvaluationContext(expEl.Attribute("evalContext").Value);

            exp.Repeat = expEl.Attribute("repeat") != null ? int.Parse(expEl.Attribute("repeat").Value) : 1;

            return exp;
        }

        private IEnumerable<Model> ParseModelsSet(string modelId)
        {
            return null;
        }

        private ISplit ParseSplit(string splitId)
        {
            return null;
        }

        private EvaluationContext ParseEvaluationContext(string evalId)
        {
            return null;
        }


    }
}
