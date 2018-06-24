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
            this.rollButton = new System.Windows.Forms.Button();
            this.editModeCheckBox = new System.Windows.Forms.CheckBox();
            this.addButton = new System.Windows.Forms.Button();
            this.addNogoodButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // rollButton
            // 
            this.rollButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rollButton.Location = new System.Drawing.Point(1032, 30);
            this.rollButton.Name = "rollButton";
            this.rollButton.Size = new System.Drawing.Size(123, 72);
            this.rollButton.TabIndex = 0;
            this.rollButton.Text = "Roll";
            this.rollButton.UseVisualStyleBackColor = true;
            this.rollButton.Click += new System.EventHandler(this.nopeButton_Click);
            // 
            // editModeCheckBox
            // 
            this.editModeCheckBox.AutoSize = true;
            this.editModeCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.editModeCheckBox.Location = new System.Drawing.Point(1041, 128);
            this.editModeCheckBox.Name = "editModeCheckBox";
            this.editModeCheckBox.Size = new System.Drawing.Size(104, 41);
            this.editModeCheckBox.TabIndex = 2;
            this.editModeCheckBox.Text = "Edit";
            this.editModeCheckBox.UseVisualStyleBackColor = true;
            this.editModeCheckBox.CheckedChanged += new System.EventHandler(this.editModeCheckBox_CheckedChanged);
            // 
            // addButton
            // 
            this.addButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.addButton.Location = new System.Drawing.Point(1032, 195);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(123, 72);
            this.addButton.TabIndex = 3;
            this.addButton.Text = "Add";
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Visible = false;
            this.addButton.Click += new System.EventHandler(this.addButton_Click);
            // 
            // addNogoodButton
            // 
            this.addNogoodButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.addNogoodButton.Location = new System.Drawing.Point(1032, 297);
            this.addNogoodButton.Name = "addNogoodButton";
            this.addNogoodButton.Size = new System.Drawing.Size(123, 72);
            this.addNogoodButton.TabIndex = 4;
            this.addNogoodButton.Text = "Nope";
            this.addNogoodButton.UseVisualStyleBackColor = true;
            this.addNogoodButton.Visible = false;
            this.addNogoodButton.Click += new System.EventHandler(this.addNogoodButton_Click);
            // 
            // ProblemEditor
            // 
            this.AcceptButton = this.rollButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1191, 1003);
            this.Controls.Add(this.addNogoodButton);
            this.Controls.Add(this.addButton);
            this.Controls.Add(this.editModeCheckBox);
            this.Controls.Add(this.rollButton);
            this.KeyPreview = true;
            this.Name = "ProblemEditor";
            this.Text = "PCGToy";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProblemEditor_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ProblemEditor_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button rollButton;
        private System.Windows.Forms.CheckBox editModeCheckBox;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Button addNogoodButton;
    }
}

