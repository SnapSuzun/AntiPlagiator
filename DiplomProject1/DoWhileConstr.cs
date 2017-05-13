using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiplomProject1
{
    class DoWhileConstr : SystemConstruction
    {
        public DoWhileConstr(string condition, string body, int pos = 0, int bufferOffset = 0, int length = 0)
        {
            Position = pos;
            Length = length;
            Type = ObjectType.Do;
            Name = "do / while";
            Body = body;
            Condition = condition;
            body = parseVariables(body, bufferOffset);
            body = parseDoWhile(body, bufferOffset);
            body = parseForWhile(body, bufferOffset);
            body = parseIfElse(body, bufferOffset);
            body = parseActions(body, bufferOffset);
            Children.Sort();
        }
    }
}
