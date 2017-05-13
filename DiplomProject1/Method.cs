using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomProject1
{
    public class Method : CodeObjectInterface
    {
        public Method(string name, string body, List<string> input, List<string> output, int pos = 0, int bufferOffset = 0, int length = 0)
        {
            Type = ObjectType.Method;
            Name = name;
            Body = body;
            Modificators = output;

            Position = pos;
            Length = length;
            CommonLength = length;
            body = parseVariables(body, bufferOffset);
            body = parseDoWhile(body, bufferOffset);
            body = parseForWhile(body, bufferOffset);
            body = parseIfElse(body, bufferOffset);
            body = parseActions(body, bufferOffset);
            Children.Sort();
            /*body = AppParams.Instance.parseVariables(body, ref vars);
            foreach(string str in input)
            {
                AppParams.Instance.parseVariables(str + ";", ref vars);
            }

            Children = new List<CodeObjectInterface>();
            Children.AddRange(vars);*/
        }

        protected override void generateToken()
        {
            token = "";
            foreach (CodeObjectInterface obj in Children)
            {
                token += obj.Token;
            }
        }

        public override List<CodeObjectInterface> RecursiveChildrenList
        {
            get
            {
                List<CodeObjectInterface> buffer = new List<CodeObjectInterface>();

                foreach (CodeObjectInterface obj in Children)
                {
                    buffer.AddRange(obj.RecursiveChildrenList);
                }

                return buffer;
            }
        }
    }
}
