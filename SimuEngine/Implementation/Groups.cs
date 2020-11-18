using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core;

namespace Implementation {
    [Serializable]
    class PersonGroup : Group {
        static int id = 0;
        int idx;
        public PersonGroup() : base() {
            switch (Node.rng.Next(1, 5)) {
                case 1:
                    statuses.Add($"Family");
                    break;
                case 2:
                    statuses.Add($"Work");
                    break;
                case 3:
                    statuses.Add($"Friends");
                    break;
                default:
                    statuses.Add($"Acquiantances");
                    break;
            }
            idx = ++id;
        }
    }
}
