using System;
using System.Collections.Generic;

namespace Core
{
    public abstract class Connection
    {
        protected List<string> statuses;
        protected Dictionary<string, int> traits;
    }
}
