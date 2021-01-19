using System;
using System.Collections.Generic;
using System.Text;

using Core;
using SharpDX.Direct3D11;

namespace SimuEngine {
    public class EventListContainer {
        private Dictionary<Type, List<Event>> eventLists;

        public EventListContainer() {
            eventLists = new Dictionary<Type, List<Event>>();
        }
        public List<Event> GetEventList(Type type) {
            return eventLists[type];
        }
        public void AddEventList(Type type, List<Event> events) {
            eventLists[type] = events;
        }
        public static EventListContainer LoadFromSerial() {
            throw new NotImplementedException();
        }
    }

    public class Event {
        //The function takes the source node which triggered the event, the Graph it's contained in, and the top-level graph. It returns a bool if the requirement is fulfilled.
        // List<Func<Node, Graph, Graph, double>> reqPossible;
        // List<Func<Node, Graph, Graph, bool>> reqGuaranteed;
        // List<Action<Node, Graph, Graph>> outcome;
        public List<Func<Node, Graph, Graph, double>> ReqPossible {
            get; private set;
        } = new List<Func<Node, Graph, Graph, double>>();
        public List<Func<Node, Graph, Graph, bool>> ReqGuaranteed {
            get; private set;
        } = new List<Func<Node, Graph, Graph, bool>>();
        public List<Action<Node, Graph, Graph>> Outcome {
            get; private set;
        } = new List<Action<Node, Graph, Graph>>();

        public HashSet<string> RequiredStatuses { get; set; }
        public HashSet<string> NegRequiredStatuses  { get; set; }
        public HashSet<string> AppliesStatus { get; set; }

        public void AddReqPossible(Func<Node, Graph, Graph, double> req) {
            ReqPossible.Add(req);
        }
        public void AddReqGuaranteed(Func<Node, Graph, Graph, bool> req) {
            ReqGuaranteed.Add(req);
        }
        public void AddOutcome(Action<Node, Graph, Graph> ev) {
            Outcome.Add(ev);
        }

        public Event() { }

        public Event(Func<Node, Graph, Graph, double> reqPossible,
                     Func<Node, Graph, Graph, bool> reqGuaranteed,
                     Action<Node, Graph, Graph> outcome) {
            if (reqPossible != null) ReqPossible.Add(reqPossible);
            if (reqGuaranteed != null) ReqGuaranteed.Add(reqGuaranteed);
            Outcome.Add(outcome);
        }
    }

    public class GuaranteedEvent : Event {
        public GuaranteedEvent(Action<Node, Graph, Graph> outcome) : base((n, l, w) => 1, (n, l, w) => true, outcome) { }
    }
}
