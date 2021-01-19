using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

using Core;
using SimuEngine;
using NodeMonog;
using System.IO;

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

            const string TEST_FILE_DIR = @"C:\Users\theodor.strom\SimuEngine\";

            if (p.engine.SaveExists(TEST_FILE_DIR)) {
                Console.WriteLine("Found save file");
                try {
                    p.engine.Load(TEST_FILE_DIR);
                    Console.WriteLine("Deserialized from save file");
                } catch (OutOfMemoryException) {
                    Console.WriteLine("Ran out of memory while deserializing");
                    throw new SystemException("Ran out of memory while deserializing");
                } catch (SerializationException ex) {
                    Console.WriteLine($"Error while deserializing ({ex}), deleting file.");
                    p.engine.CleanDir(TEST_FILE_DIR);
                }
            }
            if (p.engine.system.graph.Count.Nodes == 0) {
                Console.WriteLine("No saved system found, recreating...");
                p.InitializeGraphSystem();
                Console.WriteLine("Finished initialization, serializing...");
                try {
                    p.engine.Save(TEST_FILE_DIR);
                    Console.WriteLine("Graph initialization complete");
                } catch (IOException) {
                    Console.WriteLine("IO error while serializing, won't keep going");
                } catch (OutOfMemoryException) {
                    Console.WriteLine("Ran out of memory while serializing, no serialisation");
                }
            }

            using (Renderer renderer = new Renderer(p.engine)) {
                renderer.Run();
            }
        }

        /*private void InitializeGraphSystem() {
            const int NUM_GROUPS = 100000;
            const int NUM_PEOPLE = 500000;

            List<Person> more = new List<Person>();
            for (int i = 0; i < NUM_GROUPS; i++) {
                engine.system.graph.groups.Add(new PersonGroup());
            }
            for (int i = 0; i < NUM_PEOPLE; i++) {
                GraphSystem s = engine.system;
                s.Create<Person>(NodeCreationInfo.SystemStart);
                Node n = engine.system.graph.Nodes[i];
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

            Graph tmpsubgraph = new Graph();
            {
                Person p1 = new Person();
                p1.Name = "Billy";
                tmpsubgraph.Add(p1);
                Person p2 = new Person();
                p2.Name = "Charlie";
                tmpsubgraph.Add(p2);
                tmpsubgraph.AddConnection(p1, p2, new PersonConnection("Family"));
                tmpsubgraph.AddConnection(p2, p1, new PersonConnection("Family"));

                engine.system.graph.FindNode(x => x.Name == "0").SubGraph = tmpsubgraph;

            }

            {
                Graph tmptmpsubgraph = new Graph();
                Person p1 = new Person();
                p1.Name = "Charlie deep";
                tmptmpsubgraph.Add(p1);
                Person p2 = new Person();
                p2.Name = "ALGNAIUSHDU";
                tmptmpsubgraph.Add(p2);
                tmptmpsubgraph.AddConnection(p1, p2, new PersonConnection("Family"));
                tmptmpsubgraph.AddConnection(p2, p1, new PersonConnection("Family"));

                tmpsubgraph.Nodes[0].SubGraph = tmptmpsubgraph;

            }
        }*/

        private void InitializeEngine() {
            actions.Add(("make healthy", new GuaranteedEvent(
                delegate (Node n, Graph l, Graph w) {
                    List<string> tStatus = engine.player.selectedNode.statuses;
                    tStatus.Add("Healthy");
                    tStatus.Remove("Infected");
                    tStatus.Remove("Dead");
                    tStatus.Remove("Recovered");
                }
            )));

            actions.Add(("Make infected", new GuaranteedEvent(
                delegate (Node n, Graph l, Graph w) {
                    List<string> tStatus = engine.player.selectedNode.statuses;
                    tStatus.Remove("Healthy");
                    tStatus.Add("Infected");
                    tStatus.Remove("Dead");
                    tStatus.Remove("Recovered");
                }
            )));

            actions.Add(("Make dead", new GuaranteedEvent(
                delegate (Node n, Graph l, Graph w) {
                    List<string> tStatus = engine.player.selectedNode.statuses;
                    tStatus.Remove("Healthy");
                    tStatus.Remove("Infected");
                    tStatus.Add("Dead");
                    tStatus.Remove("Recovered");
                }
            )));

            actions.Add(("Make recovered", new GuaranteedEvent(delegate (Node n, Graph l, Graph w) {
                List<string> tStatus = engine.player.selectedNode.statuses;
                tStatus.Remove("Healthy");
                tStatus.Remove("Infected");
                tStatus.Remove("Dead");
                tStatus.Add("Recovered");
            })));

            List<Event> personEvents = NodeEvents.InitializeEvents();
            eventList.AddEventList(typeof(Person), personEvents);
            engine = new Engine(actions, eventList);
        }
    }
}
