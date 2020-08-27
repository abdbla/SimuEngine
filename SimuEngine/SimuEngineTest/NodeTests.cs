using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimuEngine;
using System;
using System.Collections.Generic;

namespace SimuEngineTest
{
    [TestClass]
    public class NodeTests
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
            Connection connection = new Connection();

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
            Assert.IsTrue(connectionList.TrueForAll(conn_node => getConnectionsList.Contains(conn_node)));
        }
    }

    [TestClass]
    public class ExampleConnection : Connection, IEquatable<ExampleConnection> {
        bool IEquatable<ExampleConnection>.Equals(ExampleConnection other) {
            return true;
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

        public override void Update() {
            throw new System.NotImplementedException();
        }

        public override void OnGenerate()
        {
            return;
        }
        public override void OnCreate()
        {
            return;
        }
        public override EventList GetEventList()
        {
            return null;
        }

        bool IEquatable<ExampleNode>.Equals(ExampleNode other) {
            return name == other.name;
        }
    }
}
