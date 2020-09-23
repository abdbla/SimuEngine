using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Core {
    public abstract class Node {
        public Graph subGraph { get; protected set; }
        public Dictionary<string, int> traits;
        public ReadOnlyDictionary<string, int> Traits {
            get => new ReadOnlyDictionary<string, int>(traits);
        }
        public List<string> statuses { get; protected set; }
        public List<Group> groups { get; protected set; }
        public List<Connection> connections { get; protected set; }
        public string name { get; protected set; }

        public Node() {
            subGraph = new Graph();
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
