using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
// ReSharper disable LocalizableElement

namespace PCGToy
{
    public partial class ProblemEditor : Form
    {
        public ProblemEditor()
        {
            InitializeComponent();
            Problem = new PCGProblem();
            EditMode = true;
        }

        private string _filePath;

        public string FilePath
        {
            get => _filePath;
            set
            {
                Text = System.IO.Path.GetFileName(value);
                _filePath = value;
            }
        }

        public bool EditMode
        {
            get => editModeCheckBox.Checked;
            set => editModeCheckBox.Checked = value;
        }

        public readonly PCGProblem Problem;

        public void MaybeSave()
        {
            if (Problem.IsDirty && MessageBox.Show("Do you want to save your file?", "Unsaved changes",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                Save();
        }

        private void Save()
        {
            if (FilePath == null)
                SaveAsFile();
            else
                Problem.WriteToFile(FilePath);
        }

        private void nopeButton_Click(object sender, EventArgs e)
        {
            Solve();
        }

        private void Solve()
        {
            try
            {
                Problem.Solve();

                foreach (var c in Controls)
                    if (c is Field f)
                        f.UpdateValue();
            }
            catch (PicoSAT.TimeoutException)
            {
                MessageBox.Show(
                    "Can't find a solution that satisfies all the requirements.  Try unlocking some variables!",
                    "Unsatisfiable constraints",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        void Reload()
        {
            // Remove current fields
            RemoveFields();

            // Reload problem
            Problem.LoadFromFile(FilePath);

            // Make new fields
            BuildFields();
            EditMode = false;
        }

        private void RebuildFields()
        {
            RemoveFields();
            BuildFields();
        }

        private void BuildFields()
        {
            var x = 7;
            var y = 13;

            void Build(Variable v, int indent)
            {
                //if (v.Value == null && v.Domain.Length != 0)
                //    // Don't display
                //    return;

                var f = new Field(v.Name) {Location = new Point(x + 20 * indent, y)};
                y += f.Height + 5;
                Controls.Add(f);
                foreach (var c in v.Children)
                    Build(c, indent + 1);
            }

            var vars = Problem.Variables.Select(p => p.Value).ToArray();
            //Array.Sort(vars, (v1, v2) => String.Compare(v1.Name, v2.Name, StringComparison.Ordinal));

            foreach (var v in vars)
            {
                if (v.Parent == null)
                    Build(v, 0);
            }
        }

        private void RemoveFields()
        {
            for (var i = Controls.Count - 1; i >= 0; i--)
            {
                if (Controls[i] is Field)
                    Controls.RemoveAt(i);
            }
        }

        private void ProblemEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                e.Handled = true;

                switch (e.KeyCode)
                {
                    case Keys.O:
                        OpenFile();
                        break;

                    case Keys.S:
                        if (e.Shift)
                            SaveAsFile();
                        else
                            Save();
                        break;

                    case Keys.N:
                        new ProblemEditor().Show();
                        break;

                    case Keys.U:
                        foreach (var c in Controls)
                            if (c is Field f)
                                f.Unlock();
                        break;

                    default:
                        e.Handled = false;
                        break;
                }
            }
        }

        private void OpenFile()
        {
            MaybeSave();
            var d = new OpenFileDialog
            {
                Filter = "SCM files (*.scm)|*.scm|All files (*.*)|*.*",
                RestoreDirectory = true
            };
            if (d.ShowDialog() == DialogResult.OK)
            {
                FilePath = d.FileName;
                Reload();
                Solve();
            }
        }

        private void SaveAsFile()
        {
            var d = new SaveFileDialog
            {
                Filter = "SCM files (*.scm)|*.scm|All files (*.*)|*.*",
                RestoreDirectory = true
            };
            if (d.ShowDialog() == DialogResult.OK)
            {
                FilePath = d.FileName;
                Save();
            }
        }

        private void editModeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            addNogoodButton.Visible = addButton.Visible = editModeCheckBox.Checked;
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            var d = new AddVariableDialog(this);
            if (d.ShowDialog() == DialogResult.OK)
            {
                RebuildFields();
                Solve();
            }
        }

        private void addNogoodButton_Click(object sender, EventArgs e)
        {
            var d = new AddNogoodDialog(this);
            d.ShowDialog();
        }

        private void ProblemEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            MaybeSave();
        }
    }
}
