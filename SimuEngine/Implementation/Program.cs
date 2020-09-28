﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

using Core;
using SimuEngine;
using NodeMonog;

namespace Implementation
{
    class Program
    {
        List<Event> actions = new List<Event>();
        EventListContainer eventList = new EventListContainer();
        Engine engine;

        static void Main() {
            Program p = new Program();
            //TODO: Create implementation running code
            p.InitializeEngine();
            for (int i = 0; i < 150; i++) {
                p.engine.system.Create<Person>(NodeCreationInfo.SystemStart);
            }
            using (Renderer renderer = new Renderer(p.engine.system.graph, p.engine)) {
                
            }
        }

        private void InitializeEngine() {
            actions.Add(new Event());
            actions[0].ReqPossible.Add(delegate (Node n, Graph l, Graph w) {
                return 1;
            });
            actions[0].Outcome.Add(delegate (Node n, Graph l, Graph w) {
                List<string> tStatus = engine.player.selectedNode.statuses;
                tStatus.Add("Healthy");
                tStatus.Remove("Infected");
                tStatus.Remove("Dead");
                tStatus.Remove("Recovered");
            });

            actions.Add(new Event());
            actions[1].ReqPossible.Add(delegate (Node n, Graph l, Graph w) {
                return 1;
            });
            actions[1].Outcome.Add(delegate (Node n, Graph l, Graph w) {
                List<string> tStatus = engine.player.selectedNode.statuses;
                tStatus.Remove("Healthy");
                tStatus.Add("Infected");
                tStatus.Remove("Dead");
                tStatus.Remove("Recovered");
            });
            actions.Add(new Event());
            actions[2].ReqPossible.Add(delegate (Node n, Graph l, Graph w) {
                return 1;
            });

            actions[2].Outcome.Add(delegate (Node n, Graph l, Graph w) {
                List<string> tStatus = engine.player.selectedNode.statuses;
                tStatus.Remove("Healthy");
                tStatus.Remove("Infected");
                tStatus.Add("Dead");
                tStatus.Remove("Recovered");
            });

            actions.Add(new Event());
            actions[3].ReqPossible.Add(delegate (Node n, Graph l, Graph w) {
                return 1;
            });
            actions[3].Outcome.Add(delegate (Node n, Graph l, Graph w) {
                List<string> tStatus = engine.player.selectedNode.statuses;
                tStatus.Remove("Healthy");
                tStatus.Remove("Infected");
                tStatus.Add("Dead");
                tStatus.Remove("Recovered");
            });

            List<Event> personEvents = Person.InitializeEvents();
            eventList.AddEventList(typeof(Person), personEvents);
            engine = new Engine(actions, eventList);
        }
    }

    class Person : Node
    {

        public Person() : base() {
            return;
        }

        public override void NodeCreation(Graph g, NodeCreationInfo info) {
            if (info == NodeCreationInfo.SystemStart) {
                Random rng = new Random();
                statuses.Add("Healthy");
                traits.Add("Hygiene", rng.Next(1, 101));
                traits.Add("Age", rng.Next(1, 101));
                traits.Add("Infected Time", 0);
                if (rng.NextDouble() <= 0.3) {
                    statuses.Add("Asthmatic");
                }
                for (int i = 0; i < Math.Min(rng.Next(1, 3), g.Nodes.Count()); i++) {
                    int t = rng.Next(g.Nodes.Count());
                    if (g.Nodes[t] == this) continue;
                    PersonConnection temp = new PersonConnection();
                    g.AddConnection(this, g.Nodes[t], temp);
                    temp = new PersonConnection();
                    g.AddConnection(g.Nodes[t], this, temp);
                }
            }
            return;
        }

        public static List<Event> InitializeEvents() {
            List<Event> personEvents = new List<Event>();

            personEvents.Add(new Event());
            personEvents[0].AddReqPossible(delegate (Node n, Graph l, Graph w) {
                double chance = 0;
                if (n.Statuses.Contains("Dead") || n.Statuses.Contains("Recovered")) return 0;
                foreach ((Connection, Node) m in l.GetConnections(n)) {
                    if (m.Item2.Statuses.Contains("Infected")) {
                        chance += (double)(m.Item1.Traits["Proximity"] / 100) * (double)((100 - m.Item2.Traits["Hygiene"]) / 100) * (double)((100 - n.Traits["Hygiene"]) / 100);
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
                chance += (double)(n.Traits["Age"] / 150);
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
                chance = Math.Pow(101 - n.Traits["Age"], 14 - n.Traits["Infected Tíme"]);
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

    class PersonConnection : Connection
    {
        public PersonConnection() : base() {
            traits.Add("Proximity", new Random().Next(1, 101));
        }

        public override float Strength() {
            return 100f - (float)Traits["Proximity"];
        }
    }
}
