using System;
using System.Collections.Generic;
using System.Text;

namespace SimuEngine {
    public abstract class Node {
        Graph subGraph;
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
        }

        public abstract void OnGenerate();

        public abstract void OnCreate();
    }

}
