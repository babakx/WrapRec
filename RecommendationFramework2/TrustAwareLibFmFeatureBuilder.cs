using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrapRec.Data;

namespace WrapRec
{
    public class TrustAwareLibFmFeatureBuilder : LibFmFeatureBuilder
    {

        private Dictionary<string, int> _connectionsFreq;

        public int MaxConnections { get; set; }

        public bool UseConnectionStrength { get; set; }

        public TrustAwareLibFmFeatureBuilder(DataContainer container, int maxConnections, bool useConnectionStrength)
        {
            MaxConnections = maxConnections;
            UseConnectionStrength = useConnectionStrength;

            if (useConnectionStrength)
                return;

            _connectionsFreq = new Dictionary<string, int>();

            foreach (var u in container.Users.Values)
            {
                string conns = u.GetProperty("Connections");

                if (!string.IsNullOrEmpty(conns))
                {
                    foreach (string connId in conns.Split(','))
                    {
                        AddConnectionCount(connId);
                    }
                }
            }
        }

        private void AddConnectionCount(string connId)
        {
            if (_connectionsFreq.ContainsKey(connId))
                _connectionsFreq[connId] += 1;
            else
                _connectionsFreq.Add(connId, 1);
        }
        
        public override string GetLibFmFeatureVector(ItemRating rating)
        {
            string baseVector = base.GetLibFmFeatureVector(rating);

            string conns = rating.User.GetProperty("Connections");

            if (string.IsNullOrEmpty(conns))
                return baseVector;

            string extension = UseConnectionStrength ? GetImplicitConnectionsVector(conns) : GetExplicitConnectionsVector(conns);

            return baseVector + " " + extension;
        }

        private string GetImplicitConnectionsVector(string conns)
        { 
            return conns.Split(',')
                .Select(c => 
                {
                    var parts = c.Split(' ');
                    return new {ConnId = parts[0], Stength = float.Parse(parts[1])};
                })
                .OrderByDescending(cs => cs.Stength)
                .Take(MaxConnections)
                .Select(cs => string.Format("{0}:{1}", Mapper.ToInternalID(cs.ConnId), cs.Stength * 0.5))
                .Aggregate((a, b) => a + " " + b);
        }

        private string GetExplicitConnectionsVector(string conns)
        {
            return conns.Split(',')
                .Select(c => new { ConnId = c, Count = _connectionsFreq[c] })
                .OrderByDescending(cc => cc.Count)
                .Take(MaxConnections)
                .Select(c => string.Format("{0}:0.1", Mapper.ToInternalID(c.ConnId)))
                .Aggregate((a, b) => a + " " + b);
        }
    }
}
