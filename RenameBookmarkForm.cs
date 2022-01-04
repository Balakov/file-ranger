using System;
using System.Windows.Forms;

namespace Ranger
{
    public partial class RenameBookmarkForm : Form
    {
        public string DisplayName => NameTextBox.Text;
        public string Path => PathTextBox.Text;

        public RenameBookmarkForm(string initialDisplayName, string initialPath)
        {
            InitializeComponent();

            NameTextBox.Text = initialDisplayName;
            PathTextBox.Text = initialPath;
        }

        private void MyOKButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void MyCancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void NameTextBox_TextChanged(object sender, EventArgs e)
        {
            ValidateText();
        }

        private void PathTextBox_TextChanged(object sender, EventArgs e)
        {
            ValidateText();
        }
        
        private void ValidateText()
        {
            MyOKButton.Enabled = !string.IsNullOrEmpty(NameTextBox.Text) &&
                                 !NameTextBox.Text.Contains("|") &&
                                 !string.IsNullOrEmpty(PathTextBox.Text);
        }

    }
}
