namespace Ranger
{
    partial class RenameBookmarkForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RenameBookmarkForm));
            this.MyOKButton = new System.Windows.Forms.Button();
            this.MyCancelButton = new System.Windows.Forms.Button();
            this.NameTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.PathTextBox = new System.Windows.Forms.TextBox();
            this.OpenWithExplorerCheckbox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // MyOKButton
            // 
            this.MyOKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.MyOKButton.Location = new System.Drawing.Point(386, 85);
            this.MyOKButton.Name = "MyOKButton";
            this.MyOKButton.Size = new System.Drawing.Size(75, 23);
            this.MyOKButton.TabIndex = 2;
            this.MyOKButton.Text = "OK";
            this.MyOKButton.UseVisualStyleBackColor = true;
            this.MyOKButton.Click += new System.EventHandler(this.MyOKButton_Click);
            // 
            // MyCancelButton
            // 
            this.MyCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.MyCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.MyCancelButton.Location = new System.Drawing.Point(467, 85);
            this.MyCancelButton.Name = "MyCancelButton";
            this.MyCancelButton.Size = new System.Drawing.Size(75, 23);
            this.MyCancelButton.TabIndex = 3;
            this.MyCancelButton.Text = "Cancel";
            this.MyCancelButton.UseVisualStyleBackColor = true;
            this.MyCancelButton.Click += new System.EventHandler(this.MyCancelButton_Click);
            // 
            // NameTextBox
            // 
            this.NameTextBox.Location = new System.Drawing.Point(68, 13);
            this.NameTextBox.Name = "NameTextBox";
            this.NameTextBox.Size = new System.Drawing.Size(474, 20);
            this.NameTextBox.TabIndex = 0;
            this.NameTextBox.TextChanged += new System.EventHandler(this.NameTextBox_TextChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "Name";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(13, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 20);
            this.label2.TabIndex = 5;
            this.label2.Text = "Path";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // PathTextBox
            // 
            this.PathTextBox.Location = new System.Drawing.Point(68, 39);
            this.PathTextBox.Name = "PathTextBox";
            this.PathTextBox.Size = new System.Drawing.Size(474, 20);
            this.PathTextBox.TabIndex = 1;
            this.PathTextBox.TextChanged += new System.EventHandler(this.PathTextBox_TextChanged);
            // 
            // OpenWithExplorerCheckbox
            // 
            this.OpenWithExplorerCheckbox.AutoSize = true;
            this.OpenWithExplorerCheckbox.Location = new System.Drawing.Point(68, 65);
            this.OpenWithExplorerCheckbox.Name = "OpenWithExplorerCheckbox";
            this.OpenWithExplorerCheckbox.Size = new System.Drawing.Size(115, 17);
            this.OpenWithExplorerCheckbox.TabIndex = 6;
            this.OpenWithExplorerCheckbox.Text = "Open with Explorer";
            this.OpenWithExplorerCheckbox.UseVisualStyleBackColor = true;
            // 
            // RenameBookmarkForm
            // 
            this.AcceptButton = this.MyOKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.MyCancelButton;
            this.ClientSize = new System.Drawing.Size(555, 120);
            this.Controls.Add(this.OpenWithExplorerCheckbox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.PathTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.NameTextBox);
            this.Controls.Add(this.MyCancelButton);
            this.Controls.Add(this.MyOKButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RenameBookmarkForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Bookmark";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button MyOKButton;
        private System.Windows.Forms.Button MyCancelButton;
        private System.Windows.Forms.TextBox NameTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox PathTextBox;
        private System.Windows.Forms.CheckBox OpenWithExplorerCheckbox;
    }
}