using System;
using System.Collections.Generic;
using System.Text;

using Core;

namespace SimuEngine
{
    public class EventListContainer
    {
        private Dictionary<Type, List<Event>> eventLists;

        public EventListContainer()
        {
            eventLists = new Dictionary<Type, List<Event>>();
        }
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
        List<Func<Node, Graph, Graph, double>> reqPossible;
        List<Func<Node, Graph, Graph, bool>> reqGuaranteed;
        List<Action<Node, Graph, Graph>> outcome;

        public List<Func<Node, Graph, Graph, double>> ReqPossible
        {
            get { return reqPossible; }
            set { }
        }
        public List<Func<Node, Graph, Graph, bool>> ReqGuaranteed
        {
            get { return reqGuaranteed; }
            set { }
        }
        public List<Action<Node, Graph, Graph>> Outcome
        {
            get { return outcome; }
            set { }
        }

        public Event()
        {
            reqPossible = new List<Func<Node, Graph, Graph, double>>();
            reqGuaranteed = new List<Func<Node, Graph, Graph, bool>>();
            outcome = new List<Action<Node, Graph, Graph>>();
        }
    }
}
