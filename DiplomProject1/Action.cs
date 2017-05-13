using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiplomProject1
{
    class Action : CodeObjectInterface
    {
        protected string Operator;
        protected List<string> Params = new List<string>();
        public Action(string name, string value, int pos = 0, int bufferOffset = 0, int length = 0, string oper = "")
        {
            Operator = oper;
            Name = name;
            Position = pos;
            Length = length;
            CommonLength = length;
            Value = value;
            Body = value;
            if (oper.Length == 0)
            {
                Type = ObjectType.Action;
                parseParams(bufferOffset);
            }
            else
            {
                if(oper.Length > 1 && oper.IndexOf('=') >= 0 && oper != "<=" && oper != ">=")
                    oper = oper.Replace("=", "");
                int index = AppParams.Instance.getOperationIndex(oper);
                if (index >= 0)
                    Type = (ObjectType)index;
                if (!checkArray(value, bufferOffset))
                    parseValue(value, bufferOffset);
            }
        }



        protected void parseValue(string body, int offset)
        {
            List<string> polishNotation = AppParams.getReversePolishNotation(body);
            Stack<string> bufferStack = new Stack<string>();

            if (polishNotation.Count == 1 && polishNotation[0].ToList().FindAll(x => x == '(').Count > 0)
            {
                string bufferWord = "", value = "";
                int opens = 0;
                foreach (char c in polishNotation[0])
                {
                    if (opens > 0)
                    {
                        if (c == '(')
                            opens++;
                        else if (c == ')') opens--;
                        if (opens != 0)
                        {
                            value += c;
                        }
                        continue;
                    }
                    switch (c)
                    {
                        case '(':
                            opens++;
                            break;
                        case ' ':
                            break;
                        default:
                            bufferWord += c;
                            break;
                    }
                }
                Action meth = new Action(polishNotation[0], value, Position, Position, Length);
                Children.Add(meth);
                return;
            }

            foreach(string str in polishNotation)
            {
                if(AppParams.Instance.isOperation(str))
                {
                    string left, right;
                    right = bufferStack.Pop();
                    left = bufferStack.Pop();

                    string val = "";
                    if (left.Length > 1)
                        val += "(" + left + ")";
                    else val += left;

                    val += str;

                    if (right.Length > 1)
                        val += "(" + right + ")";
                    else val += right;

                    if(left.ToList().FindAll(x => x == '(').Count == 1)
                    {
                        string bufferWord = "", value = "";
                        int opens = 0;
                        foreach(char c in left)
                        {
                            if(opens > 0)
                            {
                                if (c == '(')
                                    opens++;
                                else if(c == ')') opens--;
                                if (opens != 0)
                                {
                                    value += c;
                                }
                            }
                            switch(c)
                            {
                                case '(':
                                    opens++;
                                    break;
                                case ' ':
                                    break;
                                default:
                                    bufferWord += c;
                                    break;
                            }
                        }
                        Action meth = new Action(left, value, Position, Position, Length);
                        Children.Add(meth);
                    }

                    if (right.ToList().FindAll(x => x == '(').Count == 1)
                    {
                        string bufferWord = "", value = "";
                        int opens = 0;
                        foreach (char c in right)
                        {
                            if (opens > 0)
                            {
                                if (c == '(')
                                    opens++;
                                else if (c == ')') opens--;
                                if (opens != 0)
                                {
                                    value += c;
                                }
                            }
                            switch (c)
                            {
                                case '(':
                                    opens++;
                                    break;
                                case ' ':
                                    break;
                                default:
                                    bufferWord += c;
                                    break;
                            }
                        }
                        Action meth = new Action(right, value, Position, Position, Length);
                        Children.Add(meth);
                    }

                    Action var = new Action(val, "", Position, Position, Length, str);
                    Children.Add(var);
                    bufferStack.Push(val);
                }
                else
                {
                    bufferStack.Push(str);
                }
            }
        }

        protected void parseParams(int offset)
        {
            string bufferWord = "";
            int opens = 0;
            int pos = 0;
            foreach(char c in Body)
            {
                switch(c)
                {
                    case '(':
                    case '{':
                        bufferWord += c;
                        opens++;
                        break;
                    case ')':
                    case '}':
                        opens--;
                        bufferWord += c;
                        break;
                    case ',':
                        if(opens > 0)
                        {
                            bufferWord += c;
                        }
                        else
                        {
                            bufferWord = bufferWord.Trim();
                            pos = Body.IndexOf(bufferWord, pos);
                            ActionParam var = new ActionParam(bufferWord, pos + offset, 0, bufferWord.Length);
                            if (!checkArray(bufferWord, pos + offset))
                                parseValue(bufferWord, pos + offset);
                            //Children.Add(var);
                            pos += bufferWord.Length;
                            //Params.Add(bufferWord);
                            bufferWord = "";
                        }
                        break;
                    default:
                        bufferWord += c;
                        break;
                }
            }
            if (bufferWord.Length > 0)
            {
                bufferWord = bufferWord.Trim();
                pos = Body.IndexOf(bufferWord, pos);
                ActionParam var = new ActionParam(bufferWord, pos + offset, 0, bufferWord.Length);
                if (!checkArray(bufferWord, pos + offset))
                    parseValue(bufferWord, pos + offset);
                //Children.Add(var);
                //Params.Add(bufferWord);
            }
        }

        protected bool checkArray(string body, int offset)
        {
            int opens = 0;
            foreach(char c in body)
            {
                if (c != ' ' && c != '{') return false;
                if (c == '{') break;
            }
            string bufferWord = "";
            int pos = 0;
            foreach(char c in body)
            {

                switch(c)
                {
                    case ' ':
                    case '{':
                        break;
                    case ')':
                        bufferWord += c;
                        opens--;
                        break;
                    case '(':
                        bufferWord += c;
                        opens++;
                        break;
                    case '}':
                    case ',':
                        if(bufferWord.Length > 0 && opens == 0)
                        {
                            bufferWord = bufferWord.Trim();
                            pos = Body.IndexOf(bufferWord, pos);
                            if (!checkArray(bufferWord, pos + offset))
                                parseValue(bufferWord, pos + offset);
                            //Params.Add(bufferWord);
                            pos += bufferWord.Length;
                            bufferWord = "";
                        }
                        break;
                    default:
                        bufferWord += c;
                        break;
                }
            }

            return true;
        }

        protected override void generateToken()
        {
            token = "";
            foreach (CodeObjectInterface obj in Children)
            {
                token += obj.Token;
            }

            if (Operator != "=")
                token += (char)Type;
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

                if (Operator != "=")
                    buffer.Add(this);
                return buffer;
            }
        }
    }
}
