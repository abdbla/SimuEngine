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
                if (self.statuses.Contains("Vaccinated")) return 0;
                double chance = 0.0;
                const double infectionConst = 0.00001d;
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
            const double lethalityConst = 0.00015d;
            var ev = new Event();
            ev.AddReqPossible(delegate (Node n, Graph l, Graph w) {
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
            });
            if (Implementation.Program.YesNo("Does the Death Event remove statuses?")) {
                ev.AddOutcome(delegate (Node n, Graph l, Graph w) {
                    n.statuses.Clear();
                    n.statuses.Add("Dead");
                });
            }
            ev.AddOutcome(delegate (Node n, Graph l, Graph w)
            {
                foreach (var g in n.groups) {
                    switch (g.statuses[0].ToUpper()) { //the first status *should* be the type of the group...
                        case "FAMILY":
                            foreach (var p in g.members) {
                                p.traits["Awareness"] += 60;
                                p.traits["Awareness"] = Math.Min(p.traits["Awareness"], 100);
                            }
                            break;
                        case "WORK":
                            foreach (var p in g.members) {
                                p.traits["Awareness"] += 15;
                                p.traits["Awareness"] = Math.Min(p.traits["Awareness"], 100);
                            }
                            break;
                        case "FRIENDS":
                            foreach (var p in g.members) {
                                p.traits["Awareness"] += 30;
                                p.traits["Awareness"] = Math.Min(p.traits["Awareness"], 100);
                            }
                            break;
                        case "ACQUIANTANCES":
                            foreach (var p in g.members) {
                                p.traits["Awareness"] += 5;
                                p.traits["Awareness"] = Math.Min(p.traits["Awareness"], 100);
                            }
                            break;
                        default:
                            break;

                    }
                }
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
                if (n.Statuses.Contains("Isolated") || n.Traits["Awareness"] < 70) return 0;
                if ((n.statuses.Contains("Tested: True Positive") || n.statuses.Contains("Tested: False Positive")) && n.traits["Awareness"] >= 20) return 1;
                else return (n.Traits["Awareness"] - 70d) / 100d;
            }, null, delegate (Node n, Graph l, Graph w)
            {
                foreach ((Connection c, Node adjecentNode) in l.GetOutgoingConnections(n))
                {
                    //All non family members have a temproal proximity of 0
                    if (!n.groups.Find(x => x.Statuses.Contains("Family")).Members.Contains(adjecentNode)) ((PersonConnection)c).TemporalProximity = 0;
                }
                n.statuses.Add("Isolated");
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

        static Event SusceptibleEvent() {
            Event ev = new Event();
            ev.AddReqPossible(delegate (Node n, Graph l, Graph w) {
                if (n.statuses.Contains("Recovered")) return 0.001d * (n.traits["Immune Strength"] / 100d);
                return 0d;
            });
            ev.AddOutcome(delegate (Node n, Graph l, Graph w) {
                n.statuses.Remove("Recovered");
                n.statuses.Add("Healthy");
            });
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
                if (l.parent.traits["Testing Capacity"] > 0) return;
                n.statuses.Add("Tested");
                l.parent.traits["Testing Capacity"]--;
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

        static Event GetMedicinalSupport() {
            Event ev = new Event();
            ev.AddReqPossible(delegate (Node n, Graph l, Graph w) {
                if (n.statuses.Contains("Getting Support") || !n.statuses.Contains("Infected")) return 0;
                double chance = 0;
                chance += n.traits["Age"] / 300d;
                chance += n.traits["Health"] / 300d;
                return chance;
            });
            ev.AddOutcome(delegate (Node n, Graph l, Graph w) {
                if (n.traits["Health"] > 40) {
                    n.statuses.Add("Getting Support: Light");
                    n.traits["Medicinal Support"] += 20;
                } else {
                    n.statuses.Add("Getting Support: Heavy");
                    n.traits["Medicinal Support"] += 50;
                }
            });
            return ev;
        }

        static Event RemoveMedicinalSupport() {
            Event ev = new Event();
            ev.AddReqGuaranteed(delegate (Node n, Graph l, Graph w) {
                if (n.statuses.Contains("Recovered") || n.statuses.Contains("Dead")) return true;
                return false;
            });
            ev.AddOutcome(delegate (Node n, Graph l, Graph w) {
                n.statuses.Remove("Getting Support");
                if (n.statuses.Contains("Getting Support: Light")) { n.statuses.Remove("Getting Support: Light"); n.traits["Medicinal Support"] -= 20; }
                if (n.statuses.Contains("Getting Support: Heavy")) { n.statuses.Remove("Getting Support: Heavy"); n.traits["Medicinal Support"] -= 50; }
            });
            return ev;
        }

        static Event GetVaccinated() {
            Event ev = new Event();
            ev.AddReqPossible(delegate (Node n, Graph l, Graph w) {
                if (!l.parent.statuses.Contains("Vaccination Implemented") || n.statuses.Contains("Vaccinated")) return 0;
                return n.traits["Awareness"] / 100d;
            });
            ev.AddOutcome(delegate (Node n, Graph l, Graph w) {
                if (l.parent.traits["Vaccination Capacity"] == 0) return;
                l.parent.traits["Vaccination Capacity"]--;
                n.statuses.Add("Vaccinated");
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

        static Event ImplementVaccination() {
            Event ev = new Event();
            ev.AddReqPossible(delegate (Node n, Graph l, Graph w) {
                if (n.statuses.Contains("Vaccination Implemented") || !l.parent.statuses.Contains("Vaccination Started")) return 0;
                if (n.SubGraph.FindAllNodes(s => s.statuses.Contains("Dead")).Count == 0) return 0;
                double chance = (double)n.SubGraph.FindAllNodes(s => s.statuses.Contains("Dead")).Count / (double)n.traits["Population"];
                return chance;
            });
            ev.AddOutcome(delegate (Node n, Graph l, Graph w) {
                n.statuses.Add("Vaccination Implemented");
            });
            return ev;
        }

        static Event StartTesting() {
            Event ev = new Event();
            ev.AddReqPossible(delegate (Node n, Graph l, Graph w) {
                if (n.statuses.Contains("Testing Started")) return 0;
                return Math.Atan((double)n.traits["Time"] / 3d);
            });
            ev.AddOutcome(delegate (Node n, Graph l, Graph w) {
                n.statuses.Add("Testing Started");
            });
            return ev;
        }

        static Event StartVaccinating() {
            Event ev = new Event();
            ev.AddReqPossible(delegate (Node n, Graph l, Graph w) {
                if (n.statuses.Contains("Vaccination Started")) return 0;
                return Math.Atan((double)n.traits["Time"] / 450d);
            });
            ev.AddOutcome(delegate (Node n, Graph l, Graph w) {
                n.statuses.Add("Vaccination Started");
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
                foreach (var g in n.SubGraph.Nodes) {
                    if (g.statuses.Contains("Testing Implemented")) {
                        n.traits["Testing Capacity"] = n.traits["Population"] / 100;
                    }
                    if (g.statuses.Contains("Vaccination Implemented")) {
                        n.traits["Vaccination Capacity"] = n.traits["Population"] = 400;
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
                GetTested(),
                GetMedicinalSupport(),
                RemoveMedicinalSupport(),
                GetVaccinated(),
            };

            personEvents.Add(new Event());
            return personEvents;
        }

        public static List<Event> InitializeDistrictEvents() {
            List<Event> districtEvents = new List<Event> {
                ImplementTesting(),
                CheckInfection(),
                ImplementVaccination(),
                DistrictInfection(),
            };

            return districtEvents;
        }

        public static List<Event> InitializeCityEvents() {
            List<Event> cityEvents = new List<Event>() {
                StartTesting(),
                StartVaccinating(),
                KeepCount(),
            };

            return cityEvents;
        }
    }
}
