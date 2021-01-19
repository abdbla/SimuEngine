﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

using Core;
using SimuEngine;
using NodeMonog;
using System.IO;

namespace Implementation
{
    class Program
    {


        static void Main() {
            Engine engine = InitializeEngine();
            List<(string, Event)> actions = new List<(string, Event)>();
            EventListContainer eventList = new EventListContainer();

            //TODO: Create implementation running code

            const string TEST_FILE_DIR = @"C:\Users\theodor.strom\SimuEngine\";

            if (engine.SaveExists(TEST_FILE_DIR)) {
                Console.WriteLine("Found save file");
                try {
                    engine.Load(TEST_FILE_DIR);
                    Console.WriteLine("Deserialized from save file");
                } catch (OutOfMemoryException) {
                    Console.WriteLine("Ran out of memory while deserializing");
                    throw new SystemException("Ran out of memory while deserializing");
                } catch (SerializationException ex) {
                    Console.WriteLine($"Error while deserializing ({ex}), deleting file.");
                    engine.CleanDir(TEST_FILE_DIR);
                }
            }
            if (engine.system.graph.Count.Nodes == 0) {
                Console.WriteLine("No saved system found, recreating...");

                CreateGraphSystem(); // this doesn't exist

                Console.WriteLine("Finished initialization, serializing...");
                try {
                    engine.Save(TEST_FILE_DIR);
                    Console.WriteLine("Graph initialization complete");
                } catch (IOException) {
                    Console.WriteLine("IO error while serializing, won't keep going");
                } catch (OutOfMemoryException) {
                    Console.WriteLine("Ran out of memory while serializing, no serialisation");
                }
            }

            using (Renderer renderer = new Renderer(engine)) {
                renderer.Run();
            }
        }

        static void CreateGraphSystem() {
            throw new SystemException("this isn't real");
        }

        static Engine InitializeEngine() {
            var actions = new List<(string, Event)>();
            EventListContainer eventList = new EventListContainer();
            Engine engine = new Engine(actions, eventList);

            actions.Add(("make healthy", new GuaranteedEvent(
                delegate (Node n, Graph l, Graph w) {
                    List<string> tStatus = engine.player.selectedNode.statuses;
                    tStatus.Add("Healthy");
                    tStatus.Remove("Infected");
                    tStatus.Remove("Dead");
                    tStatus.Remove("Recovered");
                }
            )));

            actions.Add(("Make infected", new GuaranteedEvent(
                delegate (Node n, Graph l, Graph w) {
                    List<string> tStatus = engine.player.selectedNode.statuses;
                    tStatus.Remove("Healthy");
                    tStatus.Add("Infected");
                    tStatus.Remove("Dead");
                    tStatus.Remove("Recovered");
                }
            )));

            actions.Add(("Make dead", new GuaranteedEvent(
                delegate (Node n, Graph l, Graph w) {
                    List<string> tStatus = engine.player.selectedNode.statuses;
                    tStatus.Remove("Healthy");
                    tStatus.Remove("Infected");
                    tStatus.Add("Dead");
                    tStatus.Remove("Recovered");
                }
            )));

            actions.Add(("Make recovered", new GuaranteedEvent(delegate (Node n, Graph l, Graph w) {
                List<string> tStatus = engine.player.selectedNode.statuses;
                tStatus.Remove("Healthy");
                tStatus.Remove("Infected");
                tStatus.Remove("Dead");
                tStatus.Add("Recovered");
            })));

            List<Event> personEvents = PersonEvents.InitializeEvents();
            eventList.AddEventList(typeof(Person), personEvents);

            return engine;
        }
    }
}
