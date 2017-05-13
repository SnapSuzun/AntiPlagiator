using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiplomProject1
{
    public enum ObjectType
    {
        Variable = 1,
        Method,
        Action,
        For,
        While,
        Do,
        If,
        Else,
        Switch,
        Case,
        ActionParam, 
    }

    public class CustomTreeNode : TreeNode
    {
        protected CodeObjectInterface obj;

        public CodeObjectInterface Object
        {
            get
            {
                return obj;
            }
        }

        public CustomTreeNode(string name, CodeObjectInterface obj) : base(name)
        {
            this.obj = obj;
        }
    }

    public abstract class CodeObjectInterface : IComparable<CodeObjectInterface>
    {
        public ObjectType                   Type; // тип объекта
        public string                       Name; // название
        public string                       Value; // значение объекта
        public string                       Body;
        public List<CodeObjectInterface>    Children = new List<CodeObjectInterface>(); // ссылки на объекты, которые содержатся в этом объекте
        public List<String>                 Modificators; // модификаторы, которые стоят перед определением объекта
        public int                          Position = 0, BodyOffset = 0, Length = 0;
        protected string                    token = "";
        public int CommonLength = 0;
        public string Token
        {
            get
            {
                generateToken();
                return token;
            }
        }

        public virtual List<CodeObjectInterface> RecursiveChildrenList
        {
            get
            {
                List<CodeObjectInterface> buffer = new List<CodeObjectInterface>();
                buffer.Add(this);

                foreach (CodeObjectInterface obj in Children)
                {
                    buffer.AddRange(obj.RecursiveChildrenList);
                }

                return buffer;
            }
        }

        public virtual CustomTreeNode Node
        {
            get
            {
                CustomTreeNode root = new CustomTreeNode(Name, this);

                foreach (CodeObjectInterface obj in Children)
                {
                    root.Nodes.Add(obj.Node);
                }
                return root;
            }
        }

        protected string parseActions(string body, int offset)
        {
            List<string> bufferWords = new List<string>();
            string bufferBody = body;
            string bufferWord = "";
            string operation = "", value = "";
            string buffer = "";
            bool ignore = false, func = false, bOper = false, bOperator = false;
            int valOpens = 0, opens = 0, pos = 0;



            foreach (char c in body)
            {
                buffer += c;
                if (opens > 0)
                {
                    if (c == '{')
                        opens++;
                    else if (c == '}')
                        opens--;
                    if (opens == 0)
                    {
                        operation = "";
                        value = "";
                        bufferWords.Clear();
                        bufferWord = "";
                        ignore = false;
                        buffer = "";
                    }
                    continue;
                }
                if (ignore)
                {
                    if (c == '{')
                        opens++;
                    if (c == ';')
                    {
                        ignore = false;
                        bufferWords.Clear();
                        bufferWord = "";
                        operation = "";
                        value = "";
                        buffer = "";
                    }
                    continue;
                }
                if(bOperator)
                {
                    if(c == ';')
                    {
                        buffer = buffer.Trim();
                        pos = Body.IndexOf(buffer, pos);
                        bufferBody = bufferBody.Substring(0, bufferBody.IndexOf(buffer)) + bufferBody.Substring(bufferBody.IndexOf(buffer) + buffer.Length);
                        int bufferOffset = offset + pos + (value.Length > 0 ? buffer.IndexOf(value) : 0);
                        Action var = new Action(buffer, value, pos + offset, bufferOffset, buffer.Length, bufferWords.First());
                        var.CommonLength = buffer.Length;
                        if (bufferWords.First() != "=")
                            parseValue(buffer, pos + offset, bufferOffset);
                        else Children.Add(var);
                        pos += buffer.Length;
                        //Children.Add(var);

                        bOper = false;
                        bOperator = false;
                        operation = "";
                        bufferWord = "";
                        bufferWords.Clear();
                        buffer = "";
                        value = "";
                    }
                    else value+=c;
                    continue;
                }
                if (valOpens > 0 && func)
                {
                    if (c == '{' || c == '(' || c == '[')
                        valOpens++;
                    else if (c == '}' || c == ')' || c == ']')
                        valOpens--;
                    if (valOpens == 0)
                    {
                        //Add action funct
                        if (!ignore)
                        {
                            func = false;
                            buffer = buffer.Trim();
                            pos = Body.IndexOf(buffer, pos);
                            bufferBody = bufferBody.Substring(0, bufferBody.IndexOf(buffer)) + bufferBody.Substring(bufferBody.IndexOf(buffer) + buffer.Length);
                            int bufferOffset = offset + pos + buffer.IndexOf(bufferWord);
                            Action var = new Action(buffer, bufferWord, pos + offset, bufferOffset, buffer.Length);

                            parseValue(buffer, pos + offset, bufferOffset);

                            pos += buffer.Length;
                            //Children.Add(var);
                            bufferWords.Clear();
                            bufferWord = "";
                            operation = "";
                            value = "";
                            buffer = "";
                        }
                    }
                    else bufferWord += c;
                    continue;
                }
                if (bOper && operation.Length > 0)
                {
                    if (c == '(' || c == '{')
                        valOpens++;
                    else if (c == ')' || c == '}')
                        valOpens--;

                    if (valOpens == 0 && (c == ';' || bufferWord == ";"))
                    {
                        value = "";
                        if (bufferWord != ";" && bufferWords.Count > 0)
                            value = bufferWord;
                        buffer = buffer.Trim();
                        pos = Body.IndexOf(buffer, pos);
                        bufferBody = bufferBody.Substring(0, bufferBody.IndexOf(buffer)) + bufferBody.Substring(bufferBody.IndexOf(buffer) + buffer.Length);
                        int bufferOffset = offset + pos + (value.Length > 0 ? buffer.IndexOf(value) : 0);
                        Action var = new Action(buffer, value, pos + offset, bufferOffset, buffer.Length, operation);

                        if (operation != "=")
                            parseValue(buffer, pos + offset, bufferOffset);
                        else Children.Add(var);

                        pos += buffer.Length;
                        //Children.Add(var);
                        //List<string> list = AppParams.getReversePolishNotation(buffer.Replace(';', ' '));
                        bOper = false;
                        operation = "";
                        bufferWord = "";
                        bufferWords.Clear();
                        buffer = "";
                        value = "";
                    }
                    else bufferWord += c;
                    continue;
                }
                else if (bOper)
                {
                    if (AppParams.Instance.isOperation(bufferWord + c))
                    {
                        bufferWord += c;
                    }
                    else
                    {
                        //if(bufferWords.Count == 0 && (c == '*' || c == '&'))
                        if (AppParams.Instance.isIgnoreOperator(bufferWord))
                            bOper = false;
                        else operation = bufferWord;
                        bufferWord = "" + c;
                    }
                    continue;
                }
                if (AppParams.Instance.isOperation(""+c))
                {
                    if (bufferWords.Count > 0 || (bufferWord.Length > 0 && AppParams.Instance.isLink(bufferWord)) || (c != '*' && c != '&'))
                    {
                        if (bufferWord.Length > 0)
                        {
                            bufferWords.Add(bufferWord);
                            bufferWord = "";
                        }
                        bOper = true;
                    }
                    bufferWord += c;
                    continue;
                }
                switch (c)
                {
                    case ' ':
                        if(AppParams.Instance.isOperator(bufferWord))
                        {
                            bOperator = true;
                            bufferWords.Add(bufferWord);
                        }
                        else if (bufferWord == "if" || bufferWord == "for" || bufferWord == "switch" || bufferWord == "while" || bufferWord == "else" || bufferWord == "do")
                            ignore = true;
                        else if (bufferWord.Length > 0)
                            bufferWords.Add(bufferWord);
                        if (bufferWords.Count > 1)
                            ignore = true;
                        bufferWord = "";
                        break;
                    case '(':
                        if (bufferWord == "if" || bufferWord == "for" || bufferWord == "switch" || bufferWord == "while" || bufferWord == "else" || bufferWord == "do")
                            ignore = true;
                        else if (bufferWord.Length > 0)
                            bufferWords.Add(bufferWord);
                        if (!ignore)
                        {
                            valOpens++;
                        }
                        if (!ignore && bufferWords.Count == 1)
                        {
                            func = true;
                        }
                        /*if (bufferWords.Count > 1)
                            ignore = true;*/
                        bufferWord = "";
                        break;
                    case '{':
                        opens++;
                        break;
                    case ';':
                        if (AppParams.Instance.isOperator(bufferWord))
                        {
                            //bOperator = true;
                            //bufferWords.Add(bufferWord);

                            buffer = buffer.Trim();
                            pos = Body.IndexOf(buffer, pos);
                            bufferBody = bufferBody.Substring(0, bufferBody.IndexOf(buffer)) + bufferBody.Substring(bufferBody.IndexOf(buffer) + buffer.Length);
                            int bufferOffset = offset + pos;
                            Action var = new Action(buffer, "", pos + offset, bufferOffset, buffer.Length);
                            parseValue(buffer, pos + offset, bufferOffset);

                            pos += buffer.Length;
                            //Children.Add(var);


                        }
                        bufferWords.Clear();
                        bufferWord = "";
                        buffer = "";
                        break;
                    default:
                        bufferWord += c;
                        break;
                }
            }

            return bufferBody;
        }

        protected string parseVariables(string body, int offset)
        {
            List<string> mods = new List<string>();
            List<string> bufferWords = new List<string>();
            string bufferWord = "";
            string buffer = "";
            string bufferBody = body;
            int opens = 0;
            string varName = "";
            string varArgs = "";
            bool flag = false, bVal = false;
            int valOpens = 0, pos = 0;
            foreach (char c in body)
            {
                buffer += c;
                if (bVal)
                {
                    if ((c != ',' && c != ';') || valOpens > 0)
                    {
                        if (c == '(' || c == '{')
                        {
                            valOpens++;
                        }
                        else if (c == ')' || c == '}')
                        {
                            valOpens--;
                        }
                        if (c != ' ')
                            varArgs += c;
                        continue;
                    }
                    else
                    {
                        bVal = false;
                    }
                }
                if (opens > 0)
                {
                    if (c == '{')
                        opens++;
                    else if (c == '}')
                        opens--;
                    if (opens == 0)
                    {
                        bufferWords.Clear();
                        bufferWord = "";
                        varArgs = "";
                        varName = "";
                        buffer = "";
                        mods.Clear();
                        flag = false;
                    }
                    continue;
                }

                if (flag)
                {
                    if (valOpens > 0)
                    {
                        if (c == '(')
                            valOpens++;
                        else if (c == ')')
                            valOpens--;
                        continue;
                    }
                    else if (c == '(')
                    {
                        valOpens++;
                    }
                    else if (c == ';')
                    {
                        bufferWords.Clear();
                        bufferWord = "";
                        varArgs = "";
                        varName = "";
                        buffer = "";
                        mods.Clear();
                        flag = false;
                    }
                    else if (c == '{')
                    {
                        opens++;
                        flag = false;
                    }
                    continue;
                }
                switch (c)
                {
                    case ' ':
                        if (bufferWord.Length > 0)
                            bufferWords.Add(bufferWord);
                        bufferWord = "";
                        break;
                    case ';':
                        {
                            if (bufferWord.Length > 0)
                                bufferWords.Add(bufferWord);
                            if (bufferWords.Count > 1 && !bufferWords.Contains("return") && !AppParams.Instance.isOperation(bufferWords.Last()))
                            {
                                varName = bufferWords.Last();
                                bufferWords.Remove(varName);
                                
                                buffer = buffer.Trim();
                                pos = Body.IndexOf(buffer, pos);
                                int bufferOffset = offset + pos + buffer.IndexOf(varArgs);
                                Variable var = new Variable(varName, new List<string>(bufferWords), new List<string>(mods), pos + offset, bufferOffset, buffer.Length, varArgs);
                                pos += buffer.Length;
                                Children.Add(var);
                                bufferBody = bufferBody.Substring(0, bufferBody.IndexOf(buffer)) + bufferBody.Substring(bufferBody.IndexOf(buffer) + buffer.Length);
                                //Variables.Add(var);
                            }

                            bufferWords.Clear();
                            mods.Clear();
                            bufferWord = "";
                            varArgs = "";
                            varName = "";
                            buffer = "";
                            flag = false;
                            bVal = false;
                            break;
                        }
                    case '(':
                        valOpens++;
                        flag = true;
                        break;
                    case '{':
                        opens++;
                        break;
                    case ',':
                        {
                            if (bufferWord.Length > 0)
                                bufferWords.Add(bufferWord);
                            if (bufferWords.Count > 1 && !bufferWords.Contains("return") && !AppParams.Instance.isOperation(bufferWords.Last()))
                            {
                                varName = bufferWords.Last();
                                bufferWords.Remove(varName);
                                buffer = buffer.Trim();
                                pos = Body.IndexOf(buffer, pos);
                                int bufferOffset = offset + pos + buffer.IndexOf(varArgs);
                                Variable var = new Variable(varName, new List<string>(bufferWords), new List<string>(mods), pos + offset, bufferOffset, buffer.Length, varArgs);
                                pos += buffer.Length;
                                bufferBody = bufferBody.Substring(0, bufferBody.IndexOf(buffer)) + bufferBody.Substring(bufferBody.IndexOf(buffer) + buffer.Length);
                                buffer = "";
                                Children.Add(var);
                                //Variables.Add(var);
                            }
                            bufferWord = "";
                            varArgs = "";
                            varName = "";
                            mods.Clear();
                            bVal = false;
                            break;
                        }
                    case '=':
                        bVal = true;
                        break;
                    case '[':
                        if (bufferWord.Length > 0)
                            bufferWords.Add(bufferWord);
                        bufferWord = "[";
                        break;
                    case ']':
                        bufferWord += c;
                        mods.Add(bufferWord);
                        bufferWord = "";
                        break;
                    default:
                        bufferWord += c;
                        break;
                }
            }

            return bufferBody;
        }

        protected string parseIfElse(string body, int offset)
        {
            string bufferBody = body;
            string bufferWord = "";
            string buffer = "", condition = "";
            List<string> conditions = new List<string>();
            List<string> bodies = new List<string>();
            int opens = 0, valOpens = 0, pos = 0;
            bool bParseCondition = false, flag = false, bParseBody = false, bVal = false, bParseElse = false;

            foreach (char c in body)
            {
                buffer += c;
                if (opens > 0)
                {
                    if (c == '{')
                        opens++;
                    else if (c == '}')
                        opens--;
                    if (opens == 0)
                    {
                        if (bParseBody)
                        {
                            bodies.Add(bufferWord);
                            bParseElse = true;

                            buffer = buffer.Trim();
                            pos = Body.IndexOf(buffer, pos);
                            int bufferOffset = offset + pos + buffer.IndexOf(bufferWord);
                            IfElseConstr var = new IfElseConstr(condition, bufferWord, pos + offset, bufferOffset, buffer.IndexOf(bufferWord));
                            var.CommonLength = buffer.Length;
                            bufferBody = bufferBody.Substring(0, bufferBody.IndexOf(buffer)) + bufferBody.Substring(bufferBody.IndexOf(buffer) + buffer.Length);
                            Children.Add(var);
                            pos += buffer.Length;
                            condition = "";
                            buffer = "";

                            //bufferBody = bufferBody.Substring(0, bufferBody.IndexOf(bufferWord)) + bufferBody.Substring(bufferBody.IndexOf(bufferWord) + bufferWord.Length);
                        }
                        bufferWord = "";
                        bParseBody = false;
                    }
                    else bufferWord += c;
                    continue;
                }
                if (valOpens > 0 && bParseCondition)
                {
                    if (c == '(')
                        valOpens++;
                    else if (c == ')')
                        valOpens--;
                    if (valOpens == 0)
                    {
                        conditions.Add(bufferWord);
                        condition = bufferWord;
                        bufferWord = "";
                        bParseCondition = false;
                        bParseBody = true;
                    }
                    else bufferWord += c;
                    continue;
                }
                else if (bParseBody)
                {
                    if (c == '{' && bufferWord.Length == 0)
                    {
                        opens++;
                        continue;
                    }
                    if (bParseElse && (c == ' ' || c == '(') && bufferWord == "if")
                    {
                        flag = true;
                        bParseElse = false;
                        bufferWord = "";
                    }
                    if (flag && c == '(')
                    {
                        valOpens++;
                        flag = false;
                        bParseCondition = true;
                        bufferWord = "";
                        continue;
                    }
                    else if (c == '(' || c == '{' || c == '[')
                        valOpens++;
                    else if (c == ')' || c == '}' || c == ']')
                        valOpens--;
                    else if (valOpens == 0 && c == '=')
                        bVal = true;

                    if ((c == ',' || c == ';') && bVal && valOpens == 0)
                    {
                        bVal = false;
                    }

                    if (valOpens == 0 && (c == ';' || (!bVal && c == '}')))
                    {
                        // Adding to array
                        if (c == ';')
                            bufferWord += c;
                        bodies.Add(bufferWord);

                        buffer = buffer.Trim();
                        pos = Body.IndexOf(buffer, pos);
                        int bufferOffset = offset + pos + buffer.IndexOf(bufferWord);
                        IfElseConstr var = new IfElseConstr(condition, bufferWord, pos + offset, bufferOffset, buffer.IndexOf(bufferWord));
                        var.CommonLength = buffer.Length;
                        bufferBody = bufferBody.Substring(0, bufferBody.IndexOf(buffer)) + bufferBody.Substring(bufferBody.IndexOf(buffer) + buffer.Length);
                        Children.Add(var);
                        pos += buffer.Length;
                        condition = "";
                        buffer = "";

                        //bufferBody = bufferBody.Substring(0, bufferBody.IndexOf(bufferWord)) + "{ }" + bufferBody.Substring(bufferBody.IndexOf(bufferWord) + bufferWord.Length);
                        bParseElse = true;
                        bParseBody = false;
                        bufferWord = "";
                    }

                    else if ((c != ' ' && c != '\r' && c != '\n' && c != '\t') || bufferWord.Length > 0)
                        bufferWord += c;
                    continue;
                }
                else if (bParseElse)
                {
                    if ((c == ' ' && bufferWord.Length > 0) || c == ';' || c == '{' || c == '(' || c == '}')
                    {
                        if (bufferWord == "else")
                        {
                            bParseBody = true;
                            buffer = bufferWord + c;
                        }
                        else if (bufferWord == "if")
                        {
                            flag = true;
                            buffer = bufferWord + c;
                        }
                        else
                        {
                            bParseElse = false;
                            /*buffer = buffer.Trim();
                            pos = Body.IndexOf(buffer, pos);
                            int bufferOffset = offset + pos + buffer.IndexOf(bodies.First());
                            IfElseConstr var = new IfElseConstr(new List<string>(conditions), new List<string>(bodies), pos + offset, bufferOffset, buffer.Length);
                            bufferBody = bufferBody.Substring(0, bufferBody.IndexOf(buffer)) + bufferBody.Substring(bufferBody.IndexOf(buffer) + buffer.Length);
                            Children.Add(var);
                            pos += buffer.Length;
                             * */
                            conditions.Clear();
                            bodies.Clear();
                        }
                        if (c == '{')
                            opens++;
                        else if (c == '(' && flag)
                        {
                            bParseCondition = true;
                            valOpens++;
                        }
                        bufferWord = "";
                    }
                    else if(c!= ' ') bufferWord += c;
                    continue;
                }
                switch (c)
                {
                    case ';':
                    case ' ':
                        if (bufferWord == "if")
                        {
                            flag = true;
                            buffer = bufferWord + c;
                        }
                        bufferWord = "";
                        break;
                    case '(':
                        if (bufferWord == "if")
                        {
                            flag = true;
                            buffer = bufferWord + c;
                        }
                        if (flag)
                        {
                            bParseCondition = true;
                            valOpens++;
                            flag = false;
                        }
                        bufferWord = "";
                        break;
                    case '{':
                        bufferWord = "";
                        opens++;
                        break;
                    default:
                        bufferWord += c;
                        break;
                }
            }
            /*if (conditions.Count > 0 && bodies.Count > 0)
            {
                buffer = buffer.Trim();
                pos = Body.IndexOf(buffer, pos);
                int bufferOffset = offset + pos + buffer.IndexOf(bodies.First());
                IfElseConstr var = new IfElseConstr(new List<string>(conditions), new List<string>(bodies), pos + offset, bufferOffset, buffer.Length);
                var.Position = pos;
                bufferBody = bufferBody.Substring(0, bufferBody.IndexOf(buffer)) + bufferBody.Substring(bufferBody.IndexOf(buffer) + buffer.Length);
                Children.Add(var);
                pos += buffer.Length;
                conditions.Clear();
                bodies.Clear();
            }
             * */
            return bufferBody;
        }

        protected string parseDoWhile(string inBody, int offset)
        {
            string bufferBody = inBody;
            string bufferWord = "";
            string buffer = "";
            string condition = "", body = "";
            int opens = 0, valOpens = 0, pos = 0;
            bool flag = false, bParseCondition = false, bVal = false;
            foreach (char c in inBody)
            {
                buffer += c;
                if (opens > 0)
                {
                    if (c == '{')
                        opens++;
                    else if (c == '}')
                        opens--;

                    if (opens == 0)
                    {
                        if (flag)
                        {
                            body = bufferWord;
                            bParseCondition = true;
                            //bufferBody = bufferBody.Substring(0, bufferBody.IndexOf(body)) + bufferBody.Substring(bufferBody.IndexOf(body) + body.Length);
                        }
                        bufferWord = "";
                    }
                    else bufferWord += c;
                    continue;
                }

                else if (flag && body.Length == 0)
                {
                    if (c == '{' && bufferWord.Length == 0)
                    {
                        opens++;
                        continue;
                    }

                    if (c == '(' || c == '{' || c == '[')
                        valOpens++;
                    else if (c == ')' || c == '}' || c == ']')
                        valOpens--;
                    else if (valOpens == 0 && c == '=')
                        bVal = true;

                    if ((c == ',' || c == ';') && bVal && valOpens == 0)
                    {
                        bVal = false;
                    }

                    if (valOpens == 0 && (c == ';' || (!bVal && c == '}')))
                    {
                        // Adding to array
                        if (c == ';')
                            bufferWord += c;
                        if (bufferWord.Length > 0)
                        {
                            body = bufferWord;
                            //bufferBody = bufferBody.Substring(0, bufferBody.IndexOf(bufferWord)) + "{ }" + bufferBody.Substring(bufferBody.IndexOf(bufferWord) + bufferWord.Length);
                        }
                        bufferWord = "";
                        condition = "";
                    }

                    else if ((c != ' ' && c != '\r' && c != '\n' && c != '\t') || bufferWord.Length > 0)
                        bufferWord += c;
                    continue;
                }
                else if (valOpens > 0 && bParseCondition)
                {
                    if (c == '(' || c == '{')
                        valOpens++;
                    else if (c == ')' || c == '}')
                        valOpens--;
                    if (valOpens == 0)
                    {
                        condition = bufferWord;
                        buffer = buffer.Trim();
                        pos = Body.IndexOf(buffer, pos);
                        int bufferOffset = offset + pos + buffer.IndexOf(body);
                        DoWhileConstr var = new DoWhileConstr(condition, body, pos + offset, bufferOffset, buffer.IndexOf(body));
                        var.CommonLength = buffer.Length;
                        Children.Add(var);
                        pos += buffer.Length;
                        bufferBody = bufferBody.Substring(0, bufferBody.IndexOf(buffer)) + bufferBody.Substring(bufferBody.IndexOf(buffer) + buffer.Length + 1);
                        condition = "";
                        body = "";
                        bufferWord = "";
                        bParseCondition = false;
                        flag = false;
                    }
                    else bufferWord += c;
                    continue;
                }
                switch (c)
                {
                    case ';':
                    case ' ':
                        if (bufferWord == "do")
                        {
                            flag = true;
                            body = "";
                            condition = "";
                            buffer = bufferWord + c;
                        }
                        else if (flag && bufferWord == "while")
                        {
                            bParseCondition = true;
                            condition = "";
                        }
                        bufferWord = "";
                        break;
                    case '{':
                        if (bufferWord == "do")
                        {
                            flag = true;
                            body = "";
                            condition = "";
                            buffer = bufferWord + c;
                        }
                        bufferWord = "";
                        opens++;
                        break;
                    case '(':
                        if (flag && bufferWord == "while")
                        {
                            bParseCondition = true;
                            condition = "";
                        }
                        if (bParseCondition)
                        {
                            valOpens++;
                        }
                        break;
                    default:
                        bufferWord += c;
                        break;
                }
            }

            return bufferBody;
        }

        protected string parseForWhile(string body, int offset)
        {
            string name = "";
            string bufferBody = body;
            string buffer = "";
            string bufferWord = "";
            string condition = "";
            int opens = 0, valOpens = 0;
            int pos = 0;
            bool flag = false, bParseBody = false, bVal = false;

            foreach (char c in body)
            {
                buffer += c;
                if (opens > 0)
                {
                    if (c == '{')
                        opens++;
                    else if (c == '}')
                        opens--;
                    if (opens == 0)
                    {
                        if (bParseBody)
                        {
                            // Adding to array
                            if (bufferWord.Length > 0)
                            {
                                SystemConstruction var = null;
                                buffer = buffer.Trim();
                                pos = Body.IndexOf(buffer, pos);
                                int bufferOffset = offset + pos + buffer.IndexOf(bufferWord);
                                if (name == "for")
                                    var = new ForConstr(condition, bufferWord, pos + offset, bufferOffset, buffer.IndexOf(bufferWord));
                                else if (name == "while")
                                    var = new WhileConstr(condition, bufferWord, pos + offset, bufferOffset, buffer.IndexOf(bufferWord));
                                else if (name == "switch")
                                    var = new SwitchConstr(condition, bufferWord, pos + offset, bufferOffset, buffer.IndexOf(bufferWord) - 1);
                                var.CommonLength = buffer.Length;
                                Children.Add(var);
                                pos += buffer.Length;
                                bufferBody = bufferBody.Substring(0, bufferBody.IndexOf(buffer)) + bufferBody.Substring(bufferBody.IndexOf(buffer) + buffer.Length);
                            }
                            flag = false;
                            bParseBody = false;
                            valOpens = 0;
                        }
                        buffer = "";
                        bufferWord = "";
                        condition = "";
                    }
                    else bufferWord += c;
                    continue;
                }
                else if (valOpens > 0 && !bParseBody)
                {
                    if (c == '(' || c == '{')
                        valOpens++;
                    else if (c == ')' || c == '}')
                        valOpens--;
                    if (valOpens == 0)
                    {
                        condition = bufferWord;
                        bParseBody = true;
                        bufferWord = "";
                    }
                    else bufferWord += c;
                    continue;
                }
                else if (bParseBody)
                {
                    if (c == '{' && bufferWord.Length == 0)
                    {
                        opens++;
                        continue;
                    }

                    if (c == '(' || c == '{' || c == '[')
                        valOpens++;
                    else if (c == ')' || c == '}' || c == ']')
                        valOpens--;
                    else if (valOpens == 0 && c == '=')
                        bVal = true;

                    if ((c == ',' || c == ';') && bVal && valOpens == 0)
                    {
                        bVal = false;
                    }

                    if (valOpens == 0 && (c == ';' || (!bVal && c == '}')))
                    {
                        if (c == ';')
                        {
                            bufferWord += c;
                        }
                        // Adding to array
                        if (bufferWord.Length > 0)
                        {
                            SystemConstruction var = null;
                            buffer = buffer.Trim();
                            pos = Body.IndexOf(buffer, pos);
                            int bufferOffset = offset + pos + buffer.IndexOf(bufferWord);
                            if (name == "for")
                                var = new ForConstr(condition, bufferWord, pos + offset, bufferOffset, buffer.IndexOf(bufferWord));
                            else if (name == "while")
                                var = new WhileConstr(condition, bufferWord, pos + offset, bufferOffset, buffer.IndexOf(bufferWord));
                            else if (name == "switch")
                                var = new SwitchConstr(condition, bufferWord, pos + offset, bufferOffset, buffer.IndexOf(bufferWord));
                            var.CommonLength = buffer.Length;
                            pos += buffer.Length;
                            Children.Add(var);

                            bufferBody = bufferBody.Substring(0, bufferBody.IndexOf(buffer)) + bufferBody.Substring(bufferBody.IndexOf(buffer) + buffer.Length);
                        }
                        flag = false;
                        bParseBody = false;
                        bufferWord = "";
                        condition = "";
                        buffer = "";
                    }
                    else if ((c != ' ' && c != '\r' && c != '\n' && c != '\t') || bufferWord.Length > 0)
                        bufferWord += c;
                    continue;
                }

                switch (c)
                {
                    case ' ':
                        if (bufferWord == "for" || bufferWord == "while" || bufferWord == "switch")
                        {
                            name = bufferWord;
                            flag = true;
                            condition = "";
                            buffer = bufferWord + c;
                        }
                        bufferWord = "";
                        break;
                    case '(':
                        if (bufferWord == "for" || bufferWord == "while" || bufferWord == "switch")
                        {
                            name = bufferWord;
                            flag = true;
                            condition = "";
                            buffer = bufferWord + c;
                            valOpens++;
                        }
                        else if (flag)
                        {
                            valOpens++;
                        }
                        bufferWord = "";
                        break;
                    case ';':
                        bufferWord = "";
                        break;
                    case '{':
                        opens++;
                        bufferWord = "";
                        break;
                    default:
                        bufferWord += c;
                        break;

                }
            }

            return bufferBody;
        }

        public int CompareTo(CodeObjectInterface compare)
        {
            // A null value means that this object is greater.
            if (compare == null)
                return 1;
            else
                return this.Position.CompareTo(compare.Position);
        }

        protected virtual void generateToken()
        {
            token = "" + (char)Type;
            foreach(CodeObjectInterface obj in Children)
            {
                token += obj.Token;
            }
        }

        public void RecoveryPositions(ref List<Coord> replaced)
        {
            CodeObjectInterface obj = this;
            int offset = 0;
            int length = obj.Length;
            List<Coord> buffer = replaced.FindAll(x => x.newPos <= obj.Position);
            foreach (Coord coord in buffer)
            {
                offset += coord.length;
            }

            buffer = replaced.FindAll(x => x.newPos > obj.Position && x.newPos < obj.Position + obj.Length);
            foreach (Coord coord in buffer)
            {
                length += coord.length;
            }
            obj.Length = length;

            length = obj.CommonLength;
            buffer = replaced.FindAll(x => x.newPos > obj.Position && x.newPos < obj.Position + obj.CommonLength);

            foreach (Coord coord in buffer)
            {
                length += coord.length;
            }

            obj.Position += offset;
            obj.CommonLength = length;
            foreach (CodeObjectInterface o in Children)
                o.RecoveryPositions(ref replaced);
        }


        protected void parseValue(string body, int position,  int offset)
        {
            body = body.Replace(';', ' ');
            List<string> polishNotation = AppParams.getReversePolishNotation(body);
            Stack<string> bufferStack = new Stack<string>();
            Stack<CodeObjectInterface> actionStack = new Stack<CodeObjectInterface>();

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
                Action meth = new Action(polishNotation[0], value, position, offset, body.Length);
                Children.Add(meth);
                return;
            }
            else if(polishNotation.Count == 1 && AppParams.Instance.isOperator(polishNotation[0]))
            {
                Action meth = new Action(polishNotation[0], "", position, offset, body.Length, polishNotation[0]);
                Children.Add(meth);
                return;
            }

            foreach (string str in polishNotation)
            {
                if (AppParams.Instance.isOperation(str))
                {
                    string left = "", right = "";
                    right = bufferStack.Pop();
                    if(bufferStack.Count > 0)
                        left = bufferStack.Pop();

                    string val = "";
                    if (left.Length > 1)
                        val += "(" + left + ")";
                    else val += left;

                    val += str;

                    if (right.Length > 1)
                        val += "(" + right + ")";
                    else val += right;

                    Action var = new Action(val, "", position, offset, body.Length, str);

                    if (left.ToList().FindAll(x => x == '(').Count == 1)
                    {
                        string bufferWord = "", value = "";
                        int opens = 0;
                        foreach (char c in left)
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
                        Action meth = new Action(left, value, position, offset, body.Length);
                        var.Children.Add(meth);
                        //Children.Add(meth);
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
                        Action meth = new Action(right, value, position, offset, body.Length);
                        var.Children.Add(meth);
                        //Children.Add(meth);
                    }
                    if (actionStack.Count > 0)
                    {
                        if(actionStack.Peek() != null)
                            var.Children.Add(actionStack.Peek());
                        actionStack.Pop();
                    }
                    if (actionStack.Count > 0)
                    {
                        if(actionStack.Peek() != null)
                            var.Children.Add(actionStack.Peek());
                        actionStack.Pop();
                    }
                    actionStack.Push(var);
                    //Children.Add(var);
                    bufferStack.Push(val);
                }
                else
                {
                    bufferStack.Push(str);
                    actionStack.Push(null);
                }
            }
            if (bufferStack.Count == 2)
            {
                //Console.WriteLine(polishNotation[0]);
                if (actionStack.Count > 0 && actionStack.Peek() == null)
                    actionStack.Pop();


                string value = bufferStack.Pop();

                string Oper = bufferStack.Pop();
                Action meth = new Action(Oper + " " + value, "", position, offset, body.Length, Oper);
                if (actionStack.Count > 0 && actionStack.Peek() != null)
                    meth.Children.Add(actionStack.Pop());

                actionStack.Push(meth);
            }
            /*if (actionStack.Peek() == null)
                Console.WriteLine(body);*/

            if (actionStack.Count > 0 && actionStack.Peek() != null)
                Children.Add(actionStack.Pop());
        }

        //public void Reversal(ref int offset, ref List<int> indexes, ref List<>)
    }

    public class Defenition : CodeObjectInterface
    {

        public Defenition()
        {
        }
    }

    public class Variable : CodeObjectInterface
    {
        protected string Value;
        protected List<string> Types = new List<string>();
        public Variable(string name, List<string> types, List<string> mods, int pos = 0, int bufferOffset = 0, int length = 0, string value = "")
        {
            Type = ObjectType.Variable;
            Name = name;
            Types = types;
            Modificators = mods;
            Value = value;
            Position = pos;
            Length = length;
            CommonLength = length;
            if(Value.Length > 0)
            {
                Children.Add(new Action(name + " = " + value, value, pos, bufferOffset, length, "="));
            }
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


        public override CustomTreeNode Node
        {
            get
            {
                string val = "";

                foreach(string str in Types)
                {
                    val += str + " ";
                }
                val += Name;
                foreach(string str in Modificators)
                {
                    val += str;
                }
                CustomTreeNode root = new CustomTreeNode(val, this);

                foreach (CodeObjectInterface obj in Children)
                {
                    root.Nodes.Add(obj.Node);
                }
                return root;
            }
        }

    }


}
