using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiplomProject1
{
    struct Match
    {
        public int pos, length;
        public Match(int _p, int _l)
        {
            pos = _p;
            length = _l;
        }
    }

    class Controller
    {
        protected double accuracy = 0.2;
        protected int minLength = 4;
        protected FileCode file1 = null;
        protected FileCode file2 = null;
        private static readonly Controller instance = new Controller();
        protected List<MatchLink> links = new List<MatchLink>();
        public List<DiplomProject1.Match> MatchP = new List<Match>();
        public List<DiplomProject1.Match> MatchT = new List<Match>();

        public static Controller Instance
        {
            get
            {
                return instance;
            }
        }

        private Controller()
        {}

        public string SetFile1(string filename)
        {
            if (file1 != null)
                file1 = null;
            file1 = new FileCode(filename);
            return file1.Body;
        }

        public string SetFile2(string filename)
        {
            if (file2 != null)
                file2 = null;
            file2 = new FileCode(filename);
            return file2.Body;
        }

        public void ClearFile1()
        {
            file1 = null;
            links.Clear();
        }

        public void ClearFile2()
        {
            file2 = null;
            links.Clear();
        }

        public double Compare()
        {
            MatchP.Clear();
            MatchT.Clear();
            links.Clear();
            double coef = 0;
            if(file1 != null && file2 != null)
                coef = Comparator.CompareMethods(file1, file2, accuracy, minLength, ref MatchP, ref MatchT, ref links);
            return coef;
        }

        public void SetAccuracy(double acc)
        {
            accuracy = acc;
        }

        public void SetAccuracy(int acc)
        {
            accuracy = (double)acc / 100;
        }

        public void SetMinLength(int len)
        {
            minLength = len;
        }

        public List<CustomTreeNode> GetTreeFile1()
        {
            if(file1!=null)
            {
                return file1.Node;
            }
            return null;
        }

        public List<CustomTreeNode> GetTreeFile2()
        {
            if (file2 != null)
            {
                return file2.Node;
            }
            return null;
        }

        public void GetMatchesForFile1Node(CodeObjectInterface node, ref List<DiplomProject1.Match> T)
        {
            List<MatchLink> result = links.FindAll(x => x.P == node);
            T = new List<Match>();

            foreach(MatchLink link in result)
            {
                T.Add(new DiplomProject1.Match(link.T.Position, link.T.CommonLength));
            }
        }

        public void GetMatchesForFile2Node(CodeObjectInterface node, ref List<Match> P)
        {
            List<MatchLink> result = links.FindAll(x => x.T == node);
            P = new List<Match>();

            foreach (MatchLink link in result)
            {
                P.Add(new DiplomProject1.Match(link.P.Position, link.P.CommonLength));
            }
        }
    }
}
