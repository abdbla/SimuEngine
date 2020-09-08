using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;

namespace Core {
    public abstract class Node {

        protected Graph subGraph { get; }
        protected Dictionary<string, int> traits { get; }
        protected List<string> statuses { get; }
        protected List<Group> groups { get; }
        protected List<Connection> connections { get; }
        protected string name { get; }

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
            name = "";
        }

        public abstract void OnGenerate();

        public abstract void OnCreate();
    }

}
