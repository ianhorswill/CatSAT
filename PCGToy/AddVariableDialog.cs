#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddVariableDialog.cs" company="Ian Horswill">
// Copyright (C) 2018 Ian Horswill
//  
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in the
// Software without restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
// and to permit persons to whom the Software is furnished to do so, subject to the
// following conditions:
//  
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
#endregion
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace PCGToy
{
    public partial class AddVariableDialog : Form
    {
        private ProblemEditor Editor;
        private PCGProblem Problem => Editor.Problem;

        public AddVariableDialog(ProblemEditor editor)
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
                var d = v.DomainValues;
                conditionValueComboBox.AutoCompleteCustomSource.Clear();
                conditionValueComboBox.AutoCompleteCustomSource.AddRange(d.Select(x => x.ToString()).ToArray());
                if (d.Count > 0)
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
            var d = v.DomainValues;
            if (!d.Contains(conditionValueComboBox.Text))
            {
                MessageBox.Show($"That is not a possible value for {vName}", "Invalid variable choice", MessageBoxButtons.OK);
                conditionValueComboBox.Text = "";
            }
        }
    }
}
