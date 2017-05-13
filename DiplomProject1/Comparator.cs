using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomProject1
{
    struct TileMatch
    {
        public int p, t;
        public string match;
    };

    struct MatchLink
    {
        public CodeObjectInterface P, T;
        public Match MatchP, MatchT;

        public MatchLink(CodeObjectInterface obj1, CodeObjectInterface obj2, Match p, Match t)
        {
            P = obj1;
            T = obj2;
            MatchP = p;
            MatchT = t;
        }
    }

    static class Comparator
    {
        public static double JackardCoef(string str1, string str2, int n)
        {
            HashSet<string> gramms1 = new HashSet<string>();
            HashSet<string> gramms2 = new HashSet<string>();

            for(int i=0; i<= str1.Length-n; i++)
            {
                string buffer = str1.Substring(i, n);
                gramms1.Add(buffer);
            }

            for (int i = 0; i <= str2.Length - n; i++)
            {
                string buffer = str2.Substring(i, n);
                gramms2.Add(buffer);
            }

            int common = gramms1.Union(gramms2).Count();
            int intersect = gramms2.Intersect(gramms1).Count();
            double coef = 0;
            if(common != 0)
                coef = (double)intersect / (double)common;
            return coef;
        }
        
        public static List<TileMatch> GreedyStringTiling(string P, string T, int minLength)
        {
            List<TileMatch> tiles = new List<TileMatch>();
            int maxMatch = 0;
            List<int> markP = new List<int>();
            List<int> markT = new List<int>();
            do
            {
                List<TileMatch> matches = new List<TileMatch>();
                maxMatch = minLength;
                for(int i=0; i<P.Length; i++)
                {
                    for(int k=0; k<T.Length; k++)
                    {
                        int j = 0;
                        string m = "";
                        while(k+j < T.Length && i+j < P.Length && P[i+j] == T[k+j] && markP.IndexOf(i+j)<0 && markT.IndexOf(k+j) < 0)
                        {
                            m += T[k + j];
                            j++;
                        }

                        if(j == maxMatch)
                        {
                            TileMatch match = new TileMatch();
                            match.p = i;
                            match.t = k;
                            match.match = m;
                            matches.Add(match);
                        }
                        else if(j > maxMatch)
                        {
                            matches.Clear();
                            TileMatch match = new TileMatch();
                            match.p = i;
                            match.t = k;
                            match.match = m;
                            matches.Add(match);
                            maxMatch = j;
                        }
                    }
                }

                foreach(TileMatch m in matches)
                {
                    bool flag = true;
                    for(int i=0; i<maxMatch; i++)
                    {
                        if (markP.IndexOf(m.p + i) >= 0 || markT.IndexOf(m.t + i) >= 0)
                        {
                            flag = false;
                            break;
                        }
                    }

                    if (!flag)
                        continue;

                    for (int i = 0; i < maxMatch; i++)
                    {
                        markT.Add(m.t + i);
                        markP.Add(m.p + i);
                    }

                    tiles.Add(m);
                }

            } while (maxMatch > minLength);
            return tiles;
        }

        public static List<TileMatch> SequenceAlignmentMethod(string P, string T)
        {
            List<TileMatch> tiles = new List<TileMatch>();

            List<List<int>> matrix = new List<List<int>>();

            int t = 0;
            int d = -1, m=1, g = 2;
            for (int i = 0; i <= P.Length; i++ )
            {
                matrix.Add(new List<int>());
                matrix[i].Add(0);
                for(int j=1; j <= T.Length; j++)
                {
                    if(i==0)
                    {
                        matrix[i].Add(0);
                        continue;
                    }

                    int max = 0;

                    int value = matrix[i - 1][j - 1] + (P[i-1] == T[j-1] ? m : d);
                    if (value > max)
                        max = value;

                    value = matrix[i - 1][j] + g;
                    if (value > max)
                        max = value;

                    value = matrix[i][j - 1] + g;
                    if (value > max)
                        max = value;
                    matrix[i].Add(max);
                }
            }
            List<TileMatch> p = new List<TileMatch>();

            for (int i = P.Length; i >= 0; i--)
            {
                int max = 0, k = 0;
                for(int j=0; j<=T.Length; j++)
                {
                    if(matrix[i][j] > max)
                    {
                        max = matrix[i][j];
                        k = j;
                    }
                }

                if (k > max)
                    k = max;
                if ((i != 0 && k != 0 && matrix[i][k] != 0))
                {
                    TileMatch match = new TileMatch();
                    match.match = "";
                    /*match.p = i;
                    match.t = k;
                    p.Add(match);
                     * */
                    while(i >= 0 && k>=0)
                    {
                        if (i != 0 && k != 0 && matrix[i][k] != 0)
                        {
                            match.match += P[i - 1];
                            i--;
                            k--;
                        }
                        else break;
                    }
                    match.p = i;
                    match.t = k;
                    tiles.Add(match);
                    continue;
                }
                else if(k!=0 && matrix[i][k] == 0)
                {
                    //p.Add(p[i-1]);
                    continue;
                }
            }

            return tiles;
        }

        public static double SimCoeff(int commonMatchLength, int PLength, int TLength)
        {
            return (2 * (double)commonMatchLength / (PLength + TLength));
        }


        public static double CompareMethods(FileCode file1, FileCode file2, double accuracy, int minLength, ref List<Match> pMatches, ref List<Match> tMatches, ref List<MatchLink> links)
        {
            if (minLength <= 0) minLength = 1;
            List<Match> matches = new List<Match>();
            List<Match> matches2 = new List<Match>();
            List<CodeObjectInterface> file1Methods = file1.Objects.FindAll(x => x.Type == ObjectType.Method && x.Token.Length > 0);
            List<CodeObjectInterface> file2Methods = file2.Objects.FindAll(x => x.Type == ObjectType.Method && x.Token.Length > 0);
            List<CodeObjectInterface> ignoreMethods = new List<CodeObjectInterface>();
            links = new List<MatchLink>();

            double coef = 0;
            foreach (CodeObjectInterface P in file1Methods)
            {
                List<TileMatch> bufferMatches = new List<TileMatch>();
                CodeObjectInterface bufferMeth = null;
                List<CodeObjectInterface> ignore = new List<CodeObjectInterface>();
                List<CodeObjectInterface> tList = new List<CodeObjectInterface>();
                double max = 0;
                while (true)
                {
                    max = 0;
                    foreach (CodeObjectInterface T in file2Methods)
                    {
                        if (ignore.Contains(T)) continue;
                        double sum = JackardCoef(P.Token, T.Token, minLength);
                        if (max < sum || T.Token == P.Token)
                        {
                            bufferMeth = T;
                            max = sum;
                        }
                    }
                    if (bufferMeth == null) break;
                    bool flag = false;
                    foreach (CodeObjectInterface buffP in file1Methods)
                    {
                        double sum = JackardCoef(buffP.Token, bufferMeth.Token, minLength);
                        if (max < sum)
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        ignore.Add(bufferMeth);
                        bufferMeth = null;
                    }
                    else break;
                }

                if (bufferMeth == null || max < accuracy) continue;

                
                bufferMatches = GreedyStringTiling(P.Token, bufferMeth.Token, minLength);

                ignoreMethods.Add(bufferMeth);
                int commonLength = 0;

                bool fl1 = false, fl2 = false;
                if (bufferMatches.Count == 1 && bufferMatches[0].match.Length == P.Token.Length && bufferMatches[0].match.Length == bufferMeth.Token.Length)
                {
                    matches.Add(new DiplomProject1.Match(P.Position, P.Length));
                    fl2 = true;
                    matches2.Add(new DiplomProject1.Match(bufferMeth.Position, bufferMeth.Length));
                    fl1 = true;
                    links.Add(new MatchLink(P, bufferMeth, new DiplomProject1.Match(P.Position, P.Length), new DiplomProject1.Match(bufferMeth.Position, bufferMeth.Length)));
                }

                foreach (TileMatch match in bufferMatches)
                {
                    for (int i = 0; i < match.match.Length; i++)
                    {

                        DiplomProject1.Match matchP = new DiplomProject1.Match(P.RecursiveChildrenList[match.p + i].Position, P.RecursiveChildrenList[match.p + i].Length);
                        if(!fl2)
                            matches.Add(matchP);
                            DiplomProject1.Match matchT = new DiplomProject1.Match(bufferMeth.RecursiveChildrenList[match.t + i].Position, bufferMeth.RecursiveChildrenList[match.t + i].Length);
                        if(!fl1)
                            matches2.Add(matchT);
                        links.Add(new MatchLink(P.RecursiveChildrenList[match.p + i], bufferMeth.RecursiveChildrenList[match.t + i], matchP, matchT));
                    }
                    commonLength += match.match.Length;
                }

                coef += SimCoeff(commonLength, bufferMeth.Token.Length, P.Token.Length);

            }
            double commonCoef = coef / file1Methods.Count;
            coef = 0;
            foreach (CodeObjectInterface P in file2Methods)
            {
                if (ignoreMethods.IndexOf(P) >= 0) continue;
                List<TileMatch> bufferMatches = new List<TileMatch>();
                CodeObjectInterface bufferMeth = null;
                List<CodeObjectInterface> ignore = new List<CodeObjectInterface>();
                double max = 0;
                while (true)
                {
                    max = 0;
                    foreach (CodeObjectInterface T in file1Methods)
                    {
                        if (ignore.Contains(T)) continue;
                        double sum = JackardCoef(P.Token, T.Token, minLength);
                        if (max < sum || T.Token == P.Token)
                        {
                            bufferMeth = T;
                            max = sum;
                        }
                    }
                    if (bufferMeth == null) break;
                    bool flag = false;
                    foreach (CodeObjectInterface buffP in file2Methods)
                    {
                        double sum = JackardCoef(buffP.Token, bufferMeth.Token, minLength);
                        if (max < sum)
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        ignore.Add(bufferMeth);
                        bufferMeth = null;
                    }
                    else break;
                }

                if (bufferMeth == null || max < accuracy) continue;


                bufferMatches = GreedyStringTiling(P.Token, bufferMeth.Token, minLength);

                int commonLength = 0;
                bool fl1 = false, fl2 = false;
                if (bufferMatches.Count == 1 && bufferMatches[0].match.Length == P.Token.Length && bufferMatches[0].match.Length == bufferMeth.Token.Length)
                {
                    matches2.Add(new DiplomProject1.Match(P.Position, P.Length));
                    fl2 = true;
                    matches.Add(new DiplomProject1.Match(bufferMeth.Position, bufferMeth.Length));
                    fl1 = true;
                    links.Add(new MatchLink(bufferMeth, P, new DiplomProject1.Match(bufferMeth.Position, bufferMeth.Length), new DiplomProject1.Match(P.Position, P.Length)));
                }
                else foreach (TileMatch match in bufferMatches)
                {
                    for (int i = 0; i < match.match.Length; i++)
                    {
                        DiplomProject1.Match matchT = new DiplomProject1.Match(P.RecursiveChildrenList[match.p + i].Position, P.RecursiveChildrenList[match.p + i].Length);
                        if(!fl2)
                            matches2.Add(matchT);
                        DiplomProject1.Match matchP = new DiplomProject1.Match(bufferMeth.RecursiveChildrenList[match.t + i].Position, bufferMeth.RecursiveChildrenList[match.t + i].Length);
                        if(!fl1)
                            matches.Add(matchP);
                        links.Add(new MatchLink(bufferMeth.RecursiveChildrenList[match.t + i], P.RecursiveChildrenList[match.p + i], matchP, matchT));
                    }
                    commonLength += match.match.Length;
                }

                coef += SimCoeff(commonLength, bufferMeth.Token.Length, P.Token.Length);

            }
            commonCoef += coef / (file2Methods.Count);
            pMatches = matches;
            tMatches = matches2;
            if (commonCoef > 1) commonCoef = 1;
            return commonCoef;
        }
    }
}
