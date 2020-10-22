﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices.ComTypes;

namespace Core.Physics {
    [DebuggerDisplay("Position: {Point.Position.X} {Point.Position.Y}")]
    public class PhysicsNode : Node {
        public float Mass { get; set; }
        public Point Point;
        public Node Inner { get; private set; }
        public bool Pinned;
        public PhysicsNode(Node node) {
            var x = (float)((rng.NextDouble() - .5) * 10.0);
            var y = (float)((rng.NextDouble() - .5) * 10.0);
            Inner = node;
            Mass = 1.0f;
            Point = new Point(new Vector2(x, y));
            Name = node.Name;
        }

        public PhysicsNode(Node node, Vector2 initialPosition) {
            Inner = node;
            Mass = 1.0f;
            Point = new Point(initialPosition);
        }

        public override void NodeCreation(Graph g, NodeCreationInfo info) {
            Inner.NodeCreation(g, info);
        }

        public void ApplyForce(Vector2 force) {
            Point.Acceleration += force / Mass;
        }
    }

    public struct Vector2 {
        public static readonly Vector2 Zero = new Vector2(0, 0);

        public float X;
        public float Y;

        public Vector2(float x, float y) {
            X = x;
            Y = y;
        }

        public static Vector2 operator +(Vector2 v, Vector2 u) => new Vector2(v.X + u.X, v.Y + u.Y);

        public static Vector2 operator -(Vector2 v, Vector2 u) => new Vector2(v.X - u.X, v.Y - u.Y);

        public static Vector2 operator -(Vector2 v) => new Vector2(-v.X, -v.Y);

        public static Vector2 operator *(Vector2 v, Vector2 u) => new Vector2(v.X * u.X, v.Y * u.Y);

        public static Vector2 operator *(Vector2 v, float s) => new Vector2(v.X * s, v.Y * s);

        public static Vector2 operator /(Vector2 v, float s) => new Vector2(v.X / s, v.Y / s);

        public float Magnitude() {
            return (float)Math.Sqrt(X * X + Y * Y);
        }

        public Vector2 Normalize() {
            float m = Magnitude();
            return this * (1 / m);
        }
    }

    public class Point {
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 Acceleration;

        public Point(Vector2 pos, Vector2 vel, Vector2 acc) {
            Position = pos;
            Velocity = vel;
            Acceleration = acc;
        }

        public Point(Vector2 pos) {
            Position = pos;
            Velocity = Acceleration = Vector2.Zero;
        }
    }

    [DebuggerDisplay("{n1.Name}--{n2.Name}")]
    class Spring : Connection {
        public PhysicsNode n1;
        public PhysicsNode n2;
        public float Length { get; set; }
        public float K;

        public Spring(PhysicsNode n1, PhysicsNode n2, float length, float k) {
            Length = length;
            K = k;
            this.n1 = n1;
            this.n2 = n2;
        }

        public override float Strength() => 0.0f;
    }

    public readonly struct SimulationParams {
        public readonly float stiffness;
        public readonly float repulsion;
        public readonly float damping;
        public readonly float gravity;

        public SimulationParams(float stiffness, float repulsion, float damping, float gravity) {
            this.stiffness = stiffness;
            this.repulsion = repulsion;
            this.damping = damping;
            this.gravity = gravity;
        }
    }

    public class Simulation {
        public float Stiffness;
        public float Repulsion;
        public float Damping;
        public float Threshold;
        public float Gravity;
        public bool WithinThreshold;
        private Random rng;

        public Graph Graph { get; private set; }
        public Graph InnerGraph { get; private set; }
        public Dictionary<Node, PhysicsNode> physicsNodes;
        private List<Spring> springs;

        [Obsolete]
        public Simulation(Graph graph, float stiffness, float repulsion, float damping, float gravity)
            : this(graph, new SimulationParams(stiffness, repulsion, damping, gravity)) { }

        public Simulation(Graph graph, SimulationParams @params) {
            InnerGraph = graph;
            springs = new List<Spring>();
            Stiffness = @params.stiffness;
            Repulsion = @params.repulsion;
            Damping = @params.damping;
            Gravity = @params.gravity;
            Threshold = 0f;
            var physicsGraph = new Graph();

            var nodeMap = new Dictionary<Node, PhysicsNode>();
            foreach (var node in graph.Nodes) {
                var physicsNode = new PhysicsNode(node);
                physicsGraph.Add(physicsNode);
                nodeMap.Add(node, physicsNode);
            }

            physicsNodes = nodeMap;
            rng = new Random();

            foreach (PhysicsNode node in nodeMap.Values) {
                foreach ((var conn1, var conn2, Node other) in graph.GetNeighbors(node.Inner)) {
                    PhysicsNode node2 = nodeMap[other];
                    if (physicsGraph.TryGetDirectedConnection(node2, node, out Connection conn)) {
                        var spring = (Spring)conn;
                        physicsGraph.AddConnection(node, node2, new Spring(spring.n2, spring.n1, spring.Length, spring.K));
                    } else {
                        // one of these must be non-null, since we're iterating over all existing connections
                        var s1 = conn1?.Strength();
                        var s2 = conn2?.Strength();
                        s1 ??= s2;
                        s2 ??= s1;
                        var spring = new Spring(node,
                                       nodeMap[other],
                                       (float)(s1 + s2) / 2f,
                                       Stiffness);
                        physicsGraph.AddConnection(
                            node,
                            nodeMap[other],
                            spring
                            );
                        springs.Add(spring);
                    }
                }
            }

            Graph = physicsGraph;
        }

        public Simulation(Graph graph, SimulationParams @params, List<Node> includedNodes) {
            InnerGraph = graph;
            Stiffness = @params.stiffness;
            Repulsion = @params.repulsion;
            Damping = @params.damping;
            Gravity = @params.gravity;
            springs = new List<Spring>();

            var physicsGraph = new Graph();

            var nodeMap = new Dictionary<Node, PhysicsNode>();
            foreach (var node in includedNodes) {
                var physicsNode = new PhysicsNode(node);
                physicsGraph.Add(physicsNode);
                nodeMap.Add(node, physicsNode);
            }

            physicsNodes = nodeMap;
            rng = new Random();

            foreach (PhysicsNode node in nodeMap.Values) {
                foreach ((var conn1, var conn2, Node other) in graph.GetNeighbors(node.Inner)) {
                    if (!nodeMap.ContainsKey(other)) {
                        continue;
                    }

                    PhysicsNode node2 = nodeMap[other];
                    if (physicsGraph.TryGetDirectedConnection(node2, node, out Connection conn)) {
                        var spring = (Spring)conn;
                        physicsGraph.AddConnection(node, node2, new Spring(spring.n2, spring.n1, spring.Length, spring.K));
                    } else {
                        // one of these must be non-null, since we're iterating over all existing connections
                        var s1 = conn1?.Strength();
                        var s2 = conn2?.Strength();
                        s1 ??= s2;
                        s2 ??= s1;
                        var spring = new Spring(node,
                                                nodeMap[other],
                                                (float)(s1 + s2) / 2f,
                                                Stiffness);
                        physicsGraph.AddConnection(
                            node,
                            nodeMap[other],
                            spring);
                        springs.Add(spring);
                    }
                }
            }

            Graph = physicsGraph;

        }

        public void Reset() {
            foreach (PhysicsNode pn in Graph.Nodes.Cast<PhysicsNode>()) {
                pn.Point.Position.X = (float)((rng.NextDouble() - .5) * 10.0);
                pn.Point.Position.Y = (float)((rng.NextDouble() - .5) * 10.0);
            }
        }

        public void AddRange(IEnumerable<Node> nodes) {
            (var bottomleft, var topright) = GetBoundingBox();

            var conns = new List<(PhysicsNode, List<(Connection, Node)>)>();

            foreach (Node n in nodes) {
                if (physicsNodes.ContainsKey(n)) {
                    continue;

                    // throw new Exception("This system already contains the node!" +
                    //     " NOTE: this exception only occurs in debug mode");
                }

                var x = MapInterval(rng.NextDouble(), 0, 1, bottomleft.X, topright.X);
                var y = MapInterval(rng.NextDouble(), 0, 1, bottomleft.Y, topright.Y);
                var pNode = new PhysicsNode(n, new Vector2((float)x, (float)y));

                conns.Add((pNode, InnerGraph.GetOutgoingConnections(n)));

                physicsNodes.Add(n, pNode);
                Graph.Add(pNode);
            }
            // SelectMany(x => x) flattens the list
            foreach ((PhysicsNode src, var outgoing, Node dst) in
                conns.SelectMany(nl => {
                    (var src, var list) = nl;
                    return list.Select(cn => (src, cn.Item1, cn.Item2));
                })) {
                if (!physicsNodes.ContainsKey(dst)) continue;
                PhysicsNode pDst = physicsNodes[dst];

                if (Graph.TryGetDirectedConnection(pDst, src, out var existingSpring)) {
                    Graph.AddConnection(src, pDst, existingSpring);
                } else {
                    InnerGraph.TryGetDirectedConnection(dst, src.Inner, out var incoming);
                    var s1 = incoming?.Strength();
                    var s2 = outgoing?.Strength();
                    s1 ??= s2;
                    s2 ??= s1;
                    var spring = new Spring(src, pDst, (float)(s1 + s2) / 2f, Stiffness);
                    Graph.AddConnection(src, pDst, spring);
                    springs.Add(spring);
                }
            }
        }

        public void RemoveRange(IEnumerable<Node> nodes) {
            var removeSet = new HashSet<Spring>();
            var removeNodes = new HashSet<Node>();
            var removePhysicsNodes = new HashSet<PhysicsNode>();
            foreach (Node node in nodes) {
                removeNodes.Add(node);
                removePhysicsNodes.Add(physicsNodes[node]);
            }

            foreach (var spring in springs) {
                if (removeNodes.Contains(spring.n1.Inner) || removeNodes.Contains(spring.n2.Inner)) {
                    removeSet.Add(spring);
                }
            }
        }

        /// <summary>
        /// Map a value x from the interval [a, b] to [c, d]
        /// </summary>
        /// <param name="x">the value to map</param>
        /// <param name="a">lower bound of origin interval</param>
        /// <param name="b">upper bound of origin interval</param>
        /// <param name="c">lower bound of target interval</param>
        /// <param name="d">upper bound of target interval</param>
        /// <returns></returns>
        double MapInterval(double x, double a, double b, double c, double d) =>
            c + (d - c) / (b - a) * (x - a);
        
        /// <summary>
        /// Adds a new node and places it randomly on the bounding box of the system
        /// </summary>
        /// <param name="n"></param>
        public void Add(Node n) {
            if (physicsNodes.ContainsKey(n)) {
                throw new Exception("This simulation already contains the node");
            }
            var conns = InnerGraph.GetOutgoingConnections(n);

            (var bottomleft, var topright) = GetBoundingBox();
            var x = MapInterval(rng.NextDouble(), 0, 1, bottomleft.X, topright.X);
            var y = MapInterval(rng.NextDouble(), 0, 1, bottomleft.Y, topright.Y);
            var pNode = new PhysicsNode(n, new Vector2((float)x, (float)y));

            foreach ((var outgoing, var other) in conns) {
                if (!physicsNodes.ContainsKey(other))
                    continue;

                InnerGraph.TryGetDirectedConnection(other, n, out var incoming);
                var s1 = incoming?.Strength();
                var s2 = outgoing?.Strength();
                s1 ??= s2;
                s2 ??= s1;
                var spring = new Spring(pNode, physicsNodes[other], (float)(s1 + s2) / 2f, Stiffness);
                springs.Add(spring);
            }
            physicsNodes.Add(n, pNode);
            Graph.Add(pNode);
        }

        private (Vector2, Vector2) GetBoundingBox() {
            if (Graph.Nodes.Count == 0) {
                return (Vector2.Zero, Vector2.Zero);
            }

            Vector2 bottomLeft = ((PhysicsNode)Graph.Nodes[0]).Point.Position;
            Vector2 topRight = ((PhysicsNode) Graph.Nodes[0]).Point.Position;

            foreach (var n in Graph.Nodes.Cast<PhysicsNode>()) {
                var p = n.Point.Position;

                bottomLeft.X = Math.Min(p.X, bottomLeft.X);
                bottomLeft.Y = Math.Min(p.Y, bottomLeft.Y);

                topRight.X = Math.Max(p.X, topRight.X);
                topRight.Y = Math.Max(p.Y, topRight.Y);
            }

            return (bottomLeft, topRight);
        }

        void ApplyCoulombsLaw() {
            foreach (var n1 in Graph.Nodes.Cast<PhysicsNode>()) {
                var p1 = n1.Point;
                foreach (var n2 in Graph.Nodes.Cast<PhysicsNode>()) {
                    if (ReferenceEquals(n1, n2)) {
                        continue;
                    }

                    var p2 = n2.Point;
                    var d = p1.Position - p2.Position;
                    var dist = d.Magnitude();
                    // dist *= dist;
                    var norm = d.Normalize();
                    if (n1.Pinned && n2.Pinned) { } else if (n1.Pinned) {
                        n2.ApplyForce(-norm * Repulsion / dist);
                    } else if (n2.Pinned) {
                        n1.ApplyForce(norm * Repulsion / dist);
                    } else {
                        n1.ApplyForce(norm * Repulsion / (dist * 0.5f));
                        n2.ApplyForce(-norm * Repulsion / (dist * 0.5f));
                    }
                }
            }
        }

        void ApplyHookesLaw() {
            foreach (var spring in springs) {
                var n1 = spring.n1;
                var n2 = spring.n2;
                var d = spring.n1.Point.Position - spring.n2.Point.Position;
                var dist = d.Magnitude();
                //dist -= spring.Length;
                var displacement = dist * Stiffness;
                var direction = (d * dist).Normalize();

                var force = direction * spring.K * displacement * (dist - spring.Length);

                if (n1.Pinned && n2.Pinned) {
                    
                } else if (n1.Pinned) {
                    n2.ApplyForce(force);
                } else if (n2.Pinned) {
                    n1.ApplyForce(-force);
                } else {
                    n1.ApplyForce(-force * 0.5f);
                    n2.ApplyForce(force * 0.5f);
                }
            }
        }

        void AttractToCenter() {
            foreach (var node in Graph.Nodes.Cast<PhysicsNode>()) {
                if (!node.Pinned) {
                    var d = node.Point.Position;
                    var displacement = d.Magnitude();
                    var direction = -d.Normalize();
                    node.ApplyForce(direction * (Stiffness * displacement * Gravity));
                }
            }
        }

        void UpdateVelocity(float timeStep) {
            foreach (var node in Graph.Nodes.Cast<PhysicsNode>()) {
                var point = node.Point;
                point.Velocity += point.Acceleration * timeStep;
                point.Velocity *= Damping;
                point.Position += point.Velocity * timeStep;
                point.Acceleration = Vector2.Zero;
            }
        }

        public float GetTotalEnergy() {
            var total = 0.0f;
            foreach (var node in Graph.Nodes.Cast<PhysicsNode>()) {
                var speed = node.Point.Velocity.Magnitude();
                total += 0.5f * node.Mass * speed * speed;
            }
            return total;
        }

        // Advance the simulation
        public void Advance(float timeStep) {
            ApplyCoulombsLaw();
            ApplyHookesLaw();
            AttractToCenter();
            UpdateVelocity(timeStep);

            WithinThreshold = GetTotalEnergy() <= Threshold;
        }
    }
}
