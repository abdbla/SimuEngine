using System;
using System.Collections.Generic;
using System.Text;

namespace EngineCore {
    public abstract class Node {
        Graph subGraph;
        public Dictionary<string, int> traits;
        List<string> statuses;
        List<Group> groups;
        List<Connection> connections;

        public Graph Graph
        {
            get { return subGraph; }
            set { }
        }

        public Node() {
            subGraph = new Graph();
            statuses = new List<string>();
            groups = new List<Group>();
            connections = new List<Connection>();
            traits = new Dictionary<string, int>();
        }

        public abstract void OnGenerate();

        public abstract void OnCreate();
    }
    public class ExampleNode : Node, System.IEquatable<ExampleNode>
    {
        static char ID = 'a';
        public string name;
        public ExampleNode()
        {
            name = ID++.ToString();
        }

        public override void OnGenerate()
        {
            return;
        }
        public override void OnCreate()
        {
            return;
        }

        bool IEquatable<ExampleNode>.Equals(ExampleNode other)
        {
            return name == other.name;
        }
    }
}
