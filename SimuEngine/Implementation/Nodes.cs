using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Core;
using SimuEngine;
using System.Threading;

namespace Implementation {
    [Serializable]
    class City : Node
    {
        const int DISTRICT_AMOUNT = 17;
        public override void NodeCreation(Graph g, NodeCreationInfo creationInfo = NodeCreationInfo.Empty) {
            int tempPopulation = traits["Population"];
            List<Task<District>> districtCreationTasks = new List<Task<District>>();

            for (int i = 0; i < DISTRICT_AMOUNT; i++) {
                if (i != 16) tempPopulation /= 2;
                int n = rng.Next(traits["Density"] - 10, traits["Density"] + 11);
                districtCreationTasks.Add(
                    Task.Run(() => { int x = i; return new District(tempPopulation, n); }));  ;
            }
            Task.WaitAll(districtCreationTasks.ToArray());
            foreach (Task<District> dt in districtCreationTasks) {
                SubGraph.Add(dt.Result);
            }

            for (int i = 0; i < DISTRICT_AMOUNT - 1; i++) {
                g.AddConnection(g.Nodes[i], g.Nodes[i + 1], new DistrictConnection());
            }
            g.AddConnection(g.Nodes[0], g.Nodes[16], new DistrictConnection());
            foreach (var district in g.Nodes) {
                for (int i = 0; i < Node.rng.Next(0, 3); i++) {
                    int temp = rng.Next(0, DISTRICT_AMOUNT);
                    if (district != g.Nodes[temp]) {
                        g.AddConnection(district, g.Nodes[temp], new DistrictConnection());
                    }
                }
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
        static int idCounter = 0;
        int idx = ++idCounter;
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
                p.NodeCreation(SubGraph, NodeCreationInfo.SystemStart);
                SubGraph.Add(p);
            }
            for (int i = 0; i < NUM_FAMILIES; i++) {
                PersonGroup tempGroup = new PersonGroup("FAMILY");
                g.groups.Add(tempGroup);
                int tempAmount = Node.rng.Next(2, 8);
                for (int j = 0; j < NUM_PEOPLE; j++) {
                    if (!familyPairs.TryGetValue((Person)g.Nodes[j], out _)) {
                        tempGroup.members.Add(g.Nodes[j]);
                        g.Nodes[j].groups.Add(tempGroup);
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
                        g.Nodes[j].groups.Add(tempGroup);
                        friendPairs[(Person)g.Nodes[j]] = (friendPairs[(Person)g.Nodes[j]].Item1, tempGroup);
                    }
                    if (!friendPairs.TryGetValue((Person)g.Nodes[j], out _)) {
                        tempGroup.members.Add(g.Nodes[j]);
                        g.Nodes[j].groups.Add(tempGroup);
                        friendPairs[(Person)g.Nodes[j]] = (tempGroup, null);
                    }
                    if (tempGroup.members.Count > tempAmount) break;
                }
            }
            for (int i = 0; i < NUM_WORK_GROUPS; i++) {
                PersonGroup tempGroup = new PersonGroup("WORK");
                g.groups.Add(tempGroup);
                int tempAmount = Node.rng.Next(100, 301);
                for (int j = 0; j < NUM_PEOPLE; j++) {
                    if (!workPairs.TryGetValue((Person)g.Nodes[j], out _)) {
                        tempGroup.members.Add(g.Nodes[j]);
                        g.Nodes[j].groups.Add(tempGroup);
                        workPairs[(Person)g.Nodes[j]] = tempGroup;
                    }
                    if (tempGroup.members.Count > tempAmount) break;
                }
            }
            for (int i = 0; i < NUM_PEOPLE; i++) {
                if (!familyPairs[(Person)g.Nodes[i]].statuses.Contains("Initialized")) {
                    PersonGroup family = familyPairs[(Person)g.Nodes[i]];
                    Person t1 = (Person)family.members[0];
                    Person t2 = (Person)family.members[1];
                    for (int k = 0; k < family.members.Count; k++) {
                        if (family.members[k].traits["Age"] > t1.traits["Age"]) {
                            t2 = t1;
                            t1 = (Person)family.members[k];
                        }
                    }
                    t1.statuses.Add("Parent");
                    t2.statuses.Add("Parent");
                    for (int k = 0; k < family.members.Count; k++) {
                        if (!family.members[k].statuses.Contains("Parent")) {
                            family.members[k].statuses.Add("Child");
                        }
                    }
                    family.statuses.Add("Initialized");
                }
                foreach (var person in familyPairs[(Person)g.Nodes[i]].members) {
                    if (person != g.Nodes[i] && g.GetDirectedConnection(person, g.Nodes[i]) == null) {
                        g.AddConnection(person, g.Nodes[i], new PersonConnection("Family"));
                    }
                }
                for (int j = 0; j < rng.Next(2, 8); j++) {
                    int iTemp = rng.Next(0, workPairs[(Person)g.Nodes[i]].members.Count);
                    if (g.Nodes[iTemp] != g.Nodes[i] && g.GetDirectedConnection(workPairs[(Person)g.Nodes[i]].members[iTemp], g.Nodes[i]) != null) {
                        g.AddConnection(g.Nodes[i], workPairs[(Person)g.Nodes[i]].members[iTemp], new PersonConnection("Work"));
                    }
                }
                foreach (var person in friendPairs[(Person)g.Nodes[i]].Item1.members) {
                    if (Node.rng.Next(0, 3) == 0) {
                        g.AddConnection(person, g.Nodes[i], new PersonConnection("Friend"));
                    }
                }
                foreach (var person in friendPairs[(Person)g.Nodes[i]].Item2.members) {
                    if (Node.rng.Next(0, 3) == 0) {
                        g.AddConnection(person, g.Nodes[i], new PersonConnection("Friend"));
                    }
                }
                for (int j = 0; j < rng.Next(2, 6); j++) {
                    int iTemp = rng.Next(0, NUM_PEOPLE);
                    if (g.Nodes[iTemp] != g.Nodes[i] && g.GetDirectedConnection(g.Nodes[iTemp], g.Nodes[i]) != null) {
                        g.AddConnection(g.Nodes[i], g.Nodes[iTemp], new PersonConnection("Acquiantance"));
                    }
                }
            }
        }

        public District(int population, int density) : base() {
            traits["Population"] = population;
            traits["Density"] = density;
            SubGraph = new Graph();
            NodeCreation(SubGraph);
            Console.WriteLine($"District {idx} finished.");
        }
    }

    [DebuggerDisplay("Name: {this.Name}")]
    [Serializable]
    class Person : Node {
        static int id = 0;
        public Person() : base() {
            this.Name = Interlocked.Increment(ref id).ToString();
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
