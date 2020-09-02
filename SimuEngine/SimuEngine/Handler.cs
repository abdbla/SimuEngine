using System;
using System.Collections.Generic;
using System.Text;

using Core;

namespace SimuEngine
{
    class Handler
    {
        EventListContainer events;

        /// <summary>
        /// Method to tick every node forward one step, triggering events. Requires several calls if several top level nodes exist.
        /// </summary>
        /// <param name="graph">The Graph class created as part of the GraphSystem class. Can be passed a subGraph part of a node to partially tick forward the system.</param>
        public void Tick(Graph graph)
        {
            List<Graph> graphs = new List<Graph>();
            graphs.Add(graph);

            for (int i = 0; i < graph.Nodes.Count; i++) {
                List<Event> posEvents = new List<Event>();
                foreach (Event ev in events.GetEventList(graph.Nodes[i].GetType())) {
                    bool req = true;
                    bool pos = true;
                    for (int j = 0; j < ev.ReqGuaranteed.Count; j++) {
                        if (!ev.ReqGuaranteed[i].Invoke(graph.Nodes[i], graphs[graphs.Count - 1], graphs[0])) {
                            req = false;
                        }
                    }
                    if (req) {
                        foreach (var act in ev.Outcome) {
                            act.Invoke(graph.Nodes[i], graphs[graphs.Count - 1], graphs[0]);
                        }
                    } else {
                        for (int j = 0; j < ev.ReqGuaranteed.Count; j++) {
                            if (!ev.ReqPossible[i].Invoke(graph.Nodes[i], graphs[graphs.Count - 1], graphs[0])) {
                                pos = false;
                            }
                        }
                    }
                    if (pos) {
                        posEvents.Add(ev);
                    }
                }
                /*
                 * TODO: randomly selecting possible events to trigger
                 */

                if (graph.Nodes[i].Graph != null) {
                    Tick(graph.Nodes[i].Graph, graphs);
                }
            }
        } 
        /// <summary>
        /// Internal function to recursively iterate over several subgraphs.
        /// </summary>
        /// <param name="graph">the subgraph on which to iterate over</param>
        /// <param name="graphTree">the current graphs it has gone through</param>
        private void Tick(Graph graph, List<Graph> graphTree)
        {
            List<Graph> graphs = new List<Graph>();
            graphs.AddRange(graphTree);
            graphs.Add(graph);

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                List<Event> posEvents = new List<Event>();
                foreach (Event ev in events.GetEventList(graph.Nodes[i].GetType()))
                {
                    bool req = true;
                    bool pos = true;
                    for (int j = 0; j < ev.ReqGuaranteed.Count; j++)
                    {
                        if (!ev.ReqGuaranteed[i].Invoke(graph.Nodes[i], graphs[graphs.Count - 1], graphs[0]))
                        {
                            req = false;
                        }
                    }
                    if (req)
                    {
                        foreach (var act in ev.Outcome)
                        {
                            act.Invoke(graph.Nodes[i], graphs[graphs.Count - 1], graphs[0]);
                        }
                    }
                    else
                    {
                        for (int j = 0; j < ev.ReqGuaranteed.Count; j++)
                        {
                            if (!ev.ReqPossible[i].Invoke(graph.Nodes[i], graphs[graphs.Count - 1], graphs[0]))
                            {
                                pos = false;
                            }
                        }
                    }
                    if (pos)
                    {
                        posEvents.Add(ev);
                    }
                }
                /*
                 * TODO: randomly selecting possible events to trigger
                 */

                if (graph.Nodes[i].Graph != null)
                {
                    Tick(graph.Nodes[i].Graph, graphs);
                }
            }
        }

        public Handler()
        {
            events = new EventListContainer();
        }
    }
}
