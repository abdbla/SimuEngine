﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SimuEngine {
    public abstract class Node {
        public abstract EventList GetEventList();

        List<Node> subGraph;
        List<string> statuses;
        List<Group> groups;
        List<Connection> connections;

        public Node() {
            subGraph = new List<Node>();
            statuses = new List<string>();
            groups = new List<Group>();
            connections = new List<Connection>();
        }

        public abstract void Update();

        public abstract void OnGenerate();

        public abstract void OnCreate();
    }
}
