using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Core {
    /// <summary>
    /// A Graph class implemented using a list of Nodes and an adjacency matrix
    /// </summary>
    public class Graph {
        int connId = 0;

        public IEnumerable<Connection> Connections { get => adjacencyMatrix.Values; }
        private List<Node> nodes;
        public ReadOnlyCollection<Node> Nodes => nodes.AsReadOnly();
        public List<Group> groups;
        Dictionary<(Node, Node), Connection> adjacencyMatrix;
        Dictionary<Connection, (Node, Node)> reverseLookup;
        Dictionary<Node, List<Connection>> connectionLists;

        public Graph() {
            nodes = new List<Node>();
            groups = new List<Group>();
            adjacencyMatrix = new Dictionary<(Node, Node), Connection>();
            reverseLookup = new Dictionary<Connection, (Node, Node)>();
            connectionLists = new Dictionary<Node, List<Connection>>();
        }

        /// <summary>
        /// Add a node into the graph
        /// </summary>
        /// <param name="node">the node to be added</param>
        public void Add(Node node) {
            nodes.Add(node);
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

            if (!nodes.Remove(node)) return false;

            foreach (var key in adjacencyMatrix.Keys) {
                if (key.Item1 == node || key.Item2 == node) {
                    adjacencyMatrix.Remove(key);
                }
            }

            return true;
        }

        /// <summary>
        /// Removes multiple nodes from the graph, together with the connections
        /// </summary>
        /// <param name="nodes">the nodes to remove</param>
        /// <returns>the number of nodes and connections that were removed</returns>
        public GraphCount RemoveNodes(IEnumerable<Node> nodes) {
            var set = new HashSet<Node>(nodes);
            foreach (var node in nodes) set.Add(node);

            var toRemove = new List<(Node, Node)>();

            var connectionCount = 0;

            foreach (var kv in adjacencyMatrix) {
                bool rConn = false;

                if (set.Contains(kv.Key.Item1) || set.Contains(kv.Key.Item2)) {
                    rConn = true;
                }

                if (rConn) {
                    connectionCount++;
                    toRemove.Add(kv.Key);
                }
            }

            var nodeCount = set.Count;

            this.nodes.RemoveAll(n => set.Contains(n));

            foreach (var key in toRemove) {
                adjacencyMatrix.Remove(key);
            }

            return new GraphCount(nodeCount, connectionCount);
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
            return adjacencyMatrix.Remove((src, dst));
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
        /// FindNode but generic, will only filter through the nodes of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns>the first matching node of type T, or null</returns>
        public T FindNode<T>(Predicate<T> predicate) where T : Node
        {
            return nodes.Find(node => node is T && predicate(node as T)) as T;
        }

        /// <summary>
        /// Find all nodes matching a predicate
        /// </summary>
        /// <param name="predicate">the predicate to test with</param>
        /// <returns>the list of nodes matching predicate</returns>
        public List<Node> FindAllNodes(Predicate<Node> predicate) {
            return nodes.FindAll(predicate);
        }

        /// <summary>
        /// Generic version of FindAllNodes, will return all nodes of type T matching the predicate
        /// </summary>
        /// <typeparam name="T">the type of node to filter and return</typeparam>
        /// <param name="predicate"></param>
        /// <returns>all matching nodes of type T</returns>
        public List<T> FindAllNodes<T>(Predicate<T> predicate) where T : Node
        {
            return nodes.FindAll(node => node is T && predicate(node as T)).Cast<T>().ToList();
        }

        /// <summary>
        /// Add a directed connection between two nodes
        /// </summary>
        /// <param name="src">the source node</param>
        /// <param name="target">the target node</param>
        /// <param name="conn">the type of connection to add</param>
        public void AddConnection(Node src, Node target, Connection conn) {
            if (!connectionLists.ContainsKey(src)) {
                connectionLists.Add(src, new List<Connection>());
            }

            conn.SetName(connId++.ToString());
            src.connections.Add(conn);
            connectionLists[src].Add(conn);
            adjacencyMatrix[(src, target)] = conn;
            reverseLookup[conn] = (src, target);
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
                               let src = kv.Key.Item1
                               let dst = kv.Key.Item2
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
            Connection ret;
            bool res = adjacencyMatrix.TryGetValue((src, target), out ret);
            return res ? ret : null;
        }

        public bool TryGetDirectedConnection(Node src, Node target, out Connection connection) {
            try {
                return adjacencyMatrix.TryGetValue((src, target), out connection);
            } catch (NodeNotFoundException) {
                connection = null;
                return false;
            }
        }

        /// <summary>
        /// Get all connected nodes from a certain node
        /// </summary>
        /// <param name="node">the node to get connections from</param>
        /// <returns>a list of all the outwards connections plus the nodes they go to</returns>
        public List<(Connection, Node)> GetOutgoingConnections(Node node) {
            if (!connectionLists.TryGetValue(node, out List<Connection> conns)) {
                throw new NodeNotFoundException("Tried to get outgoing connections for non-existent node");
            }
            var ret = (from conn in conns
                       select (conn, reverseLookup[conn].Item2)).ToList();

            return ret;
        }

        public List<(Connection, Connection, Node)> GetNeighbors(Node node) {
            var connectedNodes = new HashSet<Node>();
            
            foreach (var item in adjacencyMatrix) {
                if (item.Key.Item1 == node) {
                    connectedNodes.Add(item.Key.Item2);
                } else if (item.Key.Item2 == node) {
                    connectedNodes.Add(item.Key.Item1);
                }
            }

            var ret = new List<(Connection, Connection, Node)>();

            foreach (var otherNode in connectedNodes) {
                adjacencyMatrix.TryGetValue((node, otherNode), out Connection conn1);
                adjacencyMatrix.TryGetValue((otherNode, node), out Connection conn2);
                ret.Add((conn1, conn2, otherNode));
            }

            return ret;
        }

        public List<(Node, uint)> GetNeighborsDegrees(Node node, uint separation) {
            return GetNeighborsHelper(new HashSet<Node>(), node, separation, 1).ToList();
        }
        
        private List<(Node, uint)> GetNeighborsHelper(HashSet<Node> seen, Node node, uint separation, uint level) {
            seen.Add(node);

            var neighbors = new List<(Node, uint)>();
            var subNeighbors = new List<List<(Node, uint)>>();

            if (separation == 0) {
                return GetOutgoingConnections(node).Select(x => x.Item2)
                    .Where(x => !seen.Contains(x))
                    .Select(x => (x, level))
                    .ToList();
            }

            foreach ((_, Node neighbor) in GetOutgoingConnections(node)) {
                if (!seen.Contains(neighbor)) {
                    seen.Add(neighbor);
                    neighbors.Add((neighbor, level));
                }
            }

            foreach (var neighbor in neighbors.Select(x => x.Item1)) {
                subNeighbors.Add(GetNeighborsHelper(seen, neighbor, separation - 1, level + 1));
            }

            neighbors.AddRange(subNeighbors.SelectMany(x => x));

            return neighbors.ToList();
        }

        public bool Contains(Node node) {
            return nodes.Contains(node);
        }

        public GraphCount Count {
            get {
                return new GraphCount (nodes.Count, adjacencyMatrix.Count);
            }
        }
    }

    [System.Diagnostics.DebuggerDisplay("V = {Nodes}, E = {Connections}")]
    public readonly struct GraphCount {
        public readonly int Nodes;
        public readonly int Connections;
        public GraphCount(int nodes, int connections) {
            Nodes = nodes;
            Connections = connections;
        }

        public override string ToString() {
            return $"(V = {Nodes}, E = {Connections})";
        }
    }

    public class NodeNotFoundException : Exception {
        public NodeNotFoundException(string message) : base(message) {
        }
    }
}
