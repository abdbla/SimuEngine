using System;
using Core;
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
            Random r = new Random();


            ShittyAssNode testNode =  new ShittyAssNode(new Vector2(r.Next(0, 64)));
            ShittyAssNode testNode2 = new ShittyAssNode(new Vector2(72 + r.Next(0, 64), r.Next(0, 128)));
            ShittyAssNode testNode3 = new ShittyAssNode(new Vector2(156 + r.Next(0, 64), r.Next(0, 128)));
            ShittyAssNode testNode4 = new ShittyAssNode(new Vector2(256 + r.Next(0, 64), r.Next(0, 128)));
            ShittyAssNode testNode5 = new ShittyAssNode(new Vector2(326 + r.Next(0, 64), r.Next(0, 128)));


            testNode.traits.Add("Age", 500);
            testNode.traits.Add("Corona", 200);

            testNode2.traits.Add("Age", 100);
            testNode2.traits.Add("Corona", 300);


            testNode3.traits.Add("Age", 10);
            testNode3.traits.Add("Corona", 200);


            testNode4.traits.Add("Age", 10);
            testNode4.traits.Add("Corona", 200);


            testNode5.traits.Add("Age", 10);
            testNode5.traits.Add("Corona", 200);


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

            List<ShittyAssNode> more = new List<ShittyAssNode>();
            for (int i = 0; i < 500; i++)
            {
                var n = new ShittyAssNode();
                n.NName = i.ToString();
                more.Add(n);
                g.Add(n);
            }

            Dictionary<ShittyAssNode, int> totalConns = new Dictionary<ShittyAssNode, int>();
            Dictionary<ShittyAssNode, int> curConns = new Dictionary<ShittyAssNode, int>();
            List<ShittyAssNode> remaining = new List<ShittyAssNode>();
            remaining.AddRange(more);

            var rng = new Random();
            for (int i = 0; i < more.Count; i++)
            {
                totalConns[more[i]] = rng.Next(2, 4);
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


            using (var game = new Renderer(g))
                game.Run();
        }
    }
#endif
}
