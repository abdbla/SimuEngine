using System;
using System.Collections.Generic;
using System.Text;

namespace SimuEngine
{
    public class EventList
    {
        public Dictionary<Type, List<Event>> eventLists;

        public static EventList LoadFromSerial()
        {
            //To be implemented?
            throw new NotImplementedException();
        }
    }

    public class Event
    {

    }
}
