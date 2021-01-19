using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Core;
using SimuEngine;

namespace Implementation
{
    public class Event
    {
        //The function takes the source node which triggered the event, the Graph it's contained in, and the top-level graph. It returns a bool if the requirement is fulfilled.
        // List<Func<Node, Graph, Graph, double>> reqPossible;
        // List<Func<Node, Graph, Graph, bool>> reqGuaranteed;
        // List<Action<Node, Graph, Graph>> outcome;

        public List<Func<Node, Graph, Graph, double>> ReqPossible
        {
            get; private set;
        } = new List<Func<Node, Graph, Graph, double>>();
        public List<Func<Node, Graph, Graph, bool>> ReqGuaranteed
        {
            get; private set;
        } = new List<Func<Node, Graph, Graph, bool>>();
        public List<Action<Node, Graph, Graph>> Outcome
        {
            get; private set;
        } = new List<Action<Node, Graph, Graph>>();



        public void AddReqPossible(Func<Node, Graph, Graph, double> req)
        {
            ReqPossible.Add(req);
        }
        public void AddReqGuaranteed(Func<Node, Graph, Graph, bool> req)
        {
            ReqGuaranteed.Add(req);
        }
        public void AddOutcome(Action<Node, Graph, Graph> ev)
        {
            Outcome.Add(ev);
        }

        public Event() { }

        public Event(Func<Node, Graph, Graph, double> reqPossible,
                     Func<Node, Graph, Graph, bool> reqGuaranteed,
                     Action<Node, Graph, Graph> outcome)
        {
            if (reqPossible != null) ReqPossible.Add(reqPossible);
            if (reqGuaranteed != null) ReqGuaranteed.Add(reqGuaranteed);
            Outcome.Add(outcome);
        }

        public static List<Event> InitializeEvents()
        {
            List<Event> personEvents = new List<Event>();

            personEvents.Add(new Event());
            personEvents[0].AddReqPossible(delegate (Node n, Graph l, Graph w) {
                double chance = 0;
                if (!n.Statuses.Contains("Healthy")) return 0;
                foreach ((Connection, Node) m in l.GetOutgoingConnections(n))
                {
                    if (m.Item2.Statuses.Contains("Infected"))
                    {
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

    public class GuaranteedEvent : Event
    {
        public GuaranteedEvent(Action<Node, Graph, Graph> outcome) : base((n, l, w) => 1, (n, l, w) => true, outcome) { }
    }
}

