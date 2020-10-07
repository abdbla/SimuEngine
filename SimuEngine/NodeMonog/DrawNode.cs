using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimuEngine;
using Core;
using Microsoft.Xna.Framework;
using SharpDX.DirectWrite;
using SharpDX.DXGI;

using Simulation = Core.Physics.Simulation;
using PVec2 = Core.Physics.Vector2;

namespace NodeMonog
{
    class DrawNode
    {
        Vector2? position;

        public Node node;

        public Vector2 Position
        {
            get
            {
                var point = simulation.physicsNodes[node].Point.Position;
                return new Vector2((point.X * 50) + 300, (point.Y * 50) + 300);
            }
            set
            {
                if (simulation == null) position = value;
                else
                {
                    var point = simulation.physicsNodes[node].Point;
                    point.Position = new PVec2(value.X, value.Y);
                }
            }
        }
        public Simulation simulation { get; private set; }

        public DrawNode(Vector2 pos, Node n, Simulation sim)
        {
            position = pos;
            node = n;
            simulation = sim;
        }
        //public Dictionary<string, int> TraitsWorkaround {
        //    get => traits;
        //    set => traits = value;
        //}
        public DrawNode()
        {

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
                                                            