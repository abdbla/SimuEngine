using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Core;

namespace Implementation {
    [DebuggerDisplay("Creation ID: {creationID}, Graph name: {graphName}")]
    [Serializable]
    class DistrictConnection : Connection {
        static int id = 0;
        string creationID = id++.ToString();
        string graphName; 
        
        public override void SetName(string name) {
            graphName = name;
        }

        public DistrictConnection() {
            Traits["Interconnectivity"] = Node.rng.Next(70, 131);
        }
    }

    [DebuggerDisplay("Creation ID: {creationID}, Graph name: {graphName}")]
    [Serializable]
    class PersonConnection : Connection {
        static int id = 0;
        string creationID;
        string graphName;

        public double TemporalProximity { get; set; }
        public double PhysicalProximity { get; set; }

        public override void SetName(string name) {
            graphName = name;
        }

        public PersonConnection(string t) : base() {
            creationID = id++.ToString();

            switch (t.ToUpper()) {
                case "FAMILY":
                    Traits.Add("Proximity", Node.rng.Next(75, 101));
                    break;
                case "FRIENDS":
                    Traits.Add("Proximity", Node.rng.Next(40, 76));
                    break;
                case "WORK":
                    Traits.Add("Proximity", Node.rng.Next(25, 61));
                    break;
                case "ACQUIANTANCES":
                    Traits.Add("Proximity", Node.rng.Next(5, 46));
                    break;
                default:
                    Traits.Add("Proximity", Node.rng.Next(1, 101));
                    break;
            }
        }

        public override float Strength() {
            return (float)Traits["Proximity"] * 0.1f;
        }
    }
}
