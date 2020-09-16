using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimuEngine;
using Core;

namespace NodeMonog
{
    class ShittyAssNode : Node
    {
        
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
                                                            