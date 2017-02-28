using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqLib.Sequence;
using MyMediaLite.Data;
using WrapRec.Core;

namespace WrapRec.Utils
{
    public class GpfmConverter : Experiment
    {
        public Mapping UserMapper { get; set; }
        public Mapping ContextMapper { get; set; }
        public List<string> ContextNames { get; set; }
        public string OutputPath { get; set; }
        public List<string> AllItems { get; private set; }
        public Dictionary<string, List<string>> UserItems { get; private set; }

        public override void Setup()
        {
            if (!Split.Container.IsLoaded)
            {
                Logger.Current.Info("Loading DataContainer '{0}'...", Split.Container.Id);
                Split.Container.Load();
            }

            OutputPath = Path.Combine(ExperimentManager.ResultsFolder, SetupParameters["outputPath"]);

            if (SetupParameters.ContainsKey("contextNames"))
                ContextNames = SetupParameters["contextNames"].Split(',').ToList();
            else
                ContextNames = new List<string>();

            UserMapper = new Mapping();
            ContextMapper = new Mapping();
            
            // burn id 0
            UserMapper.ToInternalID("burn");
            ContextMapper.ToInternalID("burn");

            UserItems = Split.Container.Users.Values.ToDictionary(u => u.Id,
                u => u.Feedbacks.Select(f => f.Item.Id).ToList());

            AllItems = Split.Container.Items.Values.Select(i => i.Id).ToList();
        }

        public override void Run()
        {
            var trainPointWriter = new StreamWriter(OutputPath + ".point");
            var trainPairWriter = new StreamWriter(OutputPath + ".pair");
            var testWriter = new StreamWriter(OutputPath + ".test");
            var testOrgWriter = new StreamWriter(OutputPath + ".test.org");

            foreach (var f in Split.Train)
            {
                var mUserId = UserMapper.ToInternalID(f.User.Id);
                var mItemId = ContextMapper.ToInternalID(f.Item.Id);
                var nNegItemId = ContextMapper.ToInternalID(SampleItemId(f.User.Id));
                //var context = GetContextVector(f);

                trainPointWriter.WriteLine($"{mUserId},{mItemId},1");
                trainPointWriter.WriteLine($"{mUserId},{nNegItemId},-1");
                trainPairWriter.WriteLine($"{mUserId},{mItemId},{nNegItemId},1");
                trainPairWriter.WriteLine($"{mUserId},{nNegItemId},{mItemId},-1");
                testWriter.WriteLine($"{mUserId},{mItemId},1");
                testOrgWriter.WriteLine($"{f.User.Id},{f.Item.Id},1");
            }

            foreach (var f in Split.Test)
            {
                var mUserId = UserMapper.ToInternalID(f.User.Id);
                var mItemId = ContextMapper.ToInternalID(f.Item.Id);
                //var context = GetContextVector(f);

                testWriter.WriteLine($"{mUserId},{mItemId},1");
                testOrgWriter.WriteLine($"{f.User.Id},{f.Item.Id},1");
            }

            var c = Split.Container;

            foreach (var user in c.Users.Values)
            {
                int count = 0;
                foreach (var item in c.Items.Values.Shuffle())
                {
                    if (!c.FeedbacksDic.ContainsKey(user.Id, item.Id))
                    {
                        var mappedUId = UserMapper.ToInternalID(user.Id);
                        var mappedNegIId = ContextMapper.ToInternalID(item.Id);

                        testWriter.WriteLine($"{mappedUId},{mappedNegIId},-1");
                        testOrgWriter.WriteLine($"{user.Id},{item.Id},-1");
                        count++;

                        if (count == 1000)
                            break;
                    }
                }
            }

            trainPointWriter.Close();
            trainPairWriter.Close();
            testWriter.Close();
            testOrgWriter.Close();
        }

        private string GetContextVector(Feedback f)
        {
            var ctx = ContextNames.Select(c => ContextMapper.ToInternalID(f.Attributes[c].Value).ToString()).ToList();

            if (ctx.Count > 0)
                return ctx.Aggregate((a, b) => a + "," + b);
            else
                return "1";
        }

        public string SampleItemId(string userId)
        {
            var rnd = new Random();
            int rndIndex = rnd.Next(AllItems.Count);

            while (UserItems[userId].Contains(AllItems[rndIndex]))
            {
                rndIndex = rnd.Next(AllItems.Count);
            }

            return AllItems[rndIndex];
        }
    }
}
