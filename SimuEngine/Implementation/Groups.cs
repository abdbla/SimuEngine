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
        public PersonGroup(string type) : base() {
            switch (type.ToUpper()) {
                case "FAMILY":
                    statuses.Add($"Family");
                    break;
                case "WORK":
                    statuses.Add($"Work");
                    break;
                case "FRIENDS":
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
