﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using Microsoft.Xna.Framework;

using Simulation = Core.Physics.Simulation;
using PVec2 = Core.Physics.Vector2;
using System.Diagnostics;

namespace NodeMonog
{
    class DrawGraph : Graph
    {
    }

    [DebuggerDisplay("Inner node: {node.Name}")]
    class DrawNode
    {
        public Node node;
        public int separation;

        public Vector2 Position {
            get {
                var point = simulation.physicsNodes[node].Point.Position;
                return new Vector2((point.X * 50) + 300, (point.Y * 50) + 300);
            }
            set {
                
            }
        }
        public Simulation simulation { get; private set; }

        public DrawNode(Vector2 pos, Node n, Simulation sim)
        {
            Position = pos;
            node = n;
            simulation = sim;
        }
        //public Dictionary<string, int> TraitsWorkaround {
        //    get => traits;
        //    set => traits = value;
        //}
        public DrawNode(Node n, Simulation sim)
        {
            node = n;
            simulation = sim;
        }

        public DrawNode(Node n, Simulation sim, int sep) {
            node = n;
            simulation = sim;
            separation = sep;
        }
    }

    class ShittyAssKnect : Connection
    {
        public int affection, affection2;


        public ShittyAssKnect(int affection, int affection2)
        {
            this.affection = affection;
            this.affection2 = affection2;
        }

        public override float Strength()
        {
            return affection / 1000f;
        }

    }

}
