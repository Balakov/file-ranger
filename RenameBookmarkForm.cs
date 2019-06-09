using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ranger
{
    public partial class RenameBookmarkForm : Form
    {
        public string DisplayName
        {
            get
            {
                return NameTextBox.Text;
            }
        }

        public RenameBookmarkForm(string initialDisplayName)
        {
            InitializeComponent();

            NameTextBox.Text = initialDisplayName;
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
            MyOKButton.Enabled = !string.IsNullOrEmpty(NameTextBox.Text) && !NameTextBox.Text.Contains("|");
        }
    }
}
