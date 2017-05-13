using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;

namespace DiplomProject1
{
    public class AppParams
    {
        public List<string> operators;
        public List<string> operations;
        public List<string> ignoreOperators;
        protected string FileName;
        private static readonly AppParams instance = new AppParams();
        public static AppParams Instance
        {
            get
            {
                return instance;
            }
        }

        private AppParams() {
            loadSettingsFromFile("SettingsC++.xml");
        }

        private AppParams(string filename)
        {
            loadSettingsFromFile(filename);
        }

        public bool loadSettingsFromFile(string filename)
        {
            if (!File.Exists(filename))
                return false;
            FileName = filename;
            string xmlString = File.ReadAllText(filename);
            readParams(xmlString, "Operators", ref operators);
            readParams(xmlString, "Operations", ref operations);
            readParams(xmlString, "IgnoreOperators", ref ignoreOperators);
            return true;
        }

        private void readParams(string xmlString, string param, ref List<string> dest)
        {
            if (dest == null) dest = new List<string>();
            var doc = new XmlDocument();
            doc.LoadXml(xmlString);
            XmlNode root = doc.SelectNodes("ROOT")[0];
            foreach (XmlNode node in root.SelectNodes(param))
            {
                foreach (XmlNode child in node.ChildNodes)
                    dest.Add(child.InnerText);
            }
        }

        public bool isOperation(string oper)
        {
            return operations.IndexOf(oper) >= 0;
        }

        public bool isOperator(string str)
        {
            return operators.IndexOf(str) >= 0;
        }

        public bool isIgnoreOperator(string str)
        {
            return ignoreOperators.IndexOf(str) >= 0;
        }

        public bool isLink(string str)
        {
            foreach(char c in str)
            {
                if (c != '*' && c != '&')
                    return false;
            }
            return true;
        }

        public int getOperationIndex(string oper)
        {
            int pos = operations.IndexOf(oper);
            if (pos >= 0) return Enum.GetNames(typeof(ObjectType)).Length + pos;
            pos = operators.IndexOf(oper);
            if(pos >= 0) return Enum.GetNames(typeof(ObjectType)).Length + operations.Count + pos;
            return pos;
        }

        public static List<string> getReversePolishNotation(string input)
        {
            Stack<string> operatorsStack = new Stack<string>();
            List<string> output = new List<string>();

            string bufferWord = "";
            int opens = 0;
            bool bOper = false, bClose = false, bClosed = false;
            string operation = "", value = "";

            foreach(char c in input)
            {
                if(opens > 0)
                {
                    bufferWord += c;
                    if (c == '(' || c == '{')
                        opens++;
                    else if (c == ')' || c == '}')
                        opens--;

                    if (opens == 0)
                    {
                        if (bClosed)
                        {
                            bClosed = false;
                            output.RemoveAt(output.Count - 1);
                        }
                        //output.Add(bufferWord);
                        //bufferWord = "";
                    }
                    continue;
                }
                if (bOper)
                {
                    if(instance.isOperation(operation+c))
                    {
                        operation += c;
                    }
                    else
                    {
                        if (instance.isIgnoreOperator(operation))
                        {
                            bufferWord += operation;
                        }
                        else
                        {
                            if(bufferWord.Length > 0)
                            {
                                if (bClosed)
                                {
                                    bClosed = false;
                                    output.RemoveAt(output.Count - 1);
                                }
                                output.Add(bufferWord);
                                bufferWord = "";
                            }
                            else if ((operation == "+" || operation == "-") && !bClose)
                            {
                                bufferWord = operation;
                                operation = "";
                            }

                            if(operation.Length > 0)
                            {
                                while (operatorsStack.Count > 0 && AppParams.getOperationRatio(operatorsStack.Peek()) <= AppParams.getOperationRatio(operation) && operatorsStack.Peek() != "(")
                                {
                                    output.Add(operatorsStack.Pop());
                                }
                                operatorsStack.Push(operation);
                                bClosed = false;
                                bClose = false;
                            }
                        }
                        if(c == '(')
                        {
                            operatorsStack.Push("(");
                        }
                        else if(c != ' ')
                            bufferWord += c;
                        if (c == '{')
                            opens++;
                        operation = "";
                        bOper = false;
                    }
                    continue;
                }
                else if(AppParams.Instance.isOperation("" + c))
                {
                    if (bufferWord.Length > 0 || (c != '*' && c != '&') || bClose)
                    {
                        bOper = true;
                        operation = "" + c;
                    }
                    else bufferWord += c;
                    continue;
                }
                else if(c == '(' && bufferWord.Length == 0)
                {
                    operatorsStack.Push("(");
                    continue;
                }
                else if(c == ')')
                {
                    bClose = true;
                    if (bufferWord.Length > 0)
                    {
                        output.Add(bufferWord);
                        bufferWord = "";
                    }
                    string oper;
                    int count = 0;
                    while((oper = operatorsStack.Pop()) != "(")
                    {
                        output.Add(oper);
                        count++;
                    }
                    if (count == 0)
                        bClosed = true;
                    continue;
                }

                switch(c)
                {
                    case '{':
                        bufferWord += c;
                        opens++;
                        break;
                    case '(':
                        if (Instance.isOperator(bufferWord))
                        {
                            output.Add(bufferWord);
                            operatorsStack.Push("(");
                            bufferWord = "";
                        }
                        else
                        {
                            bufferWord += c;
                            opens++;
                        }
                        break;
                    case ' ':
                        if (Instance.isOperator(bufferWord))
                        {
                            output.Add(bufferWord);
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
                if (bClosed)
                {
                    bClosed = false;
                    output.RemoveAt(output.Count - 1);
                }
                output.Add(bufferWord);
            }

            if (operation.Length > 0)
            {
                while (operatorsStack.Count > 0 && AppParams.getOperationRatio(operatorsStack.Peek()) <= AppParams.getOperationRatio(operation))
                {
                    output.Add(operatorsStack.Pop());
                }
                operatorsStack.Push(operation);
            }

            while(operatorsStack.Count > 0)
            {
                output.Add(operatorsStack.Pop());
            }

            return output;
        }

        public static int getOperationRatio(string operation)
        {
            switch(operation)
            {
                case "++":
                case "--":
                    return 3;
                case "*":
                case "/":
                case "%":
                    return 5;
                case "-":
                case "+":
                    return 6;
                case ">>":
                case "<<":
                    return 7;
                case ">":
                case "<":
                case "<=":
                case ">=":
                    return 8;
                case "&":
                    return 10;
                case "^":
                    return 11;
                case "|":
                    return 12;
                case "&&":
                    return 13;
                case "||":
                    return 14;
                case "=":
                case "+=":
                case "-=":
                case "*=":
                case "/=":
                case "%=":
                case "<<=":
                case ">>=":
                case "&=":
                case "^=":
                case "|=":
                    return 15;
                default:
                    return 0;
            }
        }
    }
}
