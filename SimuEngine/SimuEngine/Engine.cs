using System;


namespace SimuEngine
{
    class Engine
    {
        GraphSystem system;
        Handler handler;
        PlayerObject player;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        

        public Engine()
        {
            EventListContainer events = new EventListContainer();
            system = new GraphSystem();
            handler = new Handler(events);
            player = new PlayerObject(system.graph, system.graph);
        }
        [STAThread]
        static void Main()
        {
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