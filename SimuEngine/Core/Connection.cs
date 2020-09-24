using System;
using System.Collections.Generic;

namespace Core {
    public abstract class Connection {
        protected List<string> statuses;
        protected Dictionary<string, int> traits;

        public virtual float Strength() => 0.0f;
    }
}
