using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SimuEngine {
    /// <summary>
    /// A Graph class implemented using a list of Nodes and an adjacency matrix
    /// </summary>
    public class Graph
    {
        private int currentIndex;
        private List<Node> _nodes;
        public ReadOnlyCollection<Node> Nodes => _nodes.AsReadOnly();
        Dictionary<(int, int), Connection> adjacencyMatrix;
        Dictionary<Node, int> indexLookup;

        public Graph() {
            _nodes = new List<Node>();
            indexLookup = new Dictionary<Node, int>();
            adjacencyMatrix = new Dictionary<(int, int), Connection>();
            currentIndex = 0;
        }

        /// <summary>
        /// Add a node into the graph
        /// </summary>
        /// <param name="node">the node to be added</param>
        public void Add(Node node) {
            _nodes.Add(node);
            indexLookup.Add(node, currentIndex++);
        }

        /// <summary>
        /// Removes a node and all connected edges
        /// WARNING: this is a very expensive operation, please avoid if possible.
        /// It involves rebuilding practically the entire adjacency matrix from scratch
        /// </summary>
        /// <param name="node"></param>
        /// <returns>
        /// true if the node was found and removed, false if it doesn't exist.
        /// </returns>
        public bool RemoveNode(Node node) {
            try {
                int index = FindIndex(node);
                foreach (var key in adjacencyMatrix.Keys) {
                    if (key.Item1 == index || key.Item2 == index) {
                        adjacencyMatrix.Remove(key);
                    }
                }
                _nodes.RemoveAt(index);
                indexLookup.Remove(node);
                return true;
            } catch (NodeNotFoundException) {
                return false;
            }
        }

        /// <summary>
        /// Remove the connection src->dst
        /// </summary>
        /// <param name="src">the source node</param>
        /// <param name="dst">the destination node</param>
        /// <returns>
        /// true if the connection was found and removed, false if the 
        /// connection didn't exist to begin with
        /// </returns>
        public bool RemoveConnection(Node src, Node dst) {
            var index = FindSrcDstIndex(src, dst);

            return adjacencyMatrix.Remove(index);
        }

        /// <summary>
        /// Find the first node in the node list based on the predicate
        /// </summary>
        /// <param name="predicate">the predicate to test with</param>
        /// <returns>the node found</returns>
        public Node FindNode(Predicate<Node> predicate) {
            return _nodes.Find(predicate);
        }

        /// <summary>
        /// FindNode but generic, will only filter through the nodes of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns>the first matching node of type T, or null</returns>
        public T FindNode<T>(Predicate<T> predicate) where T : Node
        {
            return _nodes.Find(node => node is T && predicate(node as T)) as T;
        }

        /// <summary>
        /// Find all nodes matching a predicate
        /// </summary>
        /// <param name="predicate">the predicate to test with</param>
        /// <returns>the list of nodes matching predicate</returns>
        public List<Node> FindAllNodes(Predicate<Node> predicate) {
            return _nodes.FindAll(predicate);
        }

        /// <summary>
        /// Generic version of FindAllNodes, will return all nodes of type T matching the predicate
        /// </summary>
        /// <typeparam name="T">the type of node to filter and return</typeparam>
        /// <param name="predicate"></param>
        /// <returns>all matching nodes of type T</returns>
        public List<T> FindAllNodes<T>(Predicate<T> predicate) where T : Node
        {
            return _nodes.FindAll(node => node is T && predicate(node as T)).Cast<T>().ToList();
        }

        // find the index of a certain node
        private int FindIndex(Node node) {
            int index; 
            bool success = indexLookup.TryGetValue(node, out index);
            if (!success) {
                throw new NodeNotFoundException($"Couldn't find node `{node}` in the graph");
            }
            return index;
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
        /// Check that there are no duplicate connections
        /// WARNING: this is a relatively expensive operation (O(n^2) with n = # connections)
        /// </summary>
        /// <returns>true if duplicate connections exist</returns>
        public bool SanityCheckConnections() {
            List<Connection> connections = adjacencyMatrix.Values.ToList();
            
            for (int i = 0; i < connections.Count - 1; i++) {
                for (int j = i + 1; j < connections.Count; j++) {
                    if (ReferenceEquals(connections[i], connections[j])) {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Find all duplicate connections so they can presumably be removed or something
        /// WARNING: this is even more expensive than SanityCheckConnections
        /// </summary>
        /// <returns>(connection, List<(source, target)>)</returns>
        public List<(Connection, List<(Node, Node)>)> FindDuplicateConnections() {
            var connections = (from kv in adjacencyMatrix
                               let src = _nodes[kv.Key.Item1]
                               let dst = _nodes[kv.Key.Item2]
                               let conn = kv.Value
                               select new {
                                   src,
                                   dst,
                                   conn
                               }).ToList();
            List<(Connection, List<(Node, Node)>)> result = new List<(Connection, List<(Node, Node)>)>();

            // This loop is fucking yikes
            for (int i = 0; i < connections.Count - 1; i++) {
                // check if we've already done this series of duplicates
                bool prematureContinue = false;
                foreach (var k in result) {
                    if (ReferenceEquals(connections[i].conn, k.Item1)) {
                        prematureContinue = true;
                    }
                }
                if (prematureContinue) continue;

                // start of null as a somewhat premature optimisation
                List<(Node, Node)> currentList = null;
                for (int j = i + 1; j < connections.Count; j++) {
                    // we've found two connections that have the same conn instance
                    if (ReferenceEquals(connections[i].conn, connections[j].conn)) {
                        // if currentList wasn't initialised, it gets initialised
                        // I think this is pretty cool tbh
                        currentList ??= new List<(Node, Node)>() {
                            (connections[i].src, connections[i].dst)
                        };
                        currentList.Add((connections[j].src, connections[j].dst));
                    }
                }

                if (currentList != null) {
                    result.Add((connections[i].conn, currentList));
                }
            }

            return result;
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
                    ret.Add((item.Value, _nodes[item.Key.Item2]));
                }
            }

            return ret;
        }

        /// <summary>
        /// Get the list of all nodes
        /// </summary>
        /// <returns>the list of all nodes</returns>
        public List<Node> GetNodes() {
            return _nodes;
        }

        public GraphCount Count {
            get {
                return new GraphCount (_nodes.Count, adjacencyMatrix.Count);
            }
        }
    }

    public readonly struct GraphCount {
        public readonly int Nodes;
        public readonly int Connections;

        public GraphCount(int nodes, int connections) {
            Nodes = nodes;
            Connections = connections;
        }
    }

    public class NodeNotFoundException : Exception {
        public NodeNotFoundException(string message) : base(message) {
        }
    }
}
