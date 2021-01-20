using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Core {
    [Serializable]
    public abstract class Connection {
        //Internal fields, see Node
        public List<string> statuses;
        public Dictionary<string, int> traits;

        //External properties, see Node.
        public Dictionary<string, int> Traits {
            get => traits ??= new Dictionary<string, int>();
        }
        public List<string> Statuses {
            get => statuses ??= new List<string>();
        }

        public Connection() {
            statuses = null;
            traits = null;
        }

        public virtual float Strength() => 0.0f;

        public virtual void SetName(string name) { }
    }
}
