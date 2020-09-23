using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Core {
    public abstract class Node {
        public Dictionary<string, int> traits;
        public List<string> statuses;
        public List<Group> groups;
        public List<Connection> connections;

        public string Name { get; protected set; }
        public Graph SubGraph { get; protected set; }
        public ReadOnlyDictionary<string, int> Traits {
            get => new ReadOnlyDictionary<string, int>(traits);
        }
        public ReadOnlyCollection<string> Statuses {
            get => statuses.AsReadOnly();
        }
        public ReadOnlyCollection<Group> Groups {
            get => groups.AsReadOnly();
        }
        public ReadOnlyCollection<Connection> Connections {
            get => connections.AsReadOnly();
        }

        public Node() {
            SubGraph = new Graph();
            statuses = new List<string>();
            traits = new Dictionary<string, int>();
            groups = new List<Group>();
            connections = new List<Connection>();
        }

        public abstract void OnGenerate();

        public abstract void OnCreate();

        public void InvokeAction(Action<Node, Graph, Graph> action, Graph localGraph, Graph worldGraph) {
            action(this, localGraph, worldGraph);
        }
    }
}
