using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core;

using SimuEngine;

namespace Implementation {
    static class PersonEvents {
        static Event InfectionEvent() {
            var ev = new Event();

            ev.AddReqPossible(delegate (Node self, Graph localGraph, Graph w) {
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
            ev.AddOutcome(delegate (Node n, Graph l, Graph w) {
                n.statuses.Remove("Healthy");
                n.statuses.Add("Infected");
            });

            return ev;
        }

        static Event DeathEvent() {
            var ev = new Event(delegate (Node n, Graph l, Graph w) {
                double chance = 0;
                if (!n.Statuses.Contains("Infected")) return 0;
                if (n.Statuses.Contains("Asthmatic")) chance += 0.02;
                chance += ((double)n.Traits["Age"] / 300d);
                return chance;
            }, null, delegate (Node n, Graph l, Graph w) {
                n.statuses.Remove("Infected");
                n.statuses.Add("Dead");
            });
            return ev;
        }

        static Event WearingMask()
        {
            var ev = new Event(delegate (Node n, Graph l, Graph w) {
                if (n.Statuses.Contains("WearingMask")) return 0;
                else return n.Traits["Awareness"] / 100;
            }, null, delegate (Node n, Graph l, Graph w) {
                foreach(PersonConnection c in n.connections)
                {
                    c.PhysicalProximity -= 20;
                }
                n.statuses.Add("WearingMask");
            });
            return ev;
        }
        static Event RemovinggMask()
        {
            var ev = new Event(delegate (Node n, Graph l, Graph w) {
                if (n.Statuses.Contains("WearingMask")) return n.Traits["Awareness"] / 1000;
                else return 0;
            }, null, delegate (Node n, Graph l, Graph w) {
                foreach (PersonConnection c in n.connections)
                {
                    c.PhysicalProximity += 20;
                }
                n.statuses.Remove("WearingMask");
            });
            return ev;
        }






        static Event RecoveryEvent() {
            var ev = new Event();
            ev.AddReqPossible(delegate (Node n, Graph l, Graph w) {
                double chance = 0;
                if (!n.Statuses.Contains("Infected")) return 0;
                chance = Math.Pow((101 - n.Traits["Age"]) / 100, 14 - n.Traits["Infected Time"]);
                return chance;
            });
            ev.AddOutcome(delegate (Node n, Graph l, Graph w) {
                n.statuses.Remove("Infected");
                n.statuses.Add("Recovered");
            });

            return ev;
        }

        static Event InfectionTimeUpdateEvent() {
            var ev = new Event();
            ev.AddReqGuaranteed(delegate (Node n, Graph l, Graph w) {
                return n.Statuses.Contains("Infected");
            });
            ev.AddOutcome(delegate (Node n, Graph l, Graph w) {
                n.traits["Infected Time"]++;
            });

            return ev;
        }

        public static List<Event> InitializeEvents() {
            List<Event> personEvents = new List<Event>() { InfectionEvent(), DeathEvent(), RecoveryEvent() };

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
            return personEvents;
        }
    }
}
