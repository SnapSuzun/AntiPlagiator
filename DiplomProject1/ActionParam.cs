using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomProject1
{
    class ActionParam : CodeObjectInterface
    {
        public ActionParam(string body, int pos = 0, int bufferOffset = 0, int length = 0)
        {
            Name = body;
            Body = body;
            Position = pos;
            Length = length;
            Type = ObjectType.ActionParam;
        }
    }
}
