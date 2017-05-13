using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace DiplomProject1
{
    public partial class Help : Form
    {
        public Help()
        {
            InitializeComponent();
            if (File.Exists("Help.txt"))
            {
                string str = File.ReadAllText("Help.txt", Encoding.GetEncoding(1251));
                label1.Text = str;
            }

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
