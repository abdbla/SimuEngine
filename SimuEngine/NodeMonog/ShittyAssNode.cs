using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimuEngine;
using Core;
using Microsoft.Xna.Framework;
using SharpDX.DirectWrite;

namespace NodeMonog
{
    class ShittyAssNode : Node
    {
        public Point position { get {
                var point = simulation.physicsNodes[this].Point.Position;
                return new Point((int)(point.X * 50) + 300, (int)(point.Y * 50) + 300);
            }
            set { }
        }
        public static Core.Physics.System simulation;
        public string NName
        {
            get { return Name; }
            set { Name = value; }
        }

        public ShittyAssNode(Point position)
        {
            this.position = position;
        }

        public override void NodeCreation(NodeCreationInfo info = NodeCreationInfo.Empty) {
            throw new NotImplementedException();
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

        public override float Strength() {
            return affection / 1000f;
        }
    }
}                                                           
                                                            