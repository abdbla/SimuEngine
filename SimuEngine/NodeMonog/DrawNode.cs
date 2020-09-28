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
                    point.Position = new Core.Physics.Vector2(value.X, value.Y);
                }
            }
        }
        public static Core.Physics.System simulation = null;

        public DrawNode(Vector2 position, Node n)
        {
            this.position = position;
            this.node = n;
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
                                                            