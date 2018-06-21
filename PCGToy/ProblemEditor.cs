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
            var x = 25;
            var y = 75;
            foreach (var c in Controls)
                if (c is Field f)
                    Controls.Remove(f);
            Problem.LoadFromFile(pathComboBox.Text);
            foreach (var v in Problem.Variables)
            {
                var f = new Field(v.Value.Name);
                f.Location = new Point(x, y);
                y += f.Height + 10;
                Controls.Add(f);
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
            }
        }
    }
}
