using Core;
using SimuEngine;
using System;
using System.Collections.Generic;

namespace Implementation
{
    static class PersonEvents
    {
        static Event InfectionEvent()
        {
            var ev = new Event();

            ev.AddReqPossible(delegate (Node self, Graph localGraph, Graph w)
            {
                double chance = 0.0;
                if (!self.Statuses.Contains("Healthy")) return 0;
                foreach ((var _conn, var n) in localGraph.GetOutgoingConnections(self))
                {
                    var conn = (PersonConnection)_conn;
                    if (n.Statuses.Contains("Infected"))
                    {
                        var prox = conn.PhysicalProximity / 100 * conn.TemporalProximity;
                        chance = (1 - prox);
                    }
                }
                return chance;
            });
            ev.AddOutcome(delegate (Node n, Graph l, Graph w)
            {
                n.statuses.Remove("Healthy");
                n.statuses.Add("Infected");
            });

            return ev;
        }
        static Event DeathEvent()
        {
            var ev = new Event(delegate (Node n, Graph l, Graph w)
            {
                double chance = 0;
                if (!n.Statuses.Contains("Infected")) return 0;
                if (n.Statuses.Contains("Asthmatic")) chance += 0.02;
                chance += ((double)n.Traits["Age"] / 300d);
                return chance;
            }, null, delegate (Node n, Graph l, Graph w)
            {
                n.statuses.Remove("Infected");
                n.statuses.Add("Dead");
            });
            return ev;
        }
        static Event WearingMask()
        {
            Event ev = new Event(delegate (Node n, Graph l, Graph w)
            {
                if (n.Statuses.Contains("WearingMask")) return 0;
                else return n.Traits["Awareness"] / 100;
            }, null, delegate (Node n, Graph l, Graph w)
            {
                foreach (PersonConnection c in n.connections)
                {
                    c.PhysicalProximity -= 20;
                }
                n.statuses.Add("WearingMask");
            });
            return ev;
        }
        static Event RemovinggMask()
        {
            Event ev = new Event(delegate (Node n, Graph l, Graph w)
            {
                if (n.Statuses.Contains("WearingMask")) return n.Traits["Awareness"] / 1000;
                else return 0;
            }, null, delegate (Node n, Graph l, Graph w)
            {
                foreach (PersonConnection c in n.connections)
                {
                    c.PhysicalProximity += 20;
                }
                n.statuses.Remove("WearingMask");
            });
            ev.RequiredStatuses = new HashSet<string>() { "WearingMask" };
            return ev;
        }
        static Event IsolationEvent()
        {
            Event ev = new Event(delegate (Node n, Graph l, Graph w)
            {
                if (n.Statuses.Contains("Isolated") && n.Traits["Awareness"] < 70) return 0;
                else return (n.Traits["Awareness"] - 70 )/ 100;
            }, null, delegate (Node n, Graph l, Graph w)
            {
                foreach ((Connection c, Node adjecentNode) in l.GetOutgoingConnections(n))
                {
                    //All non family members have a temproal proximity of 0
                    if (!n.groups.Find(x => x.Statuses.Contains("Family")).Members.Contains(adjecentNode)) ((PersonConnection)c).TemporalProximity = 0;
                    n.statuses.Add("Isolated");
                }
            }
                );
            return ev;
        }
        static Event RecoveryEvent()
        {
            var ev = new Event();
            ev.AddReqPossible(delegate (Node n, Graph l, Graph w)
            {
                double chance = 0;
                if (!n.Statuses.Contains("Infected")) return 0;
                chance = Math.Pow((101 - n.Traits["Age"]) / 100, 14 - n.Traits["Infected Time"]);
                return chance;
            });
            ev.AddOutcome(delegate (Node n, Graph l, Graph w)
            {
                n.statuses.Remove("Infected");
                n.statuses.Add("Recovered");
            });

            ev.AppliesStatus = new HashSet<string>() { "Recovered" };

            return ev;
        }
        static Event InfectionTimeUpdateEvent()
        {
            var ev = new Event();
            ev.AddReqGuaranteed(delegate (Node n, Graph l, Graph w)
            {
                return n.Statuses.Contains("Infected");
            });
            ev.AddOutcome(delegate (Node n, Graph l, Graph w)
            {
                n.traits["Infected Time"]++;
            });

            return ev;
        }
        public static List<Event> InitializeEvents()
        {
            List<Event> personEvents = new List<Event>() {
                InfectionEvent(),
                DeathEvent(),
                RecoveryEvent(),
                InfectionTimeUpdateEvent(),
                WearingMask(),
                RemovinggMask(),
                IsolationEvent()
            };

            personEvents.Add(new Event());
            return personEvents;
        }
    }
}
