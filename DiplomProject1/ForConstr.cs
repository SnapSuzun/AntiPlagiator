﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomProject1
{
    public class ForConstr : SystemConstruction
    {
        public ForConstr(string condition, string body, int pos = 0, int bufferOffset = 0, int length = 0)
        {
            Type = ObjectType.For;
            Name = "for";
            Body = body;
            Condition = condition;
            Position = pos;
            Length = length;
            body = parseVariables(body, bufferOffset);
            body = parseDoWhile(body, bufferOffset);
            body = parseForWhile(body, bufferOffset);
            body = parseIfElse(body, bufferOffset);
            body = parseActions(body, bufferOffset);
            Children.Sort();
        }
    }
}
