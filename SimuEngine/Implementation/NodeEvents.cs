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
                        var prox = conn.PhysicalProximity / 100 * conn.TemporalProximity * ((double)n.traits["Viral Intensity"] / 100d) * ((double)(150 - (n.traits["Hygiene"] + self.traits["Hygiene"]) / 2) / 100d) * ((double)self.traits["Immune Strength"] / 100d) * ((double)self.traits["Genetic Factor"] / 100d);
                        chance = 1 - prox;
                    }
                }
                return chance;
            });
            ev.AddOutcome(delegate (Node n, Graph l, Graph w)
            {
                n.statuses.Remove("Healthy");
                n.statuses.Add("Infected");
                n.traits.Add("Infected Time", 0);
                n.traits.Add("Medicinal Support", 100);
                n.traits.Add("Viral Intensity", Node.rng.NextGaussian(100, 10));
            });

            return ev;
        }
        static Event DeathEvent()
        {
            const double lethalityConst = 0.2d;
            var ev = new Event(delegate (Node n, Graph l, Graph w)
            {
                double chance = 0;
                if (!n.Statuses.Contains("Infected")) return 0;
                chance = ((double)n.Traits["Age"] / 300d) * ((double)(150 - n.Traits["Health"]) / 100d) * ((double)n.Traits["Viral Intensity"] / 100d) * ((double)(200 - n.Traits["Immune Strength"]) / 100d) * ((double)(200 - n.Traits["Medicinal Support"]) / 100d) * ((double)n.Traits["Genetic Factor"] / 100d) * lethalityConst;
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
            Event ev = new Event();
            
            ev.AddReqPossible(delegate (Node n, Graph l, Graph w)
            {
                double chance = 0;
                if (!n.Statuses.Contains("Infected")) return 0;
                chance = Math.Pow((101 - n.Traits["Age"]) / 100, 14 - n.Traits["Infected Time"]) * 2.5f;
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
         
        static Event LocalWork()
        {
            Event ev = new Event();
            ev.AddReqGuaranteed(delegate (Node n, Graph l, Graph w)
            {
                return n.Statuses.Contains("Working") && !n.Statuses.Contains("Isolated");
            });
            ev.AddOutcome(delegate (Node n, Graph l, Graph w)
            {
                InfectionEvent();
            });
            return ev;
        }

        static Event InfectionTimeUpdateEvent()
        {
            Event ev = new Event();
            ev.AddReqGuaranteed(delegate (Node n, Graph l, Graph w)
            {
                return n.Statuses.Contains("Infected");
            });
            ev.AddOutcome(delegate (Node n, Graph l, Graph w)
            {
                InfectionEvent();
            });
            return ev;
        }

        static Event GetTested() {
            Event ev = new Event();
            ev.AddReqPossible(delegate (Node n, Graph l, Graph w) {
                if (n.statuses.Contains("Tested")) {
                    return 0d;
                }
                double chance = 0;
                if (n.statuses.Contains("Infected")) {
                    chance += ((double)n.traits["Awareness"] / 100d) - 0.3;
                    chance += ((double)n.traits["Viral Intensity"] - 100d) / 100d;
                }
                if (!n.statuses.Contains("Infected")) {
                    chance += 0.5 - ((double)n.traits["Awareness"] / 100d);
                }
                return chance;
            });
            ev.AddOutcome(delegate (Node n, Graph l, Graph w) {
                n.statuses.Add("Tested");
                if (n.statuses.Contains("Infected")) {
                    if (Node.rng.NextDouble() > 0.2) {
                        n.statuses.Add("Tested: Positive");
                    } else {
                        n.statuses.Add("Tested: Negative");
                    }
                }
                if (!n.statuses.Contains("Infected")) {
                    if (Node.rng.NextDouble() > 0.01) {
                        n.statuses.Add("Tested: Negative");
                    } else {
                        n.statuses.Add("Tested: Positive");
                    }
                }
            });
            return ev;
        }
        public static List<Event> InitializePersonEvents()
        {
            List<Event> personEvents = new List<Event>() {
                InfectionEvent(),
                DeathEvent(),
                RecoveryEvent(),
                InfectionTimeUpdateEvent(),
                WearingMask(),
                RemovinggMask(),
                IsolationEvent(),
                LocalWork(),
                GetTested()
            };

            personEvents.Add(new Event());
            return personEvents;
        }

        public static List<Event> InitializeDistrictEvents() {
            List<Event> districtEvents = new List<Event> {

            };

            return districtEvents;
        }

        public static List<Event> InitializeCityEvents() {
            List<Event> cityEvents = new List<Event>() {

            };

            return cityEvents;
        }
    }
}
