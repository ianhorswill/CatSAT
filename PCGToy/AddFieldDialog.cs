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
    public partial class AddFieldDialog : Form
    {
        private ProblemEditor Editor;
        private PCGProblem Problem => Editor.Problem;

        public AddFieldDialog(ProblemEditor editor)
        {
            InitializeComponent();
            Editor = editor;
            domainComboBox.AutoCompleteCustomSource.AddRange(Problem.Domains.Select(pair => pair.Key).ToArray());
            conditionVarComboBox.AutoCompleteCustomSource.AddRange(Problem.Variables.Select(pair => pair.Key).ToArray());
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            var domainName = domainComboBox.Text;
            if (!Problem.Domains.ContainsKey(domainName))
            {
                if (MessageBox.Show($"The domain \"{domainName}\" hasn't been defined.  Add it?",
                        "Unknown domain",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning)
                    == DialogResult.OK)
                {
                    Problem.AddDomain(domainName);
                }
                else
                    return;
            }

            Problem.AddVariable(nameTextBox.Text, domainName,
                conditionVarComboBox.Text,
                conditionValueComboBox.Text);
            this.DialogResult = DialogResult.OK;
        }

        private void conditionVarComboBox_Validating(object sender, CancelEventArgs e)
        {
            if (!Problem.Variables.ContainsKey(conditionVarComboBox.Text))
            {
                InvalidateConditionVarChoice("No such variable");
            }
            else
            {
                var v = Problem.Variables[conditionVarComboBox.Text];
                var d = v.Domain;
                conditionValueComboBox.AutoCompleteCustomSource.Clear();
                conditionValueComboBox.AutoCompleteCustomSource.AddRange(d.Select(x => x.ToString()).ToArray());
                if (d.Length > 0)
                    conditionValueComboBox.Text = d[0].ToString();
                else
                    InvalidateConditionVarChoice("Variable has an empty domain (no possible values)");
            }
        }

        private void InvalidateConditionVarChoice(string message)
        {
            MessageBox.Show(message, "Invalid variable choice", MessageBoxButtons.OK);
            conditionVarComboBox.Text = "";
            conditionValueComboBox.Text = conditionVarComboBox.Text = "";
            conditionValueComboBox.AutoCompleteCustomSource.Clear();
        }

        private void conditionValueComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var vName = conditionVarComboBox.Text;
            var v = Problem.Variables[vName];
            var d = v.Domain;
            if (!d.Contains(conditionValueComboBox.Text))
            {
                MessageBox.Show($"That is not a possible value for {vName}", "Invalid variable choice", MessageBoxButtons.OK);
                conditionValueComboBox.Text = "";
            }
        }
    }
}
