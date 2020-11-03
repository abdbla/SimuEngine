using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Core {
    public abstract class Connection {
        //Internal fields, see Node
        public List<string> statuses;
        public Dictionary<string, int> traits;

        //External properties, see Node.
        public ReadOnlyDictionary<string, int> Traits {
            get => new ReadOnlyDictionary<string, int>(traits);
        }
        public ReadOnlyCollection<string> Statuses {
            get => statuses.AsReadOnly();
        }

        public Connection() {
            statuses = new List<string>();
            traits = new Dictionary<string, int>();
        }

        public virtual float Strength() => 0.0f;

        public virtual void SetName(string name) { }
    }
}
