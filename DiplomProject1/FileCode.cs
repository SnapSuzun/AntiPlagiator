using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DiplomProject1
{
    public class Coord : IComparable<Coord>
    {
        public int pos = 0, length = 0, newPos = 0;

        public Coord(int _pos, int _length, int _newPos = 0)
        {
            pos = _pos;
            length = _length;
            newPos = _newPos;
        }

        public Coord() { }

        public int CompareTo(Coord compare)
        {
            if (compare == null)
                return 1;
            return this.pos.CompareTo(compare.pos);
        }
    }

    class FileCode
    {
        protected List<CodeObjectInterface> objects = new List<CodeObjectInterface>();
        protected string filename = "";
        protected string body = "";
        public string bodyClear = "";
        protected string token = "";
        protected List<CodeObjectInterface> recursiveChildrenList;

        protected List<Coord> replaced = new List<Coord>();

        public List<CustomTreeNode> Node
        {
            get
            {
                List<CustomTreeNode> nodes = new List<CustomTreeNode>();
                TreeNode root = new TreeNode(filename);
                foreach(CodeObjectInterface obj in objects)
                {
                    nodes.Add(obj.Node);
                }

                return nodes;
            }
        }

        public string Token
        {
            get
            {
                if (this.token.Length > 0)
                    return this.token;
                token = "";
                foreach(CodeObjectInterface obj in objects)
                {
                    token += obj.Token;
                }
                return token;
            }
        }

        public List<CodeObjectInterface> Objects
        {
            get
            {
                return objects;
            }
        }

        public List<CodeObjectInterface> RecursiveChildrenList
        {
            get
            {
                if (recursiveChildrenList != null)
                    return recursiveChildrenList;

                recursiveChildrenList = new List<CodeObjectInterface>();
                foreach (CodeObjectInterface obj in objects)
                {
                    recursiveChildrenList.AddRange(obj.RecursiveChildrenList);
                }

                foreach (CodeObjectInterface obj in recursiveChildrenList)
                {
                    RecoveryPosition(obj);
                }

                return recursiveChildrenList;
            }
        }

        protected void RecoveryPositions()
        {
            /*recursiveChildrenList = new List<CodeObjectInterface>();
            foreach (CodeObjectInterface obj in objects)
            {
                recursiveChildrenList.AddRange(obj.RecursiveChildrenList);
            }

            foreach (CodeObjectInterface obj in recursiveChildrenList)
            {
                RecoveryPosition(obj);
            }
            List<CodeObjectInterface> find = objects;
            foreach(CodeObjectInterface obj in find)
            {
                if(RecursiveChildrenList.IndexOf(obj) < 0)
                    RecoveryPosition(obj);
            }*/

            foreach (CodeObjectInterface obj in objects)
            {
                obj.RecoveryPositions(ref replaced);
            }
        }

        protected void RecoveryPosition(CodeObjectInterface obj)
        {
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

            obj.Position += offset;
            obj.Length = length;
        }

        public string FileName
        {
            get { return filename; }
            set {
                filename = value;
                Reset();
                loadFileBody();
            }
        }
        public string Body
        {
            get { return body; }
        }
        public FileCode(string filename)
        {
            if(File.Exists(filename))
            {
                this.filename = filename;
                loadFileBody();
            }
        }

        protected void loadFileBody()
        {
            if (File.Exists(filename))
            {
                body = File.ReadAllText(filename);
                body = body.Replace("\r", "");
                parseBody(body);
                RecoveryPositions();
            }
        }

        protected string parseBody(string body)
        {
            body = body.Replace("\t", " ");
            bodyClear = body;
            body = removeComments(body);
            body = removePreprocessors(body);
            body = clearStrings(body);
            body = body.Replace("\n", " ");
            bodyClear = body;

            replaced.Sort();
            for (int i = 0; i < replaced.Count; i++)
            {
                replaced[i].newPos = replaced[i].pos;
                for (int j = 0; j < i; j++)
                {
                    replaced[i].newPos -= replaced[j].length;
                }
            }

            body = findFunctions(body);
            body = findVariables(body);

            objects.Sort();
            return body;
        }

        protected string findVariables(string body)
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
            int valOpens=0, pos = 0;
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
                        if(c != ' ')
                            varArgs += c;
                        continue;
                    }
                    else bVal = false;
                }
                if (opens > 0)
                {
                    if (c == '{')
                        opens++;
                    else if (c == '}')
                        opens--;
                    if(opens == 0)
                    {
                        bufferWords.Clear();
                        bufferWord = "";
                        varArgs = "";
                        varName = "";
                        mods.Clear();
                        flag = false;
                    }
                    continue;
                }

                if(flag)
                {
                    if (c == ';')
                    {
                        bufferWords.Clear();
                        bufferWord = "";
                        varArgs = "";
                        varName = "";
                        buffer = "";
                        mods.Clear();
                        flag = false;
                    }
                    else if(c == '{')
                    {
                        opens++;
                        flag = false;
                    }
                    continue;
                }
                switch(c)
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
                            if (bufferWords.Count > 1)
                            {
                                varName = bufferWords.Last();
                                bufferWords.Remove(varName);
                                buffer = buffer.Trim();
                                pos = bodyClear.IndexOf(buffer, pos);

                                int offset = 0;

                                /*foreach(Coord coord in replaced)
                                {
                                    if (coord.newPos <= pos)
                                    {
                                        offset += coord.length;
                                    }
                                    else break;
                                }*/

                                int bufferOffset =offset + pos + buffer.IndexOf(varArgs);
                                Variable var = new Variable(varName, new List<string>(bufferWords), new List<string>(mods), pos + offset, bufferOffset, buffer.Length, varArgs);
                                pos += buffer.Length;
                                bufferBody = bufferBody.Substring(0, bufferBody.IndexOf(buffer)) + bufferBody.Substring(bufferBody.IndexOf(buffer) + buffer.Length);
                                objects.Add(var);
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
                        flag = true;
                        break;
                    case '{':
                        opens++;
                        break;
                    case ',':
                        {
                            if (bufferWord.Length > 0)
                                bufferWords.Add(bufferWord);
                            if (bufferWords.Count > 1)
                            {
                                varName = bufferWords.Last();
                                bufferWords.Remove(varName);
                                buffer = buffer.Trim();
                                pos = bodyClear.IndexOf(buffer, pos);

                                int offset = 0;

                                /*foreach (Coord coord in replaced)
                                {
                                    if (coord.newPos <= pos)
                                    {
                                        offset += coord.length;
                                    }
                                    else break;
                                }*/

                                int bufferOffset = offset + pos + buffer.IndexOf(varArgs);
                                Variable var = new Variable(varName, new List<string>(bufferWords), new List<string>(mods), pos + offset, bufferOffset, buffer.Length, varArgs);
                                pos += buffer.Length;
                                bufferBody = bufferBody.Substring(0, bufferBody.IndexOf(buffer)) + bufferBody.Substring(bufferBody.IndexOf(buffer) + buffer.Length);
                                objects.Add(var);
                                buffer = "";
                                bufferWord = "";
                            }
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

        protected string findFunctions(string body)
        {
            List<string> bufferWords = new List<string>();
            string bufferWord = "";
            string bufferBody = body;
            string funcBody = "";
            int opens = 0;
            bool flag = false, bIgnore = false;
            string args = "";
            string funcName = "";
            string buffer = "";
            int pos = 0;
            foreach(char c in body)
            {
                buffer += c;
                if(opens > 0)
                {
                    if (c == '{')
                        opens++;
                    else if (c == '}')
                        opens--;
                    if(opens == 0)
                    {
                        if (!bIgnore)
                        {
                            buffer = buffer.Trim();
                            pos = bodyClear.IndexOf(buffer, pos);

                            int offset = 0;

                            /*foreach (Coord coord in replaced)
                            {
                                if (coord.newPos <= pos)
                                {
                                    offset += coord.length;
                                }
                                else break;
                            }*/

                            int bufferOffset = offset + pos + buffer.IndexOf(funcBody);
                            Method func = new Method(funcName, funcBody, new List<string>(args.Split(',')), bufferWords, pos + offset, bufferOffset, buffer.Length);
                            pos += buffer.Length;
                            int index = bufferBody.IndexOf(buffer);
                            bufferBody = bufferBody.Substring(0, index) + bufferBody.Substring(bufferBody.IndexOf(buffer) + buffer.Length);
                            objects.Add(func);
                        }
                        bufferWords.Clear();
                        args = "";
                        funcName = "";
                        funcBody = "";
                        bufferWord = "";
                        buffer = "";
                        flag = false;
                        bIgnore = false;
                    }
                    else funcBody += c;
                    continue;
                }
                if(flag)
                {
                    if (c == ')')
                    {
                        flag = false;
                    }
                    else args += c;
                    continue;
                }
                switch(c)
                {
                    case ' ':
                        if (bufferWord.Length > 0)
                        {
                            bufferWords.Add(bufferWord);
                            //buffer = bufferWord + c;
                        }
                        bufferWord = "";
                        break;
                    case ';':
                        bufferWords.Clear();
                        bufferWord = "";
                        args = "";
                        funcBody = "";
                        funcName = "";
                        buffer = "";
                        bIgnore = false;
                        break;
                    case '(':
                        if (bufferWord.Length > 0)
                        {
                            //buffer = bufferWord + c;
                            bufferWords.Add(bufferWord);
                        }
                        bufferWord = ""; 
                        funcName = bufferWords.Last();
                        bufferWords.Remove(funcName);
                        flag = true;
                        break;
                    case '{':
                        if (bufferWord.Length > 0)
                            bufferWords.Add(bufferWord);
                        bufferWord = "";
                        funcBody = "";
                        opens++;
                        break;
                    case '=':
                        bIgnore = true;
                        if (bufferWord.Length > 0)
                            bufferWords.Add(bufferWord);
                        bufferWord = "";
                        break;
                    default:
                        bufferWord += c;
                        break;
                }
            }
            return bufferBody;
        }

        protected string clearStrings(string body)
        {
            string[] rows = body.Split('\n');
            string bufferBody = "";
            bool flag = false;
            int pos = 0;
            string buffer = "";

            for (int i = 0; i < rows.Count(); i++)
            {
                bool empty = rows[i].Length == 0;
                int n = -1, k = -1;
                while((n = rows[i].IndexOf('"')) >= 0)
                {
                    if(flag)
                    {
                        if (k != -1)
                        {
                            buffer = buffer.Substring(0, n);
                        }
                        else
                        {
                            buffer += rows[i].Substring(0, n);
                        }
                        if (buffer.Length > 0)
                        {
                            pos = bodyClear.IndexOf(buffer, pos);
                            Coord c = new Coord();
                            c.pos = pos;
                            c.length = buffer.Length;
                            replaced.Add(c);
                            pos += buffer.Length;
                            buffer = "";
                        }

                        rows[i] = rows[i].Substring(n+1);
                        bufferBody += '"';
                        flag = false;
                    }
                    else
                    {
                        bufferBody += rows[i].Substring(0, n + 1);
                        rows[i] = rows[i].Substring(n + 1);
                        buffer += rows[i] + "\n";
                        flag = true;
                    }
                    k = n;
                }
                if (n < 0 && flag)
                {
                    buffer += rows[i] + "\n";
                }
                if ((rows[i].Length > 0 || empty) && !flag)
                    bufferBody += rows[i] + "\n";
            }
            return bufferBody;
        }

        protected string removePreprocessors(string body)
        {
            string[] rows = body.Split('\n');
            string bufferBody = "";
            int pos = 0;
            foreach(string str in rows)
            {
                if(str.Length > 0 && str[0] == '#')
                {
                    string buffer = str + "\n";
                    pos = bodyClear.IndexOf(buffer, pos);
                    Coord c = new Coord(pos, buffer.Length);
                    replaced.Add(c);
                    pos += buffer.Length;
                }
                else bufferBody += str + "\n";
            }

            return bufferBody;
        }

        protected string removeComments(string body)
        {
            string[] rows = body.Split('\n');
            string bufferBody = "";
            bool flag = false;
            int pos = 0;
            string bufferStr = "";
            for (int i = 0; i < rows.Count(); i++)
            {
                bool empty = rows[i].Length == 0;
                int n = 0;
                if (flag)
                {
                    n = rows[i].IndexOf("*/");
                    if (n >= 0)
                    {
                        bufferStr += rows[i].Substring(0, n + 2);
                        rows[i] = rows[i].Substring(n + 2);
                        if (rows[i].Length == 0)
                            bufferStr += "\n";
                        pos = bodyClear.IndexOf(bufferStr, pos);
                        Coord c = new Coord();
                        c.pos = pos;
                        c.length = bufferStr.Length;
                        replaced.Add(c);
                        pos += bufferStr.Length;
                        flag = false;
                    }
                    else
                    {
                        bufferStr += rows[i] + "\n";
                        continue;
                    }

                }

                int n1 = rows[i].IndexOf("//");

                if ((n = rows[i].IndexOf("/*")) >= 0 && (n1 < 0 || n < n1))
                {
                    do
                    {
                        string buffer = rows[i].Substring(0, n);
                        int k = n;
                        n = rows[i].IndexOf("*/");
                        if (n >= 0)
                        {
                            bufferStr = rows[i].Substring(k, (n - k) + 2);
                            pos = bodyClear.IndexOf(bufferStr, pos);
                            Coord c = new Coord();
                            c.pos = pos;
                            c.length = bufferStr.Length;
                            replaced.Add(c);
                            pos += bufferStr.Length;
                            buffer += rows[i].Substring(n + 2);
                        }
                        else
                        {
                            flag = true;
                            bufferStr = rows[i].Substring(k);
                        }
                        rows[i] = buffer;
                    } while ((n = rows[i].IndexOf("/*")) >= 0 && !flag);
                    if (flag)
                        bufferStr += "\n";
                }
                if ((n = rows[i].IndexOf("//")) >= 0)
                {
                    string str = rows[i].Substring(n);
                    rows[i] = rows[i].Substring(0, n);
                    if (rows[i].Length == 0)
                        str += "\n";
                    pos = bodyClear.IndexOf(str, pos);
                    Coord c = new Coord();
                    c.pos = pos;
                    c.length = str.Length;
                    replaced.Add(c);
                    pos += str.Length;
                }
                if (rows[i].Length > 0 || empty)
                {
                    bufferBody += rows[i];
                    if (!flag) bufferBody += '\n';
                }
            }
            return bufferBody;
        }

        protected void Reset()
        {
            body = "";
        }
    }
}
