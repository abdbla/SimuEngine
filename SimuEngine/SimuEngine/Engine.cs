using System;
using EngineCore;

namespace SimuEngine
{
    class Engine
    {
        GraphSystem system;
        Handler handler;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Graph graph1 = new Graph();
            Graph graph2 = new Graph();
            Event ev = new Event("forall(this + neighbors; thing1 = 10)",
                "forall(neighbors; thing1 = 10)", (n, g1, g2) => { return; });

            var node1 = new ExampleNode();
            var node2 = new ExampleNode();
            var node3 = new ExampleNode();

            graph1.Add(node1);
            graph1.Add(node2);
            graph1.Add(node3);
            graph1.AddConnection(node1, node2, new ExampleConnection());
            graph1.AddConnection(node1, node3, new ExampleConnection());
            graph1.AddConnection(node2, node1, new ExampleConnection());
            graph1.AddConnection(node2, node3, new ExampleConnection());

            node1.traits["thing1"] = 20;
            node2.traits["thing1"] = 10;
            node3.traits["thing1"] = 10;

            Console.WriteLine(ev.ReqPossible(node1, graph1, graph2));
            Console.WriteLine(ev.ReqGuaranteed(node1, graph1, graph2));
            //Graph graph = new Graph();
            //var node1 = new ExampleNode("a");
            //var node2 = new ExampleNode("b");
            //var node3 = new ExampleNode("c");
            //var conn = new Connection();

            //graph.Add(node1);
            //graph.Add(node2);
            //graph.Add(node3);
            //graph.AddConnection(node1, node2, conn);
            //var conns = graph.GetConnections(node1);

            //Console.WriteLine(graph.GetConnections(node1));
        }
    }

    public class ExampleNode : Node, System.IEquatable<ExampleNode>
    {
        static char ID = 'a';
        public string name;
        public ExampleNode()
        {
            name = ID++.ToString();

            if (name == "a") {
                traits["thing1"] = 20;
                traits["thing2"] = 10;
            } else if (name == "b") {
                traits["thing1"] = 10;
                traits["thing2"] = 20;
            }
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

    public class ExampleConnection : Connection, IEquatable<ExampleConnection>
    {
        public override bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType()) return false;
            else return this.Equals(obj as ExampleConnection);
        }

        public bool Equals(ExampleConnection other)
        {
            return !ReferenceEquals(other, null);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public static bool operator ==(ExampleConnection lhs, ExampleConnection rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ExampleConnection lhs, ExampleConnection rhs)
        {
            return !(lhs == rhs);
        }
    }
}