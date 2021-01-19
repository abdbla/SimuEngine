using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using System.Runtime.Serialization;

namespace Core {
    public class NodeRandom {
        Random _instance = new Random();
        public NodeRandom() {}
        NodeRandom(Random rng) {
            _instance = rng;
        }

        public int Next(int low, int high) {
            lock (_instance) {
                return _instance.Next(low, high);
            }
        }

        public int Next(int n) {
            lock (_instance) {
                return _instance.Next(n);
            }
        }

        public double NextDouble() {
            lock (_instance) {
                return _instance.NextDouble();
            }
        }

        public static implicit operator NodeRandom(Random rng) {
            return new NodeRandom(rng);
        }
    }

    [DebuggerDisplay("Name: {Name}")]
    [Serializable]
    public abstract class Node {
        public static NodeRandom rng = new NodeRandom();

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
            statuses = new List<string>();
            traits = new Dictionary<string, int>();
            groups = new List<Group>();
            connections = new List<Connection>();
            Name = "";
        }

        public Node(Graph g, NodeCreationInfo info) : this() {
            NodeCreation(g, info);
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
