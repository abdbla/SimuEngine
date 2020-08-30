using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX.Direct3D11;
using SimuEngine;
using System;
using System.Collections.Generic;

namespace SimuEngineTest
{
    [TestClass]
    public class SystemTests
    {
        [TestMethod]
        public void CreateNode_ExampleNode_Succeed()
        {
            //arrange
            GraphSystem graphSystem = new GraphSystem();

            //act
            graphSystem.Create<ExampleNode>();

            //assert
            Assert.AreEqual(1, graphSystem.graph.GetNodes().Count);
        }

        [TestMethod]
        public void GenerateNode_ExampleNode_Succeed()
        {
            //arrange
            GraphSystem graphSystem = new GraphSystem();

            //act
            graphSystem.Generate<ExampleNode>();

            //assert
            Assert.AreEqual(1, graphSystem.graph.GetNodes().Count);
        }

        [TestMethod]
        public void CreateConnection_Connection_Succeed()
        {
            //arrange
            GraphSystem graphSystem = new GraphSystem();
            graphSystem.Create<ExampleNode>();
            graphSystem.Create<ExampleNode>();
            Node node1 = graphSystem.graph.nodes[0];
            Node node2 = graphSystem.graph.nodes[1];
            Connection connection = new ExampleConnection();

            //act
            graphSystem.graph.AddConnection(node1, node2, connection);

            //assert
            Assert.AreEqual(connection, graphSystem.graph.GetDirectedConnection(node1, node2));
        }

        [TestMethod]
        public void GetConnections_ConnectionList_Success()
        {
            //arrange
            GraphSystem graphSystem = new GraphSystem();
            graphSystem.Create<ExampleNode>();
            graphSystem.Create<ExampleNode>();
            graphSystem.Create<ExampleNode>();
            graphSystem.Create<ExampleNode>();
            graphSystem.Create<ExampleNode>();
            Node node1 = graphSystem.graph.GetNodes()[0];
            Node node2 = graphSystem.graph.GetNodes()[1];
            Node node3 = graphSystem.graph.GetNodes()[2];
            Node node4 = graphSystem.graph.GetNodes()[3];
            Node node5 = graphSystem.graph.GetNodes()[4];
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
        public void SanityCheck_Success() {
            Graph graph = new Graph();

            var n1 = new ExampleNode();
            var n2 = new ExampleNode();
            var n3 = new ExampleNode();

            var conn = new ExampleConnection();

            graph.Add(n1);
            graph.Add(n2);
            graph.Add(n3);

            graph.AddConnection(n1, n2, conn);

            Assert.IsFalse(graph.SanityCheckConnections());

            graph.AddConnection(n1, n3, conn);

            Assert.IsTrue(graph.SanityCheckConnections());
        }
    }

    [TestClass]
    public class ExampleConnection : Connection, IEquatable<ExampleConnection> {
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
    public class ExampleNode : Node, System.IEquatable<ExampleNode>
    {
        static char ID = 'a';
        public string name;
        public ExampleNode() {
            name = ID++.ToString();
        }

        public override void OnGenerate()
        {
            return;
        }
        public override void OnCreate()
        {
            return;
        }

        bool IEquatable<ExampleNode>.Equals(ExampleNode other) {
            return name == other.name;
        }

        [TestMethod]
        public void ExampleNode_IEquatable_Success() {
            ExampleNode n1 = new ExampleNode();
            ExampleNode n2 = new ExampleNode();

            Assert.IsTrue(n1.Equals(n1));
            Assert.IsFalse(n1 == n2);
        }
    }
}
