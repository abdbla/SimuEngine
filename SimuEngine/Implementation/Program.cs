using System;
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

            //TODO: Create implementation running code

            string saveDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"SimuEngine");
            if (!Directory.Exists(saveDir)) {
                Directory.CreateDirectory(saveDir);
            }

            bool readFailed = false;
            bool createNew = false;

            if (engine.SaveExists(saveDir)) {
                Console.WriteLine("Found save file");
                bool useSave = YesNo("Do you wish to use the save?");

                if (useSave) {
                    try {
                        engine.Load(saveDir, engine.player.Actions);
                        Console.WriteLine("Deserialized from save file");
                    } catch (OutOfMemoryException e) {
                        Console.WriteLine("Ran out of memory while deserializing");
                        readFailed = true;
                        throw new SystemException("Ran out of memory while deserializing", e);
                    } catch (SerializationException ex) {
                        Console.WriteLine($"Error while deserializing ({ex}), deleting file.");
                        engine.CleanDir(saveDir);
                        readFailed = true;
                    }
                } else {
                    //City c = new City(350000, 100);
                    //engine.system.graph.Add(c);
                    //// c.NodeCreation(engine.system.graph);
                    createNew = true;
                }
            } else {
                Console.WriteLine("No saved system found");
            }

            if (engine.system.graph.Count.Nodes == 0 || readFailed || createNew) {
                Console.WriteLine("Creating a new system");

                City c = new City(350000, 100);
                c.Name = "Irak";
                engine.system.graph.Add(c);
                // c.NodeCreation(engine.system.graph);

                bool ser = YesNo("Initialization finished, serialize?");

                if (ser) {
                    Console.WriteLine("Serializing...");
                    try {
                        engine.Save(saveDir);
                        Console.WriteLine("Graph initialization complete");
                    } catch (IOException) {
                        Console.WriteLine("IO error while serializing, won't keep going");
                    } catch (OutOfMemoryException) {
                        Console.WriteLine("Ran out of memory while serializing, no serialisation");
                    }
                }
            }

            ExcelExport export = new ExcelExport();

            using (Renderer renderer = new Renderer(engine)) {

                renderer.OnTickFinished += export.OnTick;
                renderer.Run();
                export.Save(new FileInfo(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "excel_export.xlsx")));
            }
        }

        static void CreateGraphSystem() {
            throw new SystemException("this isn't real");
        }

        static bool YesNo(string prompt) {
            Console.Write(prompt + " (Y/N) ");
            while (true) {
                var k = Console.ReadKey();
                Console.WriteLine();
                switch (k.KeyChar) {
                    case 'Y':
                    case 'y':
                        return true;
                    case 'N':
                    case 'n':
                        return false;
                    default:
                        Console.Write("(Y)es or (N)o, please. ");
                        break;
                }
            }
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
                    Dictionary<string, int> tTrait = engine.player.selectedNode.traits;
                    tStatus.Remove("Healthy");
                    tStatus.Add("Infected");
                    tStatus.Remove("Dead");
                    tStatus.Remove("Recovered");
                    tTrait.Add("Infected Time", 0);
                    tTrait.Add("Medicinal Support", 100);
                    tTrait.Add("Viral Intensity", Node.rng.NextGaussian(100, 10));
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

            List<Event> personEvents = PersonEvents.InitializePersonEvents();
            List<Event> districtEvents = PersonEvents.InitializeDistrictEvents();
            List<Event> cityEvents = PersonEvents.InitializeCityEvents();
            eventList.AddEventList(typeof(Person), personEvents);
            eventList.AddEventList(typeof(District), districtEvents);
            eventList.AddEventList(typeof(City), cityEvents);

            return engine;
        }
    }
}
