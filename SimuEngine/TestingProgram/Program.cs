using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core;

namespace TestingProgram {
    class Program {
        static void Main(string[] args) {
            var g = new Graph();

            var a = new TestNode();
            var b = new TestNode();
            var c = new TestNode();
            var d = new TestNode();
            var e = new TestNode();
            var f = new TestNode();

            g.Add(a);
            g.Add(b);
            g.Add(c);
            g.Add(d);
            g.Add(e);
            g.Add(f);

            Console.WriteLine(g.Count);

            g.RemoveNodes(new[] { a, d });

            Console.WriteLine(g.Count);

            g.RemoveNode(a);

            Console.WriteLine(g.Count);
        }
    }

    class TestConn : Connection {
        public TestConn() {
            
        }

        public override float Strength() {
            return 10f;
        }
    }

    class TestNode : Node {
        static int id = 0;

        public TestNode() {
            Name = id++.ToString();
        }

        public override void NodeCreation(Graph g, NodeCreationInfo info = NodeCreationInfo.Empty) {
            throw new NotImplementedException();
        }
    }
}
