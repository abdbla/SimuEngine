using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Core {
    public abstract class Group
    {
        //Internal fields, see Node.
        public List<string> statuses;
        public Dictionary<string, int> traits;
        public List<Node> members;

        //External properties, see Node.
        public ReadOnlyDictionary<string, int> Traits {
            get => new ReadOnlyDictionary<string, int>(traits);
        }
        public ReadOnlyCollection<string> Statuses {
            get => statuses.AsReadOnly();
        }
        public ReadOnlyCollection<Node> Members {
            get => members.AsReadOnly();
        }

        public Group() {
            statuses = new List<string>();
            traits = new Dictionary<string, int>();
            members = new List<Node>();
        }
    }
}
