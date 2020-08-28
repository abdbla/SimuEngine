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

    }
}
