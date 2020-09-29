using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Core {
    public abstract class Node {
        //Internal fields, only meant to be accessed by internal functions, such as Events and the PlayerObject.
        public Dictionary<string, int> traits;
        public List<string> statuses;
        public List<Group> groups;
        public List<Connection> connections;

        //External properties, which can be accessed by any object or function.
        public string Name { get; set; }
        public Graph SubGraph { get; set; }
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

        /// <summary>
        /// The constructor for node. By default does nothing with NodeCreationInfo, but has the possibility
        /// to do so for implementations of node, if they desire to change depending on said info.
        /// Not an actual constructor as generic functions complain about constructors with parameters.
        /// </summary>
        /// <param name="info">The enum for whether it should create an empty node or pre-generate as part of the system</param>
        public Node() {
            SubGraph = new Graph();
            statuses = new List<string>();
            traits = new Dictionary<string, int>();
            groups = new List<Group>();
            connections = new List<Connection>();
            Name = "";
        }
        public abstract void NodeCreation(Graph g, NodeCreationInfo info = NodeCreationInfo.Empty);
        public void InvokeAction(Action<Node, Graph, Graph> action, Graph localGraph, Graph worldGraph) {
            action(this, localGraph, worldGraph);
        }
    }

    public enum NodeCreationInfo
    {
        Empty,
        SystemStart,
        SystemRunning,
    }
}
