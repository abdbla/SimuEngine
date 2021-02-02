using Core;
using SimuEngine;
using System;
using System.Collections.Generic;
using NodeMonog;

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
                const double infectionConst = 0.1d;
                if (!self.Statuses.Contains("Healthy")) return 0;

                double selfHygiene = self.traits["Hygiene"];
                double immuneStrength = self.traits["Immune Strength"];
                double genetic = self.traits["Genetic Factor"];
                var outgoing = localGraph.GetOutgoingConnections(self);

                foreach ((var _conn, var n) in outgoing)
                {
                    var conn = (PersonConnection)_conn;
                    if (n.Statuses.Contains("Infected"))
                    {
                        double viral = n.traits["Viral Intensity"];
                        double hygiene = n.traits["Hygiene"];

                        var prox = 
                            conn.PhysicalProximity / 100 *
                            conn.TemporalProximity *
                            (viral / 100d) *
                            ((double)(150 - (hygiene + selfHygiene) / 2) / 100d) *
                            (immuneStrength / 100d) *
                            (genetic / 100d) *
                            infectionConst;
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
                n.statuses.Add("Cumulative Infection");
            });

            return ev;
        }
        static Event DeathEvent()
        {
            const double lethalityConst = 0.0001d;
            var ev = new Event(delegate (Node n, Graph l, Graph w)
            {
                double chance = 0;
                if (!n.Statuses.Contains("Infected")) return 0;
                chance = (10 * (double)n.Traits["Age"] / 300d) 
                * ((double)(400 - (3 * n.Traits["Health"])) / 100d) 
                * ((double)n.Traits["Viral Intensity"] / 100d) 
                * ((double)(200 - n.Traits["Immune Strength"]) / 100d) 
                * ((double)(200 - n.Traits["Medicinal Support"]) / 100d) 
                * ((5 * (double)n.Traits["Genetic Factor"]) / 100d) 
                * lethalityConst;
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
                else return n.Traits["Awareness"] / 100d;
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
                if (n.Statuses.Contains("WearingMask")) return n.Traits["Awareness"] / 1000d;
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
                chance = Math.Pow(((101 - n.Traits["Age"]) / 100d) 
                    * (n.traits["Immune Strength"] / 100d) 
                    * (n.traits["Genetic Factor"] / 100d), 
                    2 - (n.Traits["Infected Time"] / 7d));
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
                n.traits["Infected Time"]++;
            });
            return ev;
        }

        static Event GetTested() {
            Event ev = new Event();
            ev.AddReqPossible(delegate (Node n, Graph l, Graph w) {
                if (n.statuses.Contains("Tested") || !l.parent.statuses.Contains("Testing Implemented")) {
                    return 0;
                }
                foreach (var t in l.GetOutgoingConnections(n)) {
                    if (t.Item2.statuses.Contains("Tested: True Positive") || t.Item2.statuses.Contains("Tested: False Positive")) {
                        return 1;
                    }
                }
                double chance = 0;
                if (n.statuses.Contains("Infected")) {
                    chance = ((double)n.traits["Awareness"] / 50d) * ((double)n.traits["Viral Intensity"] / 200d) - 0.5;
                }
                if (!n.statuses.Contains("Infected")) {
                    chance += 0.3 - ((double)n.traits["Awareness"] / 100d);
                }
                return chance;
            });
            ev.AddOutcome(delegate (Node n, Graph l, Graph w) {
                if (l.parent.traits["Tests"] >= w.Nodes[0].traits["Time"] * l.parent.traits["Testing Capacity"]) return;
                n.statuses.Add("Tested");
                l.parent.traits["Tests"]++;
                if (n.statuses.Contains("Infected")) {
                    if (Node.rng.NextDouble() > 0.2) {
                        n.statuses.Add("Tested: True Positive");
                    } else {
                        n.statuses.Add("Tested: False Negative");
                    }
                }
                if (!n.statuses.Contains("Infected")) {
                    if (Node.rng.NextDouble() > 0.01) {
                        n.statuses.Add("Tested: True Negative");
                    } else {
                        n.statuses.Add("Tested: False Positive");
                    }
                }
            });
            return ev;
        }

        static Event ImplementTesting() {
            Event ev = new Event();
            ev.AddReqPossible(delegate (Node n, Graph l, Graph w) {
                if (n.statuses.Contains("Testing Implemented") || !l.parent.statuses.Contains("Testing Started")) return 0;
                if (n.SubGraph.FindAllNodes(s => s.statuses.Contains("Dead")).Count == 0) return 0;
                double chance = (double)n.SubGraph.FindAllNodes(s => s.statuses.Contains("Dead")).Count / (double)n.traits["Population"];
                return chance;
            });
            ev.AddOutcome(delegate (Node n, Graph l, Graph w) {
                n.statuses.Add("Testing Implemented");
            });
            return ev;
        }

        static Event CheckInfection() {
            Event ev = new Event();
            ev.AddReqGuaranteed(delegate (Node n, Graph l, Graph w) {
                if (n.SubGraph.FindAllNodes(s => s.statuses.Contains("Infected")).Count > n.traits["Population"] / 5 && !n.statuses.Contains("Infected")) return true;
                return false;
            });
            ev.AddOutcome(delegate (Node n, Graph l, Graph w) {
                n.statuses.Add("Infected");
            });
            return ev;
        }

        static Event DistrictInfection() {
            Event ev = new Event();
            ev.AddReqPossible(delegate (Node s, Graph l, Graph w) {
                double chance = 1;
                foreach ((Connection, Connection, Node) n in l.GetNeighbors(s)) {
                    if (n.Item3.statuses.Contains("Infected")) {
                        chance *= 1 - ((double)n.Item3.SubGraph.FindAllNodes(f => f.statuses.Contains("Infected")).Count / (double)n.Item3.traits["Population"] * (s.traits["Density"] / 100d));
                    }
                }
                chance = 1 - chance;
                return chance;
            });
            ev.AddOutcome(delegate (Node n, Graph l, Graph w) {
                List<Node> healthy = n.SubGraph.FindAllNodes(node => node.statuses.Contains("Healthy"));
                if (healthy.Count > 0) {
                    Node tmp = healthy[Node.rng.Next(healthy.Count - 1)];
                    tmp.statuses.Remove("Healthy");
                    tmp.statuses.Add("Infected");
                    tmp.traits.Add("Infected Time", 0);
                    tmp.traits.Add("Medicinal Support", 100);
                    tmp.traits.Add("Viral Intensity", Node.rng.NextGaussian(100, 10));
                }
            });
            return ev;
        }

        static Event StartTesting() {
            Event ev = new Event();
            ev.AddReqPossible(delegate (Node n, Graph l, Graph w) {
                if (n.statuses.Contains("Testing Started")) return 0;
                double chance = 0;
                return Math.Atan((double)n.traits["Time"] / 3d);
            });
            ev.AddOutcome(delegate (Node n, Graph l, Graph w) {
                n.statuses.Add("Testing Started");
            });
            return ev;
        }

        static Event KeepCount() {
            Event ev = new Event();
            ev.AddReqGuaranteed(delegate (Node n, Graph l, Graph w) {
                return true;
            });
            ev.AddOutcome(delegate (Node n, Graph l, Graph w) {
                n.traits["Time"]++;
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
                ImplementTesting(),
                CheckInfection(),
                DistrictInfection(),
            };

            return districtEvents;
        }

        public static List<Event> InitializeCityEvents() {
            List<Event> cityEvents = new List<Event>() {
                StartTesting(),
                KeepCount(),
            };

            return cityEvents;
        }
    }
}
