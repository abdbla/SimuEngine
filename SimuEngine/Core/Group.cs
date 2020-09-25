﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Core {
    public abstract class Group
    {
        protected List<string> statuses;
        protected Dictionary<string, int> traits;
        protected List<Node> members;

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
