using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;

namespace Core {
    public abstract class Node {

        public Graph subGraph { get; protected set; }
        public Dictionary<string, int> traits { get; protected set;  }
        public List<string> statuses { get; protected set; }
        public List<Group> groups { get; protected set; }
        public List<Connection> connections { get; protected set; }
        public string name { get; set; }

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
