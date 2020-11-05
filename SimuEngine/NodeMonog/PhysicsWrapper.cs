using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Core;
using Core.Physics;

using SharpDX.Direct2D1;

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
        const int MAXNODES = 100;

        public Simulation Simulation;
        public Graph Graph;
        Node selectedNode;

        SimulationTaskState savedState = null;

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
            get { var dn = new DrawNode(SelectedNode, Simulation); dn.separation = 0; return dn; }
            set => SelectedNode = value.node;
        }

        List<(Node, uint)> neighbors;

        Dictionary<Node, DrawNode> drawNodes;
        public IEnumerable<DrawNode> DrawNodes { get {
                drawNodes ??= MakeDrawNodeList();
                return drawNodes.Values;
            }
        }

        private Dictionary<Node, DrawNode> MakeDrawNodeList() {
            Dictionary<Node, DrawNode> drawNodes = new Dictionary<Node, DrawNode>() {
                { selectedNode, new DrawNode(selectedNode, Simulation) }
            };

            foreach ((Node n, DrawNode draw_n) in
                neighbors.Select(x => (x.Item1, new DrawNode(x.Item1, Simulation, sep: (int)x.Item2)))) {
                try {
                    drawNodes.Add(n, draw_n);
                } catch (ArgumentException) {

                }
            }

            // ???
            // TODO: Jury's still out on whether this is cringe
            foreach (DrawNode n in Simulation.physicsNodes.Keys.Where(x => !neighbors.Select(x => x.Item1).Prepend(selectedNode).Contains(x)).Select(x => new DrawNode(x, Simulation, sep: 100))) {
                drawNodes.Add(n.node, n);
            }

            return drawNodes;
        }

        public double initialStep = 1.0;
        public Func<int, double, double, double> timeStepFunction = (iterations, oldTimeStep, energy) => {

            //double negLog = Math.Pow(2, -Math.Log(energy, 2));
            //double timeStep = Math.Min(negLog, energy / 10);
            ////timeStep = Math.Max(timeStep, 0.01f);
            //timeStep = Math.Min(timeStep, 1);

            return 0.6 * Math.Exp(-Math.Pow(((double)iterations - 4000) / 2000, 2) / 2);
        };
        Task simulationTask;
        CancellationTokenSource tokenSource;

        public uint Degrees { get; private set; }

        public SimulationTaskStatus SimulationStatus { get; private set; }

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
                                biGraph.AddConnection(n2, n1, n2_n1);
                            } else {
                                biGraph.AddConnection(n1, n2, n1_n2);
                                biGraph.AddConnection(n2, n1, new DuplicatedConnection(n1_n2));
                            }
                        }
                    }
                }
            }

            this.Graph = biGraph;

            neighbors = this.Graph.GetNeighborsDegrees(selectedNode, degrees - 1).Take(MAXNODES).ToList();
            Simulation = new Simulation(this.Graph, @params, neighbors
                .Select(x => x.Item1)
                .Except(new[] { selectedNode })
                .Prepend(selectedNode)
                .ToList());

            Simulation.physicsNodes[selectedNode].Pinned = false;

            Degrees = degrees;
            
            (simulationTask, SimulationStatus) = CreateSimulationTask();

            singleTimeStep = (float)initialStep;
        }

        public void Update(Node newSelected) {
            var newNeighbors = Graph.GetNeighborsDegrees(newSelected, Degrees - 1).Take(MAXNODES).ToList();
            var newSet = new HashSet<Node>(newNeighbors.Select(x => x.Item1).Prepend(newSelected));
            var oldSet = new HashSet<Node>(neighbors.Select(x => x.Item1).Prepend(selectedNode));

            oldSet.ExceptWith(newSet);

            lock (Simulation) {
                // sim.physicsNodes[selectedNode].Pinned = false;

                Simulation.RemoveRange(oldSet);
                Simulation.AddRange(newSet);

                neighbors = newNeighbors;
                selectedNode = newSelected;
                // sim.physicsNodes[selectedNode].Pinned = !true;
                // sim.Origin = sim.physicsNodes[selectedNode].Point.Position;
            }

            //ResetAndRestart();

            Restart();

            singleTimeStep = (float)initialStep;

            drawNodes = null;
        }

        public void StartSimulation() {
            try {
                simulationTask.Start();
            } catch (InvalidOperationException) {}
        }

        /// <summary>
        /// Doing this while the simulation is running is a bad idea (I think)
        /// </summary>
        public void ResetSimulationState() {
            lock (Simulation) { Simulation.Reset(); }
        }

        public void RestartSimulationTask() {
            Cancel();
            (simulationTask, SimulationStatus) = CreateSimulationTask();
            savedState = null;
            StartSimulation();
        }

        public float GetTotalEnergy() {
            lock (Simulation) {
                return Simulation.GetTotalEnergy();
            }
        }

        public void Restart() {
            RestartSimulationTask();
        }

        public void ResetAndRestart() {
            Cancel();

            ResetSimulationState();

            (simulationTask, SimulationStatus) = CreateSimulationTask();

            StartSimulation();
        }

        public void Cancel() {
            tokenSource.Cancel();
            try {
                simulationTask.Wait();
            } catch (OperationCanceledException) {
                // we cancelled successfully
            } catch (AggregateException e) when (simulationTask.Exception == null) {
                Console.WriteLine("Caught aggregate exception: ", e);
            } finally {
                simulationTask.Dispose();
                tokenSource.Dispose();
            }
            simulationTask.Dispose();
        }

        public void Resume() {

        }
 
        public void CompleteReset() {
            Cancel();

            (simulationTask, SimulationStatus) = CreateSimulationTask();

            Simulation.ResetFull();
        }

        float singleTimeStep;
        int singleIter;

        public void AdvanceOnce() {
            if (SimulationStatus.Status == Status.Running) return;

            savedState ??= new SimulationTaskState((float)initialStep);
            
            lock (Simulation) {
                Simulation.Advance(savedState.TimeStep);
            }

            savedState.TimeStep = (float)timeStepFunction(savedState.IterationCount++,
                                                          savedState.TimeStep,
                                                          Simulation.GetTotalEnergy());
        }

        public DrawNode LookupDrawNode(Node node) {
            var success = drawNodes.TryGetValue(node, out var ret);
            return success ? ret : null;
        }

        (Task, SimulationTaskStatus) CreateSimulationTask() {
            var status = new SimulationTaskStatus(Status.Idle);
            tokenSource = new CancellationTokenSource();
            var ct = tokenSource.Token;

            var task = new Task(() => {
                float timeStep = SimulationStatus == null ? (float)initialStep : 0.0001f;
                status.Status = Status.Running;

                // if it's null, make a new one
                savedState ??= new SimulationTaskState((float)initialStep);
                int i = savedState.IterationCount;

                for (; i < 1000000; i++) {
                    if (ct.IsCancellationRequested) {
                        status.Status = Status.Cancelled;
                        lock (savedState) {
                            savedState.IterationCount = i;
                            savedState.TimeStep = timeStep;
                        }
                        throw new OperationCanceledException();
                    }

                    float total = Simulation.GetTotalEnergy();
                    lock (Simulation) {
                        Simulation.Advance(timeStep);
                        if (Simulation.WithinThreshold) {
                            status.Status = Status.MinimaReached;
                            return;
                        }

                        var newTotal = Simulation.GetTotalEnergy();
                        if (newTotal / total > 1.0 && total != 0) {
                            Console.WriteLine($"Warning: newTotal / total = {newTotal / total}");
                            if (newTotal / total > 100.0) {
                                Console.WriteLine($"Severe warning: newTotal / total = {newTotal / total}, stopping.");
                                // status.Status = Status.Cancelled;
                                // return;
                            }
                        }
                        total = newTotal;
                    }
                    timeStep = (float)timeStepFunction(i, timeStep, total);
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

    class SimulationTaskStatus {
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

        public SimulationTaskStatus(Status status) {
            this.status = new StatusWrapper(status);
            iterations = new List<(int, float)>();
        }
    }

    class SimulationTaskState {
        public int IterationCount;
        public float TimeStep;

        public SimulationTaskState(float initialStep) {
            IterationCount = 0;
            TimeStep = initialStep;
        }

        public SimulationTaskState(float currentTimeStep, int currentIterationCount) {
            IterationCount = currentIterationCount;
            TimeStep = currentTimeStep;
        }
    }
}
