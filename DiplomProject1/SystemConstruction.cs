using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiplomProject1
{
    public class SystemConstruction : CodeObjectInterface
    {
        protected List<Variable> Variables = new List<Variable>();
        protected string Condition = "";

        public override CustomTreeNode Node
        {
            get
            {
                string val = Name;
                if(Condition.Length > 0)
                {
                    val = Name + "(" + Condition + ")";
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
