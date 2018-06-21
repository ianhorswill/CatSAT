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
            nameTextBox.Text = Name = name;
            nameTextBox.TextChanged += (ignore, args) =>
            {
                Name = nameTextBox.Text;
                lockedCheckBox.Checked = true;
            };
        }

        private ProblemEditor Editor => (ProblemEditor) Parent;
        private PCGProblem Problem => Editor.Problem;
        private Variable Variable => Problem.Variables[Name];

        public object[] Domain => Variable.Domain;

        private void valueComboBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Domain.Contains(valueComboBox.Text))
                Variable.Value = valueComboBox.Text;
            else
            {
                e.Cancel = true;
                UpdateValue();
            }
        }

        public void UpdateValue()
        {
            var val = Variable.Value;
            valueComboBox.Text = val?.ToString() ?? "";
            Enabled = val != null;
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
    }
}
