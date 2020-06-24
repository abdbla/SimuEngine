using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;

namespace SimuEngine
{
    public class GraphSystem
    {
        public List<Node> graph;

        public GraphSystem()
        {
            graph = new List<Node>();
        }

        public void Generate<T>() where T : Node, new()
        {
            T node = new T();
            graph.Add(node);
            node.OnGenerate();
        }

        public void Create<T>() where T : Node, new()
        {
            T node = new T();
            graph.Add(node);
            node.OnCreate();
        }
    }
    public class Node
    {
        List<Node> subGraph;
        List<string> statuses;
        NodeType type;
        List<Group> groups;
        List<Connection> connections;

        public Node()
        {
            subGraph = new List<Node>();
            statuses = new List<string>();
            groups = new List<Group>();
            connections = new List<Connection>();
        }

        public void OnGenerate()
        {
            type = NodeType.Base;
        }

        public void OnCreate()
        {
            type = NodeType.Base;
        }
    }

    public class Group
    {
        GroupType type;
    }

    public class Connection
    {
        ConnectionType type;
    }
}
