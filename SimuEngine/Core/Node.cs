using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;

namespace Core {
    public abstract class Node {
        protected Graph subGraph;
        protected Dictionary<string, int> traits;
        protected List<string> statuses;
        protected List<Group> groups;
        protected List<Connection> connections;

        public Graph Graph
        {
            get { return subGraph; }
            set { }
        }

        public Node() {
            subGraph = new Graph();
            statuses = new List<string>();
            traits = new Dictionary<string, int>();
            groups = new List<Group>();
            connections = new List<Connection>();
        }

        public abstract void OnGenerate();

        public abstract void OnCreate();
    }

}
