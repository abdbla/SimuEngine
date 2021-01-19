using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Core;

namespace SimuEngine
{
    public class Handler
    {
        EventListContainer events;
        // TODO: merge documentation
        /// <summary>
        /// Method to tick every node forward one step, triggering events. Requires several calls if several top level nodes exist.
        /// </summary>
        /// <param name="graph">The Graph class created as part of the GraphSystem class. Can be passed a subGraph part of a node to partially tick forward the system.</param>
        /// <summary>
        /// Internal function to recursively iterate over several subgraphs.
        /// </summary>
        /// <param name="graph">the subgraph on which to iterate over</param>
        /// <param name="graphTree">the current graphs it has gone through</param>
        public void Tick(Graph graph, List<Graph> graphTree = null)
        {
            graphTree ??= new List<Graph>();
            List<Graph> graphs = new List<Graph>();
            List<(Node, Action<Node, Graph, Graph>)> stack = new List<(Node, Action<Node, Graph, Graph>)>();
            graphs.AddRange(graphTree);
            graphs.Add(graph);
            var worldGraph = graphs[0];
            var localGraph = graphs[graphs.Count - 1];

            foreach (Node n in graph.Nodes) {
                List<Event> posEvents = new List<Event>();
                NodeRandom rng = Node.rng;
                foreach (Event ev in events.GetEventList(n.GetType())) {
                    InvokeEvent(n, stack, ev, localGraph, worldGraph);
                }

                if (n.SubGraph != null) {
                    Tick(n.SubGraph, graphs);
                }
            }

            foreach (var item in stack) {
                item.Item1.InvokeAction(item.Item2, localGraph, worldGraph);
            }
        }

        void InvokeEvent(Node n, List<(Node, Action<Node, Graph, Graph>)> stack,
                         Event ev, Graph localGraph, Graph worldGraph) {
            bool? req = null;
            bool? pos = null;
            foreach (var guaranteedCheck in ev.ReqGuaranteed) { // Check each requirement
                req = (req ?? true) && guaranteedCheck(n, localGraph, worldGraph);
            }
            if (req ?? false) {
                foreach (var act in ev.Outcome) {
                    stack.Add((n, act));
                }
            } else {
                foreach (var possibleCheck in ev.ReqPossible) {
                    //same over here as before, though the possible req returns a modifier on the chance to fire the event
                    pos = (pos ?? true) && Node.rng.NextDouble() <= possibleCheck(n, localGraph, worldGraph);
                }
            }
            if (pos ?? false) {
                foreach (var act in ev.Outcome) {
                    stack.Add((n, act));
                }
            }
        }

        public Handler(EventListContainer _eventList)
        {
            events = _eventList;
        }
    }
}
