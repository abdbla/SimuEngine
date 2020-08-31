using System;
using System.Collections.Generic;
using System.Text;

using EngineCore;

using EventParser;

namespace SimuEngine
{
    public class EventListContainer
    {
        private Dictionary<Type, List<Event>> eventLists;

        public List<Event> GetEventList(Type type)
        {
            return eventLists[type];
        }

        public static EventListContainer LoadFromSerial()
        {
            throw new NotImplementedException();
        }
    }

    public class Event
    {
        //The function takes the source node which triggered the event, the Graph it's contained in, and the top-level graph. It returns a bool if the requirement is fulfilled.
        Func<Node, Graph, Graph, bool> reqPossible;
        Func<Node, Graph, Graph, bool> reqGuaranteed;
        Action<Node, Graph, Graph> outcome;

        public Event(string possible, string guaranteed, Action<Node, Graph, Graph> outcome)
        {
            reqPossible = new EventParser.Parser(possible).toFunction();
            reqGuaranteed = new EventParser.Parser(guaranteed).toFunction();

            this.outcome = outcome;
        }

        public Func<Node, Graph, Graph, bool> ReqPossible
        {
            get { return reqPossible; }
            set { }
        }
        public Func<Node, Graph, Graph, bool> ReqGuaranteed
        {
            get { return reqGuaranteed; }
            set { }
        }
        public Action<Node, Graph, Graph> Outcome
        {
            get { return outcome; }
            set { }
        }

        public Event()
        {
            reqPossible = null;
            reqGuaranteed = null;
            outcome = (n, g1, g2) => { return; };
        }
    }
}
