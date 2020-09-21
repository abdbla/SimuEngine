using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Core;

namespace SimuEngine
{
    class Handler
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
            graphs.AddRange(graphTree);
            graphs.Add(graph);

            for (int i = 0; i < graph.Nodes.Count; i++) {
                List<Event> posEvents = new List<Event>();
                Random rng = new Random();
                foreach (Event ev in events.GetEventList(graph.Nodes[i].GetType())) {
                    bool req = true;
                    bool pos = true;
                    var worldGraph = graphs[0];
                    var localGraph = graphs[graphs.Count - 1];
                    for (int j = 0; j < ev.ReqGuaranteed.Count; j++) {
                        if (!ev.ReqGuaranteed[i](graph.Nodes[i], localGraph, worldGraph)) {
                            req = false;
                        }
                    }
                    if (req) {
                        foreach (var act in ev.Outcome) {
                            graph.Nodes[i].InvokeAction(act, localGraph, worldGraph);
                        }
                    } else {
                        for (int j = 0; j < ev.ReqGuaranteed.Count; j++) {
                            if (!ev.ReqPossible[i](graph.Nodes[i], localGraph, worldGraph)) {
                                pos = false;
                            }
                        }
                    }
                    if (pos) {
                        if (rng.NextDouble() <= ev.Chance) {
                            foreach (var act in ev.Outcome) {
                                graph.Nodes[i].InvokeAction(act, localGraph, worldGraph);
                            }
                        }
                    }
                }

                if (graph.Nodes[i].Graph != null) {
                    Tick(graph.Nodes[i].Graph, graphs);
                }
            }
        }

        public Handler(EventListContainer _eventList)
        {
            events = _eventList;
        }
    }
}
