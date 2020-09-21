using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Core;

namespace SimuEngine
{
    class PlayerObject : Node
    {
        List<Event> actions;
        Graph localGraph;
        Graph worldGraph;
        public List<Event> GetEvents()
        {
            return actions;
        }
        public void ActivateAction(Event ev)
        {
            foreach (Event act in actions) {
                if (act == ev) {
                    foreach (Action<Node, Graph, Graph> action in act.Outcome) {
                        action(this, localGraph, worldGraph);
                    }
                }
            }   
        }
        public override void OnCreate()
        {
            throw new NotImplementedException();
        }
        public override void OnGenerate()
        {
            throw new Exception("Attempted to call OnGenerate on the player object.");
        }

        public PlayerObject(Graph _localGraph, Graph _worldGraph)
        {
            localGraph = _localGraph;
            worldGraph = _worldGraph;
            actions = new List<Event>(); //TODO: add implementation support
        }
    }
}
