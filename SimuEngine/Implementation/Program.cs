using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

using Core;
using SimuEngine;
using NodeMonog;
using System.Diagnostics;

namespace Implementation
{
    class Program
    {
        List<(string, Event)> actions = new List<(string, Event)>();
        EventListContainer eventList = new EventListContainer();
        Engine engine;

        static void Main() {
            Program p = new Program();
            //TODO: Create implementation running code
            p.InitializeEngine();
            List<Person> more = new List<Person>();
            for (int i = 0; i < 30; i++) {
                p.engine.system.graph.groups.Add(new PersonGroup());
            }
            for (int i = 0; i < 1000; i++) {
                GraphSystem s = p.engine.system;
                s.Create<Person>(NodeCreationInfo.SystemStart);
                Node n = p.engine.system.graph.Nodes[i];
                more.Add((Person)n);
                n.Name = i.ToString();
                n.groups.Add(s.graph.groups[i % 30]);
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
                totalConns[more[i]] = rng.Next(2, 5);
                curConns[more[i]] = 0;
            }

            while (remaining.Count > 1) {
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
                p.engine.system.graph.AddConnection(node1, node2, new PersonConnection(ctype));
                p.engine.system.graph.AddConnection(node2, node1, new PersonConnection(ctype));
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


            Graph tmpsubgraph = new Graph();
            Person p1 = new Person();
            p1.Name = "Billy";
            tmpsubgraph.Add(p1);
            Person p2 = new Person();
            p2.Name = "Charlie";
            tmpsubgraph.Add(p2);
            tmpsubgraph.AddConnection(p1, p2, new PersonConnection("Family"));
            tmpsubgraph.AddConnection(p2, p1, new PersonConnection("Family"));

            p.engine.system.graph.FindNode(x => x.Name == "0").SubGraph = tmpsubgraph;

            // p.engine.system.graph = new Graph();
            var graph = new Graph();

            var a = new Person();
            var b2 = new Person();
            var c = new Person();
            var d = new Person();
            var e = new Person();
            var f = new Person();
            var g = new Person();
            var h = new Person();
            var i2 = new Person();

            graph.Add(a);
            graph.Add(b2);
            graph.Add(c);
            graph.Add(d);
            graph.Add(e);
            graph.Add(f);
            graph.Add(g);
            graph.Add(h);
            graph.Add(i2);


            graph.AddConnection(a, b2, new PersonConnection("Healthy"));
            graph.AddConnection(b2, c, new PersonConnection("Healthy"));
            graph.AddConnection(c, d, new PersonConnection("Healthy"));
            graph.AddConnection(d, e, new PersonConnection("Healthy"));
            graph.AddConnection(e, f, new PersonConnection("Healthy"));
            graph.AddConnection(f, g, new PersonConnection("Healthy"));
            graph.AddConnection(g, h, new PersonConnection("Healthy"));
            graph.AddConnection(h, i2, new PersonConnection("Healthy"));
            graph.AddConnection(i2, a, new PersonConnection("Healthy"));

            using (Renderer renderer = new Renderer(p.engine)) {
                renderer.Run();
            }
        }

        private void InitializeEngine() {
            actions.Add(("Make healthy", new Event()));
            actions[0].Item2.ReqPossible.Add(delegate (Node n, Graph l, Graph w) {
                return 1;
            });
            actions[0].Item2.Outcome.Add(delegate (Node n, Graph l, Graph w) {
                List<string> tStatus = engine.player.selectedNode.statuses;
                tStatus.Add("Healthy");
                tStatus.Remove("Infected");
                tStatus.Remove("Dead");
                tStatus.Remove("Recovered");
            });

            actions.Add(("Make infected", new Event()));
            actions[1].Item2.ReqPossible.Add(delegate (Node n, Graph l, Graph w) {
                return 1;
            });
            actions[1].Item2.Outcome.Add(delegate (Node n, Graph l, Graph w) {
                List<string> tStatus = engine.player.selectedNode.statuses;
                tStatus.Remove("Healthy");
                tStatus.Add("Infected");
                tStatus.Remove("Dead");
                tStatus.Remove("Recovered");
            });
            actions.Add(("Make dead", new Event()));
            actions[2].Item2.ReqPossible.Add(delegate (Node n, Graph l, Graph w) {
                return 1;
            });

            actions[2].Item2.Outcome.Add(delegate (Node n, Graph l, Graph w) {
                List<string> tStatus = engine.player.selectedNode.statuses;
                tStatus.Remove("Healthy");
                tStatus.Remove("Infected");
                tStatus.Add("Dead");
                tStatus.Remove("Recovered");
            });

            actions.Add(("Make recovered", new Event()));
            actions[3].Item2.ReqPossible.Add(delegate (Node n, Graph l, Graph w) {
                return 1;
            });
            actions[3].Item2.Outcome.Add(delegate (Node n, Graph l, Graph w) {
                List<string> tStatus = engine.player.selectedNode.statuses;
                tStatus.Remove("Healthy");
                tStatus.Remove("Infected");
                tStatus.Remove("Dead");
                tStatus.Add("Recovered");
            });

            List<Event> personEvents = Person.InitializeEvents();
            eventList.AddEventList(typeof(Person), personEvents);
            engine = new Engine(actions, eventList);
        }
    }

    [DebuggerDisplay("name: {this.Name}")]
    class Person : Node
    {
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

    [DebuggerDisplay("Creation ID: {creationID}, Graph name: {graphName}")]
    class PersonConnection : Connection
    {
        static int id = 0;
        string creationID;
        string graphName;

        public override void SetName(string name) {
            graphName = name;
        }

        public PersonConnection(string t) : base() {
            creationID = id++.ToString();

            switch (t) {
                case "Family":
                    traits.Add("Proximity", Node.rng.Next(75, 101));
                    break;
                case "Friends":
                    traits.Add("Proximity", Node.rng.Next(40, 76));
                    break;
                case "Work":
                    traits.Add("Proximity", Node.rng.Next(25, 61));
                    break;
                case "Acquiantances":
                    traits.Add("Proximity", Node.rng.Next(5, 46));
                    break;
                default:
                    traits.Add("Proximity", Node.rng.Next(1, 101));
                    break;
            }
        }

        public override float Strength() {
            return (float)Traits["Proximity"] * 0.1f;
        }
    }

    class PersonGroup : Group
    {
        static int id = 0;
        int idx;
        public PersonGroup() : base() {
            switch (Node.rng.Next(1, 5)) {
                case 1:
                    statuses.Add($"Family");
                    break;
                case 2:
                    statuses.Add($"Work");
                    break;
                case 3:
                    statuses.Add($"Friends");
                    break;
                default:
                    statuses.Add($"Acquiantances");
                    break;
            }
            idx = ++id;
        }
    }
}
