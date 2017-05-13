using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiplomProject1
{
    public partial class MainForm: Form
    {
        FileCode file1 = null, file2 = null;
        Help helpForm = null;
        List<Match> MatchFile1 = new List<Match>();
        List<Match> MatchFile2 = new List<Match>();
        public MainForm()
        {
            InitializeComponent();
            Controller.Instance.SetAccuracy(trackBarAccuracy.Value);
            Controller.Instance.SetMinLength(trackBarLength.Value);
            toolTipAccuracy.SetToolTip(trackBarAccuracy, "Минимальный процент совпадения 2х методов");
            toolTipAccuracy.SetToolTip(label1, "Минимальный процент совпадения 2х методов");
            toolTipAccuracy.SetToolTip(labelPercent, "Минимальный процент совпадения 2х методов");

            toolTipMinLength.SetToolTip(trackBarLength, "Минимальный размер совпадающего блока(в операциях)");
            toolTipMinLength.SetToolTip(label2, "Минимальный размер совпадающего блока(в операциях)");
            toolTipMinLength.SetToolTip(labelLength, "Минимальный размер совпадающего блока(в операциях)");

            toolTipTextFile1.SetToolTip(richTextBox_FileView1, "Исходный код файла #1");
            toolTipTextFile2.SetToolTip(richTextBox_FileView2, "Исходный код файла #2");
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog_LoadCode.FileName = "";
            openFileDialog_LoadCode.ShowDialog();
            string filename = openFileDialog_LoadCode.FileName;
            if (filename.Length == 0) return;
            richTextBox_FileView1.Text = Controller.Instance.SetFile1(filename);
            richTextBox_FileView2.Text = richTextBox_FileView2.Text;
            treeView1.Nodes.Clear();
            treeView1.Nodes.AddRange(Controller.Instance.GetTreeFile1().ToArray());
            Compare();
        }

        private void open2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog_LoadCode.FileName = "";
            openFileDialog_LoadCode.ShowDialog();
            string filename = openFileDialog_LoadCode.FileName;
            if (filename.Length == 0) return;
            richTextBox_FileView2.Text = Controller.Instance.SetFile2(filename);
            richTextBox_FileView1.Text = richTextBox_FileView1.Text;
            treeView2.Nodes.Clear();
            List<CustomTreeNode> nodes = Controller.Instance.GetTreeFile2();
            treeView2.Nodes.AddRange(nodes.ToArray());
            Compare();
        }

        private void buttonClear1_Click(object sender, EventArgs e)
        {
            Controller.Instance.ClearFile1();
            treeView1.Nodes.Clear();
            richTextBox_FileView1.Text = "";
            labelCoeff.Text = "Совпадение ≈ " + 0 + "%";
        }

        private void buttonClear2_Click(object sender, EventArgs e)
        {
            Controller.Instance.ClearFile2();
            treeView2.Nodes.Clear();
            richTextBox_FileView2.Text = "";
            labelCoeff.Text = "Совпадение ≈ " + 0 + "%";
        }

        private void trackBarAccuracy_Scroll(object sender, EventArgs e)
        {
            Controller.Instance.SetAccuracy(trackBarAccuracy.Value);
            labelPercent.Text = "" + trackBarAccuracy.Value + "%";
            Compare();
        }

        private void trackBarLength_Scroll(object sender, EventArgs e)
        {
            Controller.Instance.SetMinLength(trackBarLength.Value);
            labelLength.Text = "" + trackBarLength.Value;
            Compare();
        }

        protected void Compare()
        {
            treeView1.SelectedNode = null;
            treeView2.SelectedNode = null;
            double coef = Controller.Instance.Compare();

            MatchFile1 = Controller.Instance.MatchP;
            MatchFile2 = Controller.Instance.MatchT;

            richTextBox_FileView2.Text = richTextBox_FileView2.Text;
            richTextBox_FileView1.Text = richTextBox_FileView1.Text;
            labelCoeff.Text = "";

            if (MatchFile2 == null || MatchFile2 == null)
                return;

            SelectMatches();

            labelCoeff.Text = "Совпадение ≈ " + (int)(coef * 100) + "%";
        }

        protected void SelectMatches()
        {
            richTextBox_FileView2.Text = richTextBox_FileView2.Text;
            richTextBox_FileView1.Text = richTextBox_FileView1.Text;

            if (MatchFile1 == null || MatchFile2 == null)
                return;

            foreach (Match obj in MatchFile1)
            {
                SelectMatchFile1(obj.pos, obj.length);
            }

            foreach (Match obj in MatchFile2)
            {
                SelectMatchFile2(obj.pos, obj.length);
            }
        }
        
        protected void ClearFile1Selection()
        {
            richTextBox_FileView1.Text = richTextBox_FileView1.Text;
        }

        protected void ClearFile2Selection()
        {
            richTextBox_FileView2.Text = richTextBox_FileView2.Text;
        }

        protected void SelectMatchFile1(int pos, int length)
        {
            richTextBox_FileView1.Select(pos, length);
            richTextBox_FileView1.SelectionBackColor = Color.Black;
            richTextBox_FileView1.SelectionColor = Color.White;
        }

        protected void DeselectFragmentFile1(int pos, int length)
        {
            richTextBox_FileView1.Select(pos, length);
            richTextBox_FileView1.SelectionBackColor = Color.White;
            richTextBox_FileView1.SelectionColor = Color.Black;
        }

        protected void SelectMatchFile2(int pos, int length)
        {
            richTextBox_FileView2.Select(pos, length);
            richTextBox_FileView2.SelectionBackColor = Color.Black;
            richTextBox_FileView2.SelectionColor = Color.White;
        }

        protected void DeselectFragmentFile2(int pos, int length)
        {
            richTextBox_FileView2.Select(pos, length);
            richTextBox_FileView2.SelectionBackColor = Color.White;
            richTextBox_FileView2.SelectionColor = Color.Black;
        }

        private void справкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (helpForm == null)
            {
                helpForm = new Help();
                helpForm.Show();
            }
            else if(helpForm.IsDisposed)
            {
                helpForm = new Help();
                helpForm.Show();
            }
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("AntiPlagiator\nАвтор Невзоров Георгий\nНовосибирск, 2016г.");
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            CustomTreeNode node = (CustomTreeNode)e.Node;
            if (node != null)
            {
                ClearFile1Selection();
                ClearFile2Selection();
                SelectMatches();
                List<DiplomProject1.Match> matches = new List<Match>();
                Controller.Instance.GetMatchesForFile1Node(node.Object, ref matches);
                richTextBox_FileView1.Select(node.Object.Position, node.Object.CommonLength);
                richTextBox_FileView1.SelectionBackColor = Color.Blue;
                richTextBox_FileView1.SelectionColor = Color.White;
                richTextBox_FileView1.ScrollToCaret();
                foreach(Match m in matches)
                {
                    richTextBox_FileView2.Select(m.pos, m.length);
                    richTextBox_FileView2.SelectionBackColor = Color.Blue;
                    richTextBox_FileView2.SelectionColor = Color.White;
                }
                if (matches.Count > 0)
                    richTextBox_FileView2.ScrollToCaret();
            }
        }

        private void treeView2_AfterSelect(object sender, TreeViewEventArgs e)
        {
            CustomTreeNode node = (CustomTreeNode)e.Node;
            if (node != null)
            {
                ClearFile1Selection();
                ClearFile2Selection();
                SelectMatches();
                List<DiplomProject1.Match> matches = new List<Match>();
                Controller.Instance.GetMatchesForFile2Node(node.Object, ref matches);
                richTextBox_FileView2.Select(node.Object.Position, node.Object.CommonLength);
                richTextBox_FileView2.SelectionBackColor = Color.Blue;
                richTextBox_FileView2.SelectionColor = Color.White;
                richTextBox_FileView2.ScrollToCaret();
                foreach (Match m in matches)
                {
                    richTextBox_FileView1.Select(m.pos, m.length);
                    richTextBox_FileView1.SelectionBackColor = Color.Blue;
                    richTextBox_FileView1.SelectionColor = Color.White;
                }
                if (matches.Count > 0)
                    richTextBox_FileView1.ScrollToCaret();
            }
        }
    }
}
