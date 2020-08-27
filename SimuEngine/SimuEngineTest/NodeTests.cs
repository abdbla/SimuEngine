using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimuEngine;
using System.Collections.Generic;

namespace SimuEngineTest
{
    [TestClass]
    public class NodeTests
    {
        [TestMethod]
        public void CreateNode_Node_Succeed()
        {
            //arrange
            GraphSystem graphSystem = new GraphSystem();

            //act
            graphSystem.Create<ExampleNode>();

            //assert
            Assert.AreEqual(1, graphSystem.graph.GetNodes());
        }

        [TestMethod]
        public void GenerateNode_Node_Succeed()
        {
            //arrange
            GraphSystem graphSystem = new GraphSystem();

            //act
            graphSystem.Generate<ExampleNode>();

            //assert
            Assert.AreEqual(1, graphSystem.graph.GetNodes());
        }

        [TestMethod]
        public void CreateConnection_Node_Succeed()
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
    }

    [TestClass]
    public class ExampleNode : Node
    {
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
    }
}
