﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;

namespace SimuEngine
{
    public class GraphSystem
    {
        public Graph graph;

        public GraphSystem()
        {
            graph = new Graph();
        }

        public void Generate<T>() where T: Node, new()
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

    public abstract class Group
    {
    }

    public class Connection
    {
    }
}
