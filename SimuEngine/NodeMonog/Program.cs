using System;
using Core;
using SimuEngine;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace NodeMonog
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Graph g = new Graph();
            EventListContainer c = new EventListContainer();
            c.AddEventList(typeof(DrawNode), new List<Event>());
            Engine e = new Engine(new List<Event>(), c);

            Random r = new Random();


           /* DrawNode testNode =  new DrawNode(new Vector2(r.Next(0, 64)));
            DrawNode testNode2 = new DrawNode(new Vector2(72 + r.Next(0, 64), r.Next(0, 128)));
            DrawNode testNode3 = new DrawNode(new Vector2(156 + r.Next(0, 64), r.Next(0, 128)));
            DrawNode testNode4 = new DrawNode(new Vector2(256 + r.Next(0, 64), r.Next(0, 128)));
            DrawNode testNode5 = new DrawNode(new Vector2(326 + r.Next(0, 64), r.Next(0, 128)));


            testNode.TraitsWorkaround.Add("Age", 500);
            testNode.TraitsWorkaround.Add("Corona", 200);

            testNode2.TraitsWorkaround.Add("Age", 100);
            testNode2.TraitsWorkaround.Add("Corona", 300);


            testNode3.TraitsWorkaround.Add("Age", 10);
            testNode3.TraitsWorkaround.Add("Corona", 200);


            testNode4.TraitsWorkaround.Add("Age", 10);
            testNode4.TraitsWorkaround.Add("Corona", 200);


            testNode5.TraitsWorkaround.Add("Age", 10);
            testNode5.TraitsWorkaround.Add("Corona", 200);


            g.Add(testNode);
            g.Add(testNode2);
            g.Add(testNode3);
            g.Add(testNode4);
            g.Add(testNode5);

            testNode.NName = "billy";
            testNode2.NName = "Steve";
            testNode3.NName = "Felix";
            testNode4.NName = "Felix But good";
            testNode5.NName = "Felix 2";

            //Doesn't work btw                                          
            g.AddConnection(testNode, testNode2, new ShittyAssKnect(2000, 1000));
            g.AddConnection(testNode, testNode3, new ShittyAssKnect(2000, 1000));
            g.AddConnection(testNode, testNode5, new ShittyAssKnect(2000, 1000));
            g.AddConnection(testNode, testNode4, new ShittyAssKnect(1000, 500));
            g.AddConnection(testNode2, testNode3, new ShittyAssKnect(2000, 1000));
            g.AddConnection(testNode2, testNode, new ShittyAssKnect(2000, 1000));
            g.AddConnection(testNode4, testNode3, new ShittyAssKnect(2000, 1000));
            g.AddConnection(testNode4, testNode2, new ShittyAssKnect(2000, 1000));
            g.AddConnection(testNode4, testNode, new ShittyAssKnect(2000, 1000));

            List<Node> more = new List<Node>();
            for (int i = 0; i < 150; i++)
            {
                var n = new DrawNode(NodeCreationInfo.Empty);
                n.Name = i.ToString();
                more.Add(n);
                g.Add(n);
            }

            Dictionary<DrawNode, int> totalConns = new Dictionary<DrawNode, int>();
            Dictionary<DrawNode, int> curConns = new Dictionary<DrawNode, int>();
            List<DrawNode> remaining = new List<DrawNode>();
            remaining.AddRange(more);

            var rng = new Random();
            for (int i = 0; i < more.Count; i++)
            {
                totalConns[more[i]] = rng.Next(2, 10);
                curConns[more[i]] = 0;
            }

            while (remaining.Count > 1)
            {
                var x1 = rng.Next(remaining.Count);
                var x2 = rng.Next(remaining.Count);
                if (x1 == x2)
                {
                    continue;
                }
                var node1 = remaining[x1];
                var node2 = remaining[x2];
                var strength = rng.Next(100, 400);
                g.AddConnection(node1, node2, new ShittyAssKnect(strength, 500));
                g.AddConnection(node2, node1, new ShittyAssKnect(strength, 500));
                curConns[node1] += 1;
                curConns[node2] += 1;
                if (curConns[node1] == totalConns[node1])
                {
                    if (x1 < x2)
                    {
                        x2 -= 1;
                    }
                    remaining.RemoveAt(x1);
                }
                if (curConns[node2] == totalConns[node2])
                {
                    remaining.RemoveAt(x2);
                }
            }


            using (var game = new Renderer(g,e))
                game.Run();*/
        }
    }
#endif
}
