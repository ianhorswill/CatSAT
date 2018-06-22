using System;
using System.Drawing;
using System.Windows.Forms;

namespace PCGToy
{
    public partial class ProblemEditor : Form
    {
        public ProblemEditor()
        {
            InitializeComponent();
            Problem = new PCGProblem();
        }

        public bool EditMode => editModeCheckBox.Checked;

        public readonly PCGProblem Problem;

        private void nopeButton_Click(object sender, EventArgs e)
        {
            Solve();
        }

        private void Solve()
        {
            Problem.Solve();
            foreach (var c in Controls)
                if (c is Field f)
                    f.UpdateValue();
        }

        private void pathComboBox_Validated(object sender, EventArgs e)
        {
            Reload();
        }

        void Reload()
        {
            // Remove current fields
            RemoveFields();

            // Reload problem
            Problem.LoadFromFile(pathComboBox.Text);

            // Make new fields
            RebuildFields();
        }

        private void RebuildFields()
        {
            var x = 7;
            var y = 55;
            foreach (var v in Problem.Variables)
            {
                var f = new Field(v.Value.Name);
                f.Location = new Point(x, y);
                y += f.Height + 10;
                Controls.Add(f);
            }
        }

        private void RemoveFields()
        {
            for (int i = Controls.Count - 1; i >= 0; i--)
            {
                var c = Controls[i];
                if (c is Field f)
                    Controls.RemoveAt(i);
            }
        }

        private void ProblemEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.O)
            {
                var d = new OpenFileDialog();
                d.Filter = "SCM files (*.scm)|All files (*.*)";
                d.RestoreDirectory = true;
                if (d.ShowDialog() == DialogResult.OK)
                {
                    pathComboBox.Text = d.FileName;
                    Reload();
                    Solve();
                }

                e.Handled = true;
            } else if (e.Control && e.KeyCode == Keys.S)
                Problem.WriteToFile(pathComboBox.Text);
        }

        private void editModeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            addNogoodButton.Visible = addButton.Visible = editModeCheckBox.Checked;
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            var d = new AddFieldDialog(this);
            if (d.ShowDialog() == DialogResult.OK)
            {
                RemoveFields();
                RebuildFields();
            }
        }
    }
}
