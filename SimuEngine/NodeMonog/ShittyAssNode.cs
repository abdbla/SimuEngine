using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimuEngine;
using Core;
using Microsoft.Xna.Framework;

namespace NodeMonog
{
    class ShittyAssNode : Node
    {
        public Point position;

        public string NName
        {
            get { return Name; }
            set { Name = value; }
        }

        public ShittyAssNode(Point position)
        {
            this.position = position;
        }

        public override void OnCreate()
        {
            //traits.Add("age", 50);
            //traits.Add("Corona", 2);
        }                                                   
                                                            
        public override void OnGenerate()                   
        {                                                   
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
    }
}                                                           
                                                            