using System;
using System.Collections.Generic;

namespace SimuEngine {
    /// <summary>
    /// A Graph class implemented using a list of Nodes and an adjacency matrix
    /// </summary>
    public class Graph
    {
        public List<Node> nodes; //public for the purposes of unit tests
        Dictionary<(int, int), Connection> adjacencyMatrix;

        public Graph() {
            nodes = new List<Node>();
            adjacencyMatrix = new Dictionary<(int, int), Connection>();
        }

        /// <summary>
        /// Add a node into the graph
        /// </summary>
        /// <param name="node">the node to be added</param>
        public void Add(Node node) {
            nodes.Add(node);
        }

        /// <summary>
        /// Find the first node in the node list based on the predicate
        /// </summary>
        /// <param name="predicate">the predicate to test with</param>
        /// <returns>the node found</returns>
        public Node FindNode(Predicate<Node> predicate) {
            return nodes.Find(predicate);
        }

        /// <summary>
        /// Find all nodes matching a predicate
        /// </summary>
        /// <param name="predicate">the predicate to test with</param>
        /// <returns>the list of nodes matching predicate</returns>
        public List<Node> FindAllNodes(Predicate<Node> predicate) {
            return nodes.FindAll(predicate);
        }

        // find the index of a certain node
        private int FindIndex(Node node) {
            return nodes.FindIndex(node_ => node == node_);
        }

        // shorthand to find the indices of two nodes
        private (int, int) FindSrcDstIndex(Node src, Node dst) {
            return (FindIndex(src), FindIndex(dst));
        }

        /// <summary>
        /// Add a directed connection between two nodes
        /// </summary>
        /// <param name="src">the source node</param>
        /// <param name="target">the target node</param>
        /// <param name="conn">the type of connection to add</param>
        public void AddConnection(Node src, Node target, Connection conn) {
            var srcIndex = FindIndex(src);
            var targetIndex = FindIndex(target);
            adjacencyMatrix[(srcIndex, targetIndex)] = conn;
        }

        /// <summary>
        /// Get the directed connection src and target
        /// </summary>
        /// <param name="src">the source node</param>
        /// <param name="target">the target node</param>
        /// <returns>the connection src->target</returns>
        public Connection GetDirectedConnection(Node src, Node target) {
            var index = FindSrcDstIndex(src, target);
            Connection ret;
            bool res = adjacencyMatrix.TryGetValue(index, out ret);
            return res ? ret : null;
        }

        /// <summary>
        /// Get all connected nodes from a certain node
        /// </summary>
        /// <param name="node">the node to get connections from</param>
        /// <returns>a list of all the outwards connections plus the nodes they go to</returns>
        public List<(Connection, Node)> GetConnections(Node node) {
            var idx = FindIndex(node);
            var ret = new List<(Connection, Node)>();
            foreach (var item in adjacencyMatrix) {
                if (item.Key.Item1 == idx) {
                    ret.Add((item.Value, nodes[item.Key.Item2]));
                }
            }

            return ret;
        }

        /// <summary>
        /// Get the list of all nodes
        /// </summary>
        /// <returns>the list of all nodes</returns>
        public List<Node> GetNodes() {
            return nodes;
        }
    }
}
