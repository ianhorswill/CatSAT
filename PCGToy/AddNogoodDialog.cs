using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCGToy
{
    public partial class AddNogoodDialog : Form
    {
        private readonly ProblemEditor Editor;
        private PCGProblem Problem => Editor.Problem;

        public AddNogoodDialog(ProblemEditor editor)
        {
            Editor = editor;
            InitializeComponent();
            variablesListBox.DataSource = Problem.Variables.Select(pair=>pair.Value).ToArray();
            variablesListBox.DisplayMember = "NameAndValue";
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Problem.AddNogood(variablesListBox.SelectedItems.Cast<Variable>());
            DialogResult = DialogResult.OK;
        }
    }
}
