using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SimuEngine {
    public class Engine {
        public GraphSystem system;
        public Handler handler;
        public PlayerObject player;

        /// <summary>
        /// Where the application starts the engine.
        /// </summary>
        /// <param name="actions">The actions that the PlayerObject can take.</param>
        /// <param name="eventListContainer">The events that different nodes can experience.</param>
        public Engine(List<Event> actions, EventListContainer eventListContainer) {
            system = new GraphSystem();
            handler = new Handler(eventListContainer);
            player = new PlayerObject(system.graph, system.graph, actions);
        }

        [STAThread]
        static void Main() {
            //Graph graph = new Graph();
            //var node1 = new ExampleNode("a");
            //var node2 = new ExampleNode("b");
            //var node3 = new ExampleNode("c");
            //var conn = new Connection();

            //graph.Add(node1);
            //graph.Add(node2);
            //graph.Add(node3);
            //graph.AddConnection(node1, node2, conn);
            //var conns = graph.GetConnections(node1);

            //Console.WriteLine(graph.GetConnections(node1));
        }
    }
}