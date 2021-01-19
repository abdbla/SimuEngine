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
            int NUM_GROUPS = 0;

            List<Person> more = new List<Person>();
            for (int i = 0; i < NUM_GROUPS; i++) {
                g.groups.Add(new PersonGroup());
            }
            for (int i = 0; i < NUM_PEOPLE; i++) {
                GraphSystem s = engine.system;
                s.Create<Person>(NodeCreationInfo.SystemStart);
                Node n = g.Nodes[i];
                more.Add((Person)n);
                n.Name = i.ToString();
                n.groups.Add(s.graph.groups[i % NUM_GROUPS]);
                n.groups[0].members.Add(n);
            }

            Dictionary<Person, int> totalConns = new Dictionary<Person, int>();
            Dictionary<Person, int> curConns = new Dictionary<Person, int>();
            List<Person> remaining = new List<Person>();
            List<bool> inters = new List<bool>();
            remaining.AddRange(more);
            foreach (Node node in more) { inters.Add(false); }

            var rng = Node.rng;
            for (int i = 0; i < more.Count; i++) {
                totalConns[more[i]] = rng.Next(2, 7);
                curConns[more[i]] = 0;
            }

            while (remaining.Count > 1) {
                if (remaining.Count % 100000 == 0 && remaining.Count < NUM_PEOPLE) {
                    Console.WriteLine($"Remaining: {remaining.Count}");
                }

                bool inter = false;
                if (rng.NextDouble() <= 0.15) inter = true;
                var x1 = rng.Next(remaining.Count);
                if (inters[x1]) inter = false;
                var x2 = rng.Next(remaining[x1].groups[0].members.Count);
                if (inter) { x2 = rng.Next(remaining.Count); }
                if (!inter && (remaining[x1] == remaining[x1].groups[0].members[x2])
                    || (x1 == x2 && inter)) {
                    continue;
                }
                var node1 = remaining[x1];
                Person node2;
                if (inter) {
                    node2 = remaining[x2];
                } else {
                    node2 = (Person)remaining[x1].groups[0].members[x2];
                }
                string ctype = inter ? "Interconnection" : node2.groups[0].statuses[0];
                engine.system.graph.AddConnection(node1, node2, new PersonConnection(ctype));
                engine.system.graph.AddConnection(node2, node1, new PersonConnection(ctype));
                if (inter) inters[x1] = true;
                curConns[node1] += 1;
                curConns[node2] += 1;
                if (curConns[node1] == totalConns[node1]) {
                    if (x1 < x2) {
                        x2 -= 1;
                    }
                    remaining.RemoveAt(x1);
                    inters.RemoveAt(x1);
                }
                if (curConns[node2] == totalConns[node2]) {
                    int b = remaining.FindIndex(n => n == node2);
                    remaining.RemoveAt(b);
                    inters.RemoveAt(b);
                }
            }
        }

        public District(int population, int density) {
            traits["Population"] = population;
            traits["Density"] = density;
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

        
    }
}
