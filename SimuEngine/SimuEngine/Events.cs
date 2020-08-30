using System;
using System.Collections.Generic;
using System.Text;

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
        List<Func<Node, Graph, Graph, bool>> posRequirement;
        List<Func<Node, Graph, Graph, bool>> reqRequirement;
        List<Func<Node, Graph, Graph, bool>> outcome;

        public List<Func<Node, Graph, Graph, bool>> PosRequirement
        {
            get { return posRequirement; }
            set { }
        }
        public List<Func<Node, Graph, Graph, bool>> ReqRequirement
        {
            get { return reqRequirement; }
            set { }
        }
        public List<Func<Node, Graph, Graph, bool>> Outcome
        {
            get { return outcome; }
            set { }
        }

        public Event()
        {
            posRequirement = new List<Func<Node, Graph, Graph, bool>>();
            reqRequirement = new List<Func<Node, Graph, Graph, bool>>();
            outcome = new List<Func<Node, Graph, Graph, bool>>();
        }
    }
}
