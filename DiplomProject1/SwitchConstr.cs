using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomProject1
{
    class SwitchConstr : SystemConstruction
    {
        public SwitchConstr(string condition, string body, int pos = 0, int bufferOffset = 0, int length = 0)
        {
            Type = ObjectType.Switch;
            Name = "switch";
            Body = body;
            Condition = condition;
            Position = pos;
            Length = length;
        }
    }
}
