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
            NodeCreation(SubGraph);
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
                    if (temp.Item2 == null && temp.Item1 != null) {
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

        public District(int population, int density) : base() {
            traits["Population"] = population;
            traits["Density"] = density;
            SubGraph = new Graph();
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
                traits.Add("Awareness", 0);
                if (rng.NextDouble() <= 0.3) {
                    statuses.Add("Asthmatic");
                }
            }
            return;
        }

        
    }
}
