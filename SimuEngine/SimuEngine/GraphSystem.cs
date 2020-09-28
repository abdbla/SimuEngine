﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Core;

namespace SimuEngine {
    public class GraphSystem {
        public Graph graph;

        public GraphSystem() {
            graph = new Graph();
        }

        public void Create<T>(NodeCreationInfo info) where T : Node, new() {
            T node = new T();
            graph.Add(node);
            node.NodeCreation(graph, info);
        }

        public void Serialize(string fileName) {
            using (FileStream fs = File.Create(fileName)) {
                var json = JsonSerializer.Serialize(this);
                File.WriteAllText(fileName, json);
            }
        }

        public static GraphSystem Deserialize(string fileName) {
            using (FileStream fs = File.OpenRead(fileName)) {
                var json = File.ReadAllText(fileName);
                return JsonSerializer.Deserialize<GraphSystem>(json);
            }
        }
    }
}
