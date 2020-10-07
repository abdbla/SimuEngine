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

            for (int i = 0; i < graph.Nodes.Count; i++) {
                List<Event> posEvents = new List<Event>();
                Random rng = Node.rng;
                foreach (Event ev in events.GetEventList(graph.Nodes[i].GetType())) {
                    bool req = true;
                    bool pos = true;
                    for (int j = 0; j < ev.ReqGuaranteed.Count; j++) { // Check each requirement
                        if (!ev.ReqGuaranteed[j](graph.Nodes[i], localGraph, worldGraph)) {
                            req = false; //if any are false, dont fire the event.
                        }
                    }
                    if (ev.ReqGuaranteed.Count == 0) {
                        req = false; //dont fire if it has no guaranteed requirements.
                    }
                    if (req) {
                        foreach (var act in ev.Outcome) {
                            stack.Add((graph.Nodes[i], act));
                        }
                    } else {
                        for (int j = 0; j < ev.ReqPossible.Count; j++) {
                            if (!(rng.NextDouble() <= ev.ReqPossible[j](graph.Nodes[i], localGraph, worldGraph))) {
                                pos = false; //same over here as before, though the possible req returns a modifier on the chance to fire the event
                            }
                        }
                        if (ev.ReqPossible.Count == 0) {
                            pos = false; //same as before
                        }
                    }
                    if (pos) {
                        foreach (var act in ev.Outcome) {
                            stack.Add((graph.Nodes[i], act));
                        }
                    }
                }

                if (graph.Nodes[i].SubGraph != null) {
                    Tick(graph.Nodes[i].SubGraph, graphs);
                }
            }

            foreach (var item in stack) {
                item.Item1.InvokeAction(item.Item2, localGraph, worldGraph);
            }
        }

        public Handler(EventListContainer _eventList)
        {
            events = _eventList;
        }
    }
}
