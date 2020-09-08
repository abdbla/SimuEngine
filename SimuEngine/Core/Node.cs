using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;

namespace Core {
    public abstract class Node {

        public Graph subGraph;
        public Dictionary<string, int> traits;
        public List<string> statuses;
        public List<Group> groups;
        public List<Connection> connections;
        public string name = "";


        public Graph Graph
        {
            get { return subGraph; }
            set { }
        }

  //     public string Name{
  //         get { return name; }
  //         set { name = value; }
  // }

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
