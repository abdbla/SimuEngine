using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimuEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;

using Core;
using System.Collections.ObjectModel;

namespace SimuEngineTest
{
    [TestClass]
    public class SystemTests
    {
        /* TODO: tests for RemoveConnection
         * TODO: tests for doing stuff with non-existent nodes 
         * TODO: test that OnCreate/OnGenerate are called successfully
         * TODO: tests for generic versions of FindNode/FindAllNodes
         */
        [TestMethod]
        public void CreateNode_ExampleNode_Succeed() {
            //arrange
            GraphSystem graphSystem = new GraphSystem();

            //act
            graphSystem.Create<ExampleNode>();

            //assert
            Assert.AreEqual(1, graphSystem.graph.Nodes.Count);
        }

        [TestMethod]
        public void GenerateNode_ExampleNode_Succeed() {
            //arrange
            GraphSystem graphSystem = new GraphSystem();

            //act
            graphSystem.Generate<ExampleNode>();

            //assert
            Assert.AreEqual(1, graphSystem.graph.Nodes.Count);
        }

        [TestMethod]
        public void CreateConnection_Connection_Succeed() {
            //arrange
            GraphSystem graphSystem = new GraphSystem();
            graphSystem.Create<ExampleNode>();
            graphSystem.Create<ExampleNode>();
            Node node1 = graphSystem.graph.Nodes[0];
            Node node2 = graphSystem.graph.Nodes[1];
            Connection connection = new ExampleConnection();

            //act
            graphSystem.graph.AddConnection(node1, node2, connection);

            //assert
            Assert.AreEqual(connection, graphSystem.graph.GetDirectedConnection(node1, node2));
        }

        [TestMethod]
        public void GetConnections_ConnectionList_Success() {
            //arrange
            GraphSystem graphSystem = new GraphSystem();
            graphSystem.Create<ExampleNode>();
            graphSystem.Create<ExampleNode>();
            graphSystem.Create<ExampleNode>();
            graphSystem.Create<ExampleNode>();
            graphSystem.Create<ExampleNode>();
            Node node1 = graphSystem.graph.Nodes[0];
            Node node2 = graphSystem.graph.Nodes[1];
            Node node3 = graphSystem.graph.Nodes[2];
            Node node4 = graphSystem.graph.Nodes[3];
            Node node5 = graphSystem.graph.Nodes[4];
            Connection connection = new ExampleConnection();
            graphSystem.graph.AddConnection(node1, node2, connection);
            graphSystem.graph.AddConnection(node1, node3, connection);
            graphSystem.graph.AddConnection(node1, node4, connection);
            graphSystem.graph.AddConnection(node1, node5, connection);


            //act
            List<(Connection, Node)> connectionList = new List<(Connection, Node)>();
            connectionList.Add((connection, node2));
            connectionList.Add((connection, node3));
            connectionList.Add((connection, node4));
            connectionList.Add((connection, node5));

            List<(Connection, Node)> getConnectionsList = graphSystem.graph.GetConnections(node1);

            //assert
            CollectionAssert.AreEquivalent(connectionList, getConnectionsList);
        }

        [TestMethod]
        public void FindNode_Predicate_Success() {
            Graph graph = new Graph();
            var n1 = new ExampleNode();
            var n2 = new ExampleNode();

            graph.Add(n1);
            graph.Add(n2);

            Assert.AreSame(n1, graph.FindNode(node => node == n1));
        }

        [TestMethod]
        public void FindNode_WrongPredicate_ReturnsNull() {
            Graph graph = new Graph();
            var n1 = new ExampleNode();
            var n2 = new ExampleNode();

            graph.Add(n1);

            Assert.IsNull(graph.FindNode(node => node == n2));
        }

        [TestMethod]
        public void FindAllNodes_Predicate_Success() {
            Graph graph = new Graph();
            var n1 = new ExampleNode();
            var n2 = new ExampleNode();
            var n3 = new ExampleNode();

            graph.Add(n1);
            graph.Add(n2);
            graph.Add(n3);

            CollectionAssert.AreEquivalent(new List<Node>() { n1, n2 },
                graph.FindAllNodes(node => node == n1 || node == n2));
            CollectionAssert.AreEquivalent(new List<Node>() { n1, n3 },
                graph.FindAllNodes(node => node == n1 || node == n3));
        }

        [TestMethod]
        public void GenericFindNode_MultipleNodeTypes_ReturnsCorrectly() {
            Graph graph = new Graph();
            ExampleNode2 a = new ExampleNode2();
            ExampleNode2 b = new ExampleNode2();

            graph.Add(new ExampleNode());
            graph.Add(a);
            graph.Add(new ExampleNode());
            graph.Add(b);
            graph.Add(new ExampleNode());

            Assert.AreEqual(b, graph.FindNode<ExampleNode2>(n => n.Name == "b"));
            Assert.AreEqual(a, graph.FindNode<ExampleNode2>(n => n.Name == "a"));
            Assert.AreNotEqual((Node)b, graph.FindNode<ExampleNode>(n => n.Name == "b"));
        }

        [TestMethod]
        public void SanityCheck_Duplicates_ReturnsTrue() {
            Graph graph = new Graph();

            var n1 = new ExampleNode();
            var n2 = new ExampleNode();
            var n3 = new ExampleNode();

            var conn = new ExampleConnection();

            graph.Add(n1);
            graph.Add(n2);
            graph.Add(n3);

            graph.AddConnection(n1, n2, conn);
            graph.AddConnection(n1, n3, conn);

            Assert.IsTrue(graph.SanityCheckConnections());
        }

        [TestMethod]
        public void SanityCheck_NoDuplicates_ReturnsFalse() {
            Graph graph = new Graph();

            var n1 = new ExampleNode();
            var n2 = new ExampleNode();
            var n3 = new ExampleNode();

            var c1 = new ExampleConnection();
            var c2 = new ExampleConnection();

            graph.Add(n1);
            graph.Add(n2);
            graph.Add(n3);

            graph.AddConnection(n1, n2, c1);
            graph.AddConnection(n1, n3, c2);

            Assert.IsFalse(graph.SanityCheckConnections());
        }

        [TestMethod]
        public void FindDuplicateConnections_MultipleConnections_CorrectCount() {
            Graph graph = new Graph();

            var n1 = new ExampleNode();
            var n2 = new ExampleNode();
            var n3 = new ExampleNode();
            var n4 = new ExampleNode();

            var conn1 = new ExampleConnection();
            var conn2 = new ExampleConnection();
            var conn3 = new ExampleConnection();

            graph.Add(n1);
            graph.Add(n2);
            graph.Add(n3);
            graph.Add(n4);

            graph.AddConnection(n1, n2, conn1);
            graph.AddConnection(n1, n3, conn1);
            graph.AddConnection(n1, n4, conn1);

            graph.AddConnection(n2, n3, conn2);
            graph.AddConnection(n3, n2, conn2);

            graph.AddConnection(n3, n4, conn3);

            var dupList = graph.FindDuplicateConnections();

            Assert.AreEqual(2, dupList.Count);
        }

        [TestMethod]
        public void FindDuplicateConnections_NoDuplicates_ReturnsEmpty() {
            Graph graph = new Graph();

            var nodes = new List<Node>();

            for (int i = 0; i < 10; i++) {
                var node = new ExampleNode();
                nodes.Add(node);
                graph.Add(node);
            }

            for (int i = 0; i < nodes.Count - 1; i++) {
                graph.AddConnection(nodes[i], nodes[i + 1], new ExampleConnection());
            }

            var emptyList = new List<(Connection, List<(Node, Node)>)>();

            CollectionAssert.AreEquivalent(emptyList,
                graph.FindDuplicateConnections());
        }

        [TestMethod]
        public void FindDuplicateConnections_MultipleDistinctDuplicates_ReturnsAll() {
            Graph graph = new Graph();

            var n1 = new ExampleNode();
            var n2 = new ExampleNode();
            var n3 = new ExampleNode();
            var n4 = new ExampleNode();

            var conn1 = new ExampleConnection();
            var conn2 = new ExampleConnection();
            var conn3 = new ExampleConnection();

            graph.Add(n1);
            graph.Add(n2);
            graph.Add(n3);
            graph.Add(n4);

            graph.AddConnection(n1, n2, conn1);
            graph.AddConnection(n1, n3, conn1);
            graph.AddConnection(n1, n4, conn1);

            graph.AddConnection(n2, n3, conn2);
            graph.AddConnection(n3, n2, conn2);

            var dupList = graph.FindDuplicateConnections();

            var conn1List = dupList.Find(tuple => ReferenceEquals(tuple.Item1, conn1)).Item2;
            var conn2List = dupList.Find(tuple => ReferenceEquals(tuple.Item1, conn2)).Item2;


            CollectionAssert.AreEquivalent(
                new List<(Node, Node)>() {
                    (n1, n2),
                    (n1, n3),
                    (n1, n4),
                },
                conn1List
            );

            CollectionAssert.AreEquivalent(
                new List<(Node, Node)>() {
                    (n2, n3),
                    (n3, n2),
                },
                conn2List
            );
        }

        [TestMethod]
        public void RemoveNode_RemoveUnconnectedNode_Success() {
            Graph graph = new Graph();
            var node = new ExampleNode();

            graph.Add(node);
            Assert.IsTrue(graph.Nodes.Count > 0, "Graph was empty after adding");
            graph.RemoveNode(node);
            Assert.IsTrue(graph.Nodes.Count == 0, "Graph was non-empty after removing a node");
        }

        [TestMethod]
        public void RemoveNode_ReturnsTrue() {
            Graph graph = new Graph();
            var node = new ExampleNode();

            graph.Add(node);
            Assert.IsTrue(graph.Nodes.Count > 0, "Graph was empty after adding");
            var result = graph.RemoveNode(node);
            Assert.IsTrue(graph.Nodes.Count == 0 && result,
                "RemoveNode return value was incorrect");
        }

        [TestMethod]
        public void RemoveNode_EmptyGraph_ReturnsFalse() {
            Graph graph = new Graph();
            var node = new ExampleNode();
            Assert.IsFalse(graph.RemoveNode(node), "RemoveNode returned true when the graph was empty");
        }

        [TestMethod]
        public void RemoveNode_NonEmptyGraphNonexistentNode_ReturnsFalse() {
            Graph graph = new Graph();
            var node1 = new ExampleNode();
            var node2 = new ExampleNode();
            graph.Add(node1);

            Assert.IsFalse(graph.RemoveNode(node2), "RemoveNode returned true when it didn't contain the node");
        }

        [TestMethod]
        public void RemoveNode_NonexistentNode_DoesNothing() {
            Graph graph = new Graph();
            var node1 = new ExampleNode();
            var node2 = new ExampleNode();

            graph.Add(node1);
            graph.RemoveNode(node2);

            Assert.IsTrue(graph.Nodes.Count > 0, "Graph is empty even though nothing was removed");
        }

        [TestMethod]
        public void RemoveNode_RemovesCorrectConnections() {
            Graph graph = new Graph();

            var n1 = new ExampleNode();
            var n2 = new ExampleNode();
            var n3 = new ExampleNode();

            graph.Add(n1);
            graph.Add(n2);
            graph.Add(n3);

            graph.AddConnection(n1, n2, new ExampleConnection());
            graph.AddConnection(n2, n1, new ExampleConnection());
            graph.AddConnection(n1, n3, new ExampleConnection());
            graph.AddConnection(n2, n3, new ExampleConnection());

            graph.RemoveNode(n1);
            Assert.AreEqual(new GraphCount(2, 1), graph.Count);
        }
    }

    [TestClass]
    public class ExampleConnection : Connection, IEquatable<ExampleConnection>
    {
        [TestMethod]
        public void ExampleConnection_IEquatable_Success() {
            ExampleConnection c1 = new ExampleConnection();
            ExampleConnection c2 = new ExampleConnection();

            Assert.AreSame(c1, c1);
            Assert.AreNotSame(c1, c2);
            Assert.IsTrue(c1 == c2);
            Assert.IsFalse(c1 != c2);
        }

        public override bool Equals(object obj) {
            if (obj.GetType() != this.GetType()) return false;
            else return this.Equals(obj as ExampleConnection);
        }

        public bool Equals(ExampleConnection other) {
            return !ReferenceEquals(other, null);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override string ToString() {
            return base.ToString();
        }

        public static bool operator ==(ExampleConnection lhs, ExampleConnection rhs) {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ExampleConnection lhs, ExampleConnection rhs) {
            return !(lhs == rhs);
        }
    }

    [TestClass]
    public class ExampleNode : Node, System.IEquatable<ExampleNode> {

        static char ID = 'a';

        public ExampleNode() {
            Name = ID++.ToString();
        }

        public override void OnGenerate() {
            return;
        }
        public override void OnCreate() {
            return;
        }

        bool IEquatable<ExampleNode>.Equals(ExampleNode other) {
            return Name == other.Name;
        }

        [TestMethod]
        public void ExampleNode_IEquatable_Success() {
            ExampleNode n1 = new ExampleNode();
            ExampleNode n2 = new ExampleNode();

            Assert.IsTrue(n1.Equals(n1));
            Assert.IsFalse(n1 == n2);
        }
    }

    [TestClass]
    public class ExampleNode2 : Node, System.IEquatable<ExampleNode2> {

        static char ID = 'a';
        public ExampleNode2() {
            Name = ID++.ToString();
        }

        public override void OnGenerate() {
            return;
        }
        public override void OnCreate() {
            return;
        }

        bool IEquatable<ExampleNode2>.Equals(ExampleNode2 other) {
            return Name == other.Name;
        }
    }
}
