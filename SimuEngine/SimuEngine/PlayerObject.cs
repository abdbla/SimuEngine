using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Core;

namespace SimuEngine
{
    [Serializable]
    public class PlayerObject : Node
    {
        List<(string, Event)> actions;
        Graph localGraph;
        Graph worldGraph;
        public Node selectedNode { get; private set; }
        public void ActivateAction(Event ev) {
            foreach ((string, Event) act in actions) {
                if (act.Item2 == ev) {
                    foreach (Action<Node, Graph, Graph> action in act.Item2.Outcome) {
                        action(this, localGraph, worldGraph);
                    }
                }
            }
        }

        public static implicit operator PartialPlayerObject(PlayerObject player) {
            return new PartialPlayerObject(player.localGraph, player.worldGraph, player.selectedNode);
        }

        public void Serialize(string filepath) {
            ((PartialPlayerObject)this).Serialize(filepath);
        }

        public static PartialPlayerObject Deserialize(string filepath) => PartialPlayerObject.Deserialize(filepath);
        public static PlayerObject Deserialize(string filepath, List<(string, Event)> actions)
            => new PlayerObject(PartialPlayerObject.Deserialize(filepath), actions);

        public void SelectNode(Node n) {
            selectedNode = n;
        }
        public void MoveGraph(Graph t) {
            localGraph = t;
            selectedNode = null;
        }
        public ReadOnlyCollection<Node> CurrentNodes {
            get { return localGraph.Nodes; }
        }
        public ReadOnlyCollection<Node> TopLevelNodes {
            get { return worldGraph.Nodes; }
        }
        public List<(string, Event)> Actions {
            get { return actions; }
        }
        public override void NodeCreation(Graph g, NodeCreationInfo info) {
            throw new Exception("Attempted to call NodeCreation on PlayerObject.");
        }

        /// <summary>
        /// The constructor for the PlayerObject
        /// </summary>
        /// <param name="_localGraph">The graph it's started out looking at. Usually the same as worldGraph.</param>
        /// <param name="_worldGraph">The top-level graph, belongning to the GraphSystem.</param>
        /// <param name="_actions">the list of actions the PlayerObject can take.</param>
        public PlayerObject(Graph _localGraph, Graph _worldGraph, List<(string, Event)> _actions) {
            localGraph = _localGraph;
            worldGraph = _worldGraph;
            actions = _actions;
        }

        public PlayerObject(PartialPlayerObject part, List<(string, Event)> _actions) {
            localGraph = part.localGraph;
            worldGraph = part.worldGraph;
            selectedNode = part.selectedNode;
            actions = _actions;
        }
    }

    [Serializable]
    public class PartialPlayerObject {
        public Graph localGraph, worldGraph;
        public Node selectedNode;

        public PartialPlayerObject(Graph lg, Graph wg, Node sn) {
            localGraph = lg;
            worldGraph = wg;
            selectedNode = sn;
        }

        public void Serialize(string filepath) {
            using (FileStream fs = File.OpenWrite(filepath)) {
                var bf = new BinaryFormatter();
                bf.Serialize(fs, this);
            }
        }

        public static PartialPlayerObject Deserialize(string filepath) {
            using (FileStream fs = File.OpenRead(filepath)) {
                var bf = new BinaryFormatter();
                return (PartialPlayerObject)bf.Deserialize(fs);
            }
        }
    }
}
