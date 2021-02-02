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

        public override void SetName(string name) {
            graphName = name;
        }

        public PersonConnection(string t) : base() {
            creationID = id++.ToString();

            switch (t.ToUpper()) {
                case "FAMILY":
                    Traits.Add("Physical Proximity", Node.rng.Next(25, 101));
                    Traits.Add("Temporal Proximity", Node.rng.Next(75, 101));
                    break;
                case "FRIENDS":
                    Traits.Add("Physical Proximity", Node.rng.Next(40, 76));
                    Traits.Add("Temporal Proximity", Node.rng.Next(40, 101));
                    break;
                case "WORK":
                    Traits.Add("Physical Proximity", Node.rng.Next(25, 61));
                    Traits.Add("Temporal Proximity", Node.rng.Next(40, 81));
                    break;
                case "ACQUIANTANCES":
                    Traits.Add("Physical Proximity", Node.rng.Next(5, 46));
                    Traits.Add("Temporal Proximity", Node.rng.Next(5, 16));
                    break;
                default:
                    Traits.Add("Physical Proximity", Node.rng.Next(1, 101));
                    Traits.Add("Temporal Proximity", Node.rng.Next(1, 101));
                    break;
            }
        }

        public override float Strength() {
            return (float)Traits["Proximity"] * 0.1f;
        }
    }
}
