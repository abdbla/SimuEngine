using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Core;
using Core.Physics;

namespace NodeMonog {

    class DuplicatedConnection : Connection {
        Connection inner;
        public DuplicatedConnection(Connection c) {
            inner = c;
            statuses = inner.statuses;
            traits = inner.traits;
        }

        public override float Strength() => inner.Strength();
    }

    class PhysicsWrapper {
        Simulation sim;
        Graph graph;
        Node selectedNode;

        /// <summary>
        /// The currently selected node
        /// 
        /// The set method is just sugar for calling Update()
        /// </summary>
        public Node SelectedNode {
            get => selectedNode;
            set {
                Update(value);
            }
        }

        public DrawNode SelectedDrawNode {
            get { var dn = new DrawNode(SelectedNode, sim); dn.separation = 0; return dn; }
            set => SelectedNode = value.node;
        }

        List<(Node, uint)> neighbors;

        List<DrawNode> drawNodes;
        public List<DrawNode> DrawNodes { get {
                drawNodes ??= MakeDrawNodeList();
                return drawNodes;
            }
        }

        private List<DrawNode> MakeDrawNodeList() {
            List<DrawNode> drawNodes = new List<DrawNode>() { new DrawNode(selectedNode, sim) };

            drawNodes.AddRange(neighbors.Select(x => new DrawNode(x.Item1, sim, sep: (int)x.Item2)));

            return drawNodes;
        }

        public double initialStep = 1.5;
        public Func<int, double, double> timeStepFunction = (iterCount, energy) => {
            double negLog = Math.Pow(2, -Math.Log(energy, 2));
            double timeStep = Math.Min(negLog, energy / 10);
            //timeStep = Math.Max(timeStep, 0.01f);
            timeStep = Math.Min(timeStep, 1);

            return timeStep;
        };

        TaskStatus status;
        Task simulationTask;
        CancellationTokenSource tokenSource;

        public uint Degrees { get; private set; }

        public Status Status { get => status.Status; }

        public PhysicsWrapper(Graph graph, Node selectedNode, uint degrees, SimulationParams @params) {
            this.selectedNode = selectedNode;
            var biGraph = new Graph();

            foreach (Node n in graph.Nodes) {
                biGraph.Add(n);
            }

            foreach (Node n1 in graph.Nodes) {
                foreach (Node n2 in graph.Nodes) {
                    if (n1 != n2) {
                        if (graph.TryGetDirectedConnection(n1, n2, out var n1_n2)) {
                            if (graph.TryGetDirectedConnection(n2, n1, out var n2_n1)) {
                                biGraph.AddConnection(n1, n2, n1_n2);
                                biGraph.AddConnection(n1, n2, n2_n1);
                            } else {
                                biGraph.AddConnection(n1, n2, n1_n2);
                                biGraph.AddConnection(n1, n2, new DuplicatedConnection(n1_n2));
                            }
                        }
                    }
                }
            }

            this.graph = biGraph;

            neighbors = graph.GetNeighborsDegrees(selectedNode, degrees).ToList();
            sim = new Simulation(graph, @params, neighbors
                .Select(x => x.Item1)
                .Except(new[] { selectedNode })
                .Prepend(selectedNode)
                .ToList());
            Degrees = degrees;
            
            (simulationTask, status) = CreateSimulationTask();
        }

        public void Update(Node newSelected) {
            var newNeighbors = graph.GetNeighborsDegrees(newSelected, Degrees);
            var newSet = new HashSet<Node>(newNeighbors.Select(x => x.Item1));
            var oldSet = new HashSet<Node>(neighbors.Select(x => x.Item1));

            if (newSet.Contains(selectedNode)) {
            }

            // oldSet.ExceptWith(newSet);
            // newSet.ExceptWith(oldSet);

            lock (sim) {
                sim.RemoveRange(oldSet);
                sim.AddRange(newSet);
            }

            neighbors = newNeighbors;
            selectedNode = newSelected;

            ResetAndRestart();

            drawNodes = null;
        }

        public void StartSimulation() {
            simulationTask.Start();
        }

        public float GetTotalEnergy() {
            lock (sim) {
                return sim.GetTotalEnergy();
            }
        }

        public void ResetSimulation() {
            tokenSource.Cancel();
            simulationTask.Wait();
            simulationTask.Dispose();

            lock (sim) { sim.Reset(); }

            (simulationTask, status) = CreateSimulationTask();
        }

        public void ResetAndRestart() {
            ResetSimulation();
            StartSimulation();
        }

        (Task, TaskStatus) CreateSimulationTask() {
            var status = new TaskStatus(Status.Idle);
            tokenSource = new CancellationTokenSource();
            var ct = tokenSource.Token;

            var task = new Task(() => {
                float timeStep = (float)initialStep;
                status.Status = Status.Running;
                for (int i = 0; i < 10000; i++) {
                    if (ct.IsCancellationRequested) {
                        status.Status = Status.Cancelled;
                        return;
                    }

                    float total;
                    lock (sim) {
                        sim.Advance(timeStep);
                        if (sim.WithinThreshold) {
                            status.Status = Status.MinimaReached;
                            return;
                        }

                        total = sim.GetTotalEnergy();
                    }
                    timeStep = (float)timeStepFunction(i, total);
                    status.TimeStep = timeStep;
                }
                status.Status = Status.IterationCap;
            }, ct);

            return (task, status);
        }
    }

    public enum Status {
        Idle,
        Running,
        Cancelled,
        IterationCap,
        MinimaReached
    }

    class StatusWrapper {
        public Status status;
        public float timestep;
        public int iterationCount;
        public StatusWrapper(Status init) {
            status = init;
            iterationCount = 0;
        }
    }

    class TaskStatus {
        private StatusWrapper status;
        public Status Status {
            get {
                lock (status) {
                    return status.status;
                }
            }
            set {
                lock (status) {
                    status.status = value;
                }
            }
        }
        public float TimeStep {
            get {
                lock (status) {
                    return status.timestep;
                }
            }
            set {
                lock (status) {
                    status.timestep = value;
                    lock (iterations) {
                        iterations.Add((status.iterationCount, status.timestep));
                        status.iterationCount += 1;
                    }
                }
            }
        }
        public List<(int, float)> TimeStepHistory {
            get {
                lock (iterations) {
                    return iterations.ToList();
                }
            }
        }

        private List<(int, float)> iterations;

        public TaskStatus(Status status) {
            this.status = new StatusWrapper(status);
            iterations = new List<(int, float)>();
        }
    }
}
