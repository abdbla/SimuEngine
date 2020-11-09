using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


using Core;

namespace SimuEngine {
    public class GraphSystem {
        public Graph graph;

        public GraphSystem() {
            graph = new Graph();
        }

        public GraphSystem(Graph graph) {
            this.graph = graph;
        }

        public void Create<T>(NodeCreationInfo info) where T : Node, new() {
            T node = new T();
            graph.Add(node);
            node.NodeCreation(graph, info);
        }

        public void Serialize(string fileName) {
            using (FileStream fs = File.Create(fileName)) {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(fs, graph);
            }
        }

        public static GraphSystem Deserialize(string fileName) {
            using (FileStream fs = File.OpenRead(fileName)) {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                return new GraphSystem((Graph)binaryFormatter.Deserialize(fs));
            }
        }
    }
}
