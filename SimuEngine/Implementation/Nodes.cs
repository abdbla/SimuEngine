using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Core;
using SimuEngine;

namespace Implementation {
    [Serializable]
    class City : Node
    {
        const int DISTRICT_AMOUNT = 17;
        public override void NodeCreation(Graph g, NodeCreationInfo creationInfo = NodeCreationInfo.Empty) {
            int tempPopulation = traits["Population"];
            for (int i = 0; i < DISTRICT_AMOUNT; i++) {
                if (i != 16) tempPopulation /= 2;
                SubGraph.Add(new District(tempPopulation, Node.rng.Next(traits["Density"] - 10, traits["Density"] + 11)));
            }
        }

        public City(int population, int density) {
            traits["Population"] = population;
            traits["Density"] = density;
            SubGraph = new Graph();
        }
    }

    [Serializable]
    class District : Node
    {
        public override void NodeCreation(Graph g, NodeCreationInfo info = NodeCreationInfo.Empty) {
            int NUM_PEOPLE = traits["Population"];
            int NUM_FAMILIES = traits["Population"] / 5;
            int NUM_WORK_GROUPS = traits["Population"] / 200;
            int NUM_FRIEND_GROUPS = traits["Population"] / 4;

            Dictionary<Person, PersonGroup> familyPairs = new Dictionary<Person, PersonGroup>();
            Dictionary<Person, PersonGroup> workPairs = new Dictionary<Person, PersonGroup>();
            Dictionary<Person, (PersonGroup, PersonGroup)> friendPairs = new Dictionary<Person, (PersonGroup, PersonGroup)>();

            for (int i = 0; i < NUM_PEOPLE; i++) {
                Person p = new Person();
                p.Name = i.ToString();
                SubGraph.Add(p);
            }
            for (int i = 0; i < NUM_FAMILIES; i++) {
                PersonGroup tempGroup = new PersonGroup("FAMILY");
                g.groups.Add(tempGroup);
                int tempAmount = Node.rng.Next(2, 8);
                for (int j = 0; j < NUM_PEOPLE; j++) {
                    if (!familyPairs.TryGetValue((Person)g.Nodes[i], out _)) {
                        tempGroup.members.Add(g.Nodes[j]);
                        familyPairs[(Person)g.Nodes[j]] = tempGroup;
                    }
                    if (tempGroup.members.Count > tempAmount) break;
                }
            }
            for (int i = 0; i < NUM_FRIEND_GROUPS; i++) {
                PersonGroup tempGroup = new PersonGroup("FRIENDS");
                g.groups.Add(tempGroup);
                int tempAmount = Node.rng.Next(2, 8);
                for (int j = 0; j < NUM_PEOPLE; j++) {
                    (PersonGroup, PersonGroup) temp;
                    friendPairs.TryGetValue((Person)g.Nodes[j], out temp);
                    if (temp.Item2 == null) {
                        tempGroup.members.Add(g.Nodes[j]);
                        friendPairs[(Person)g.Nodes[j]] = (friendPairs[(Person)g.Nodes[j]].Item1, tempGroup);
                    }
                    if (!friendPairs.TryGetValue((Person)g.Nodes[j], out _)) {
                        tempGroup.members.Add(g.Nodes[j]);
                        friendPairs[(Person)g.Nodes[j]] = (tempGroup, null);
                    }
                    if (tempGroup.members.Count > tempAmount) break;
                }
            }
            for (int i = 0; i < NUM_WORK_GROUPS; i++) {
                PersonGroup tempGroup = new PersonGroup("WORK");
                g.groups.Add(tempGroup);
                int tempAmount = Node.rng.Next(2, 8);
                for (int j = 0; j < NUM_PEOPLE; j++) {
                    if (!workPairs.TryGetValue((Person)g.Nodes[j], out _)) {
                        tempGroup.members.Add(g.Nodes[j]);
                        workPairs[(Person)g.Nodes[j]] = tempGroup;
                    }
                    if (tempGroup.members.Count > tempAmount) break;
                }
            }
        }

        public District(int population, int density) {
            traits["Population"] = population;
            traits["Density"] = density;
            NodeCreation(SubGraph);
        }
    }

    [DebuggerDisplay("Name: {this.Name}")]
    [Serializable]
    class Person : Node {
        static int id = 0;
        public Person() : base() {
            this.Name = id++.ToString();
            return;
        }

        public override void NodeCreation(Graph g, NodeCreationInfo info) {
            if (info == NodeCreationInfo.SystemStart) {
                statuses.Add("Healthy");
                traits.Add("Hygiene", rng.Next(1, 101));
                traits.Add("Age", rng.Next(1, 101));
                traits.Add("Infected Time", 0);
                if (rng.NextDouble() <= 0.3) {
                    statuses.Add("Asthmatic");
                }
            }
            return;
        }

        public static List<Event> InitializeEvents() {
            List<Event> personEvents = new List<Event>();

            personEvents.Add(new Event());
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
            personEvents[0].AddOutcome(delegate (Node n, Graph l, Graph w) {
                n.statuses.Remove("Healthy");
                n.statuses.Add("Infected");
            });

            personEvents.Add(new Event());
            personEvents[1].AddReqPossible(delegate (Node n, Graph l, Graph w) {
                double chance = 0;
                if (!n.Statuses.Contains("Infected")) return 0;
                if (n.Statuses.Contains("Asthmatic")) chance += 0.1;
                chance += ((double)n.Traits["Age"] / 150d);
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
                return n.Statuses.Contains("Infected") ? true : false;
            });
            personEvents[3].AddOutcome(delegate (Node n, Graph l, Graph w) {
                n.traits["Infected Time"]++;
            });

            return personEvents;
        }
    }
}
