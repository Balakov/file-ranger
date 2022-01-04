namespace Ranger
{
    partial class OrganiseBookmarksForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ToolBarBookmarksListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ToolBarUpButton = new System.Windows.Forms.Button();
            this.ToolBarDownButton = new System.Windows.Forms.Button();
            this.MyOKButton = new System.Windows.Forms.Button();
            this.MyCancelButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.ToolBarBookmarksListView);
            this.groupBox1.Controls.Add(this.ToolBarUpButton);
            this.groupBox1.Controls.Add(this.ToolBarDownButton);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(299, 389);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Bookmarks";
            // 
            // ToolBarBookmarksListView
            // 
            this.ToolBarBookmarksListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ToolBarBookmarksListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.ToolBarBookmarksListView.FullRowSelect = true;
            this.ToolBarBookmarksListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.ToolBarBookmarksListView.HideSelection = false;
            this.ToolBarBookmarksListView.Location = new System.Drawing.Point(6, 19);
            this.ToolBarBookmarksListView.Name = "ToolBarBookmarksListView";
            this.ToolBarBookmarksListView.Size = new System.Drawing.Size(287, 335);
            this.ToolBarBookmarksListView.TabIndex = 0;
            this.ToolBarBookmarksListView.UseCompatibleStateImageBehavior = false;
            this.ToolBarBookmarksListView.View = System.Windows.Forms.View.Details;
            // 
            // ToolBarUpButton
            // 
            this.ToolBarUpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ToolBarUpButton.Location = new System.Drawing.Point(6, 360);
            this.ToolBarUpButton.Name = "ToolBarUpButton";
            this.ToolBarUpButton.Size = new System.Drawing.Size(75, 23);
            this.ToolBarUpButton.TabIndex = 3;
            this.ToolBarUpButton.Text = "Up";
            this.ToolBarUpButton.UseVisualStyleBackColor = true;
            this.ToolBarUpButton.Click += new System.EventHandler(this.ToolBarUpButton_Click);
            // 
            // ToolBarDownButton
            // 
            this.ToolBarDownButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ToolBarDownButton.Location = new System.Drawing.Point(87, 360);
            this.ToolBarDownButton.Name = "ToolBarDownButton";
            this.ToolBarDownButton.Size = new System.Drawing.Size(75, 23);
            this.ToolBarDownButton.TabIndex = 4;
            this.ToolBarDownButton.Text = "Down";
            this.ToolBarDownButton.UseVisualStyleBackColor = true;
            this.ToolBarDownButton.Click += new System.EventHandler(this.ToolBarDownButton_Click);
            // 
            // MyOKButton
            // 
            this.MyOKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.MyOKButton.Location = new System.Drawing.Point(155, 415);
            this.MyOKButton.Name = "MyOKButton";
            this.MyOKButton.Size = new System.Drawing.Size(75, 23);
            this.MyOKButton.TabIndex = 1;
            this.MyOKButton.Text = "OK";
            this.MyOKButton.UseVisualStyleBackColor = true;
            this.MyOKButton.Click += new System.EventHandler(this.MyOKButton_Click);
            // 
            // MyCancelButton
            // 
            this.MyCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.MyCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.MyCancelButton.Location = new System.Drawing.Point(236, 415);
            this.MyCancelButton.Name = "MyCancelButton";
            this.MyCancelButton.Size = new System.Drawing.Size(75, 23);
            this.MyCancelButton.TabIndex = 2;
            this.MyCancelButton.Text = "Cancel";
            this.MyCancelButton.UseVisualStyleBackColor = true;
            this.MyCancelButton.Click += new System.EventHandler(this.MyCancelButton_Click);
            // 
            // OrganiseBookmarksForm
            // 
            this.AcceptButton = this.MyOKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.MyCancelButton;
            this.ClientSize = new System.Drawing.Size(323, 450);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.MyCancelButton);
            this.Controls.Add(this.MyOKButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "OrganiseBookmarksForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Organise Bookmarks";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ListView ToolBarBookmarksListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Button MyOKButton;
        private System.Windows.Forms.Button MyCancelButton;
        private System.Windows.Forms.Button ToolBarUpButton;
        private System.Windows.Forms.Button ToolBarDownButton;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}