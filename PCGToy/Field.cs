#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Field.cs" company="Ian Horswill">
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
using System.Collections.Generic;
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

        public IList<object> Domain => Variable.DomainValues;

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
                    Variable.DomainValues = Variable.DomainValues.Concat(new object[] {valueComboBox.Text}).ToArray();
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
            Enabled = Variable.DomainValues.Count == 0 || val != null;
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
