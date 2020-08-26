using System;
using System.Collections.Generic;
using System.Text;

namespace SimuEngine {
    public class Graph
    {
        List<Node> nodes;
        Dictionary<(int, int), Connection> adjacencyMatrix;

        public Graph() {
            nodes = new List<Node>();
            adjacencyMatrix = new Dictionary<(int, int), Connection>();
        }

        public void Add(Node node) {
            nodes.Add(node);
        }

        private int FindIndex(Node node) {
            return nodes.FindIndex(node_ => node == node_);
        }

        private (int, int) FindSrcDstIndex(Node src, Node dst) {
            return (FindIndex(src), FindIndex(dst));
        }

        public void AddConnection(Node src, Node target, Connection conn) {
            var srcIndex = FindIndex(src);
            var targetIndex = FindIndex(target);
            adjacencyMatrix[(srcIndex, targetIndex)] = conn;
        }

        public Connection GetDirectedConnection(Node src, Node target) {
            var index = FindSrcDstIndex(src, target);
            Connection ret;
            bool res = adjacencyMatrix.TryGetValue(index, out ret);
            return res ? ret : null;
        }
    }
}
