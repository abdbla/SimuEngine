using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimuEngine;

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
            Node node = new Node();

            //act
            graphSystem.Create<ExampleNode>();

            //assert
            Assert.AreEqual(1, graphSystem.graph.Count);
        }
    }
}
