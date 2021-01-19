using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using SharpDX.MediaFoundation;

namespace SimuEngine {
    public class Engine {
        static string PLAYER_OBJECT_PATH = "player.obj";
        static string GRAPH_SYSTEM_PATH = "graph.obj";

        public GraphSystem system;
        public Handler handler;
        public PlayerObject player;

        /// <summary>
        /// Where the application starts the engine.
        /// </summary>
        /// <param name="actions">The actions that the PlayerObject can take.</param>
        /// <param name="eventListContainer">The events that different nodes can experience.</param>
        public Engine(List<(string, Event)> actions, EventListContainer eventListContainer) {
            system = new GraphSystem();
            handler = new Handler(eventListContainer);
            player = new PlayerObject(system.graph, system.graph, actions);
        }

        public void Save(string directoryPath) {
            string dir = Path.GetDirectoryName(directoryPath);

            string playerPath = Path.Combine(dir, PLAYER_OBJECT_PATH);
            string graphPath = Path.Combine(dir, GRAPH_SYSTEM_PATH);

            system.Serialize(graphPath);
            player.Serialize(playerPath);
        }

        string PlayerPath(string dir) {
            return Path.Combine(dir, PLAYER_OBJECT_PATH);
        }

        string GraphPath(string dir) {
            return Path.Combine(dir, GRAPH_SYSTEM_PATH);
        }

        public bool SaveExists(string directoryPath) {
            return File.Exists(PlayerPath(directoryPath)) && File.Exists(GraphPath(directoryPath));
        }

        public void CleanDir(string directoryPath) {
            var gp = GraphPath(directoryPath);
            var pp = PlayerPath(directoryPath);

            if (File.Exists(gp)) File.Delete(gp);
            if (File.Exists(pp)) File.Delete(pp);
        }

        public void Load(string directoryPath, List<(string, Event)> actions = null) {
            string dir = Path.GetDirectoryName(directoryPath);
            string playerPath = PlayerPath(dir);
            string graphPath = GraphPath(dir);

            system = GraphSystem.Deserialize(graphPath);
            player = PlayerObject.Deserialize(playerPath, actions ?? new List<(string, Event)>());
        }
    }
}