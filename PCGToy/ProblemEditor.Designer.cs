namespace PCGToy
{
    partial class ProblemEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.nopeButton = new System.Windows.Forms.Button();
            this.pathComboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // nopeButton
            // 
            this.nopeButton.Location = new System.Drawing.Point(838, 30);
            this.nopeButton.Name = "nopeButton";
            this.nopeButton.Size = new System.Drawing.Size(123, 72);
            this.nopeButton.TabIndex = 0;
            this.nopeButton.Text = "NOPE";
            this.nopeButton.UseVisualStyleBackColor = true;
            this.nopeButton.Click += new System.EventHandler(this.nopeButton_Click);
            // 
            // pathComboBox
            // 
            this.pathComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.pathComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.pathComboBox.FormattingEnabled = true;
            this.pathComboBox.Location = new System.Drawing.Point(26, 30);
            this.pathComboBox.Name = "pathComboBox";
            this.pathComboBox.Size = new System.Drawing.Size(700, 33);
            this.pathComboBox.TabIndex = 1;
            this.pathComboBox.Text = "Enter file name";
            this.pathComboBox.Validated += new System.EventHandler(this.pathComboBox_Validated);
            // 
            // ProblemEditor
            // 
            this.AcceptButton = this.nopeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1002, 1003);
            this.Controls.Add(this.pathComboBox);
            this.Controls.Add(this.nopeButton);
            this.KeyPreview = true;
            this.Name = "ProblemEditor";
            this.Text = "PCGToy";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ProblemEditor_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button nopeButton;
        private System.Windows.Forms.ComboBox pathComboBox;
    }
}

