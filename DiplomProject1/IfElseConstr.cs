using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomProject1
{
    class IfElseConstr : SystemConstruction
    {
        public IfElseConstr(string condition, string body, int pos = 0, int bufferOffset = 0, int length = 0)
        {
            if (condition.Length > 0)
            {
                Name = "if";
                Type = ObjectType.If;
            }
            else
            {
                Name = "else";
                Type = ObjectType.Else;
            }
            Condition = condition;
            Body = body;
            Position = pos;
            Length = length;
            body = parseVariables(body, bufferOffset);
            body = parseDoWhile(body, bufferOffset);
            body = parseForWhile(body, bufferOffset);
            body = parseIfElse(body, bufferOffset);
            body = parseActions(body, bufferOffset);
            Children.Sort();
            /*if(bodies.Count > conditions.Count)
            {
                conditions.Add("");
            }*/
        }
    }
}
