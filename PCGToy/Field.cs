using System;
using System.Linq;
using System.Windows.Forms;

namespace PCGToy
{
    public partial class Field : UserControl
    {
        public Field(string name)
        {
            InitializeComponent();
            nameLabel.Text = Name = name;
            ParentChanged += (sender, args) =>
            {
                if (Parent != null)
                    valueComboBox.AutoCompleteCustomSource.AddRange(Domain.Select(d => d.ToString()).ToArray());
            };
        }

        private ProblemEditor Editor => (ProblemEditor) Parent;
        private PCGProblem Problem => Editor.Problem;
        private Variable Variable => Problem.Variables[Name];

        public object[] Domain => Variable.Domain;

        private void valueComboBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!Domain.Contains(valueComboBox.Text))
            {
                if (Editor.EditMode 
                    && MessageBox.Show($"\"{valueComboBox.Text}\" isn't a part of the domain {Variable.DomainName}.  Add it?",
                        "Unknown value", MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning)
                    == DialogResult.OK)
                {
                    Variable.Domain = Variable.Domain.Concat(new object[] {valueComboBox.Text}).ToArray();
                }
                else
                {
                    e.Cancel = true;
                    UpdateValue();
                    return;
                }
            }

            Variable.Value = valueComboBox.Text;
            lockedCheckBox.Checked = true;
        }

        public void UpdateValue()
        {
            var val = Variable.Value;
            valueComboBox.Text = val?.ToString() ?? "";
            Enabled = Variable.Domain.Length == 0 || val != null;
        }
        
        private void lockedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (lockedCheckBox.Checked)
            {
                // We just checked it
                Editor.Problem.Variables[Name].Value = valueComboBox.Text;
            }
            else
            {
                Editor.Problem.Variables[Name].Unbind();
            }
        }

        public void Unlock()
        {
            lockedCheckBox.Checked = false;
        }
    }
}
