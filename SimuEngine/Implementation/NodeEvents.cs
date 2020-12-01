using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core;

using SimuEngine;

namespace Implementation {
    static class PersonEvents {
        public static List<Event> InitializeEvents() {
            List<Event> personEvents = new List<Event>();

            personEvents.Add(new Event());
#if false
            personEvents[0].AddReqPossible(delegate (Node n, Graph l, Graph w) {
                double chance = 0;
                if (!n.Statuses.Contains("Healthy")) return 0;
                foreach ((Connection, Node) m in l.GetOutgoingConnections(n)) {
                    if (m.Item2.Statuses.Contains("Infected")) {
                        chance += (double)((m.Item1.Traits["Proximity"]) + (double)((100 - m.Item2.Traits["Hygiene"])) + (double)((100 - n.Traits["Hygiene"])) / 300);
                    }
                }
                return chance;
            });
#endif
            personEvents[0].AddReqPossible(delegate (Node self, Graph localGraph, Graph w) {
                double chance = 0.0;
                if (!self.Statuses.Contains("Healthy")) return 0;
                foreach ((var _conn, var n) in localGraph.GetOutgoingConnections(self)) {
                    var conn = (PersonConnection)_conn;
                    if (n.Statuses.Contains("Infected")) {
                        var prox = conn.PhysicalProximity / 100 * conn.TemporalProximity;
                        chance = (1 - prox);
                    }
                }
                return chance;
            });
            personEvents[0].AddOutcome(delegate (Node n, Graph l, Graph w) {
                n.statuses.Remove("Healthy");
                n.statuses.Add("Infected");
            });

            personEvents.Add(new Event());
            personEvents[1].AddReqPossible(delegate (Node n, Graph l, Graph w) {
                double chance = 0;
                if (!n.Statuses.Contains("Infected")) return 0;
                if (n.Statuses.Contains("Asthmatic")) chance += 0.02;
                chance += ((double)n.Traits["Age"] / 300d);
                return chance;
            });
            personEvents[1].AddOutcome(delegate (Node n, Graph l, Graph w) {
                n.statuses.Remove("Infected");
                n.statuses.Add("Dead");
            });

            personEvents.Add(new Event());
            personEvents[2].AddReqPossible(delegate (Node n, Graph l, Graph w) {
                double chance = 0;
                if (!n.Statuses.Contains("Infected")) return 0;
                chance = Math.Pow((101 - n.Traits["Age"]) / 100, 14 - n.Traits["Infected Time"]);
                return chance;
            });
            personEvents[2].AddOutcome(delegate (Node n, Graph l, Graph w) {
                n.statuses.Remove("Infected");
                n.statuses.Add("Recovered");
            });

            personEvents.Add(new Event());
            personEvents[3].AddReqGuaranteed(delegate (Node n, Graph l, Graph w) {
                return n.Statuses.Contains("Infected");
            });
            personEvents[3].AddOutcome(delegate (Node n, Graph l, Graph w) {
                n.traits["Infected Time"]++;
            });

            return personEvents;
        }
    }
}
