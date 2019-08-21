using System;
using System.Windows.Forms;

namespace Ranger
{
    public partial class EverythingSearchForm : Form
    {
        private Action<string> m_selectionAction;

        public EverythingSearchForm(Action<string> selectionAction)
        {
            m_selectionAction = selectionAction;

            InitializeComponent();
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            var results = Everything.SDK.Search((sender as TextBox).Text);

            SearchListView.BeginUpdate();
            SearchListView.Items.Clear();

            foreach (var result in results)
            {
                var lvi = new ListViewItem(new string[]
                {
                    System.IO.Path.GetFileName(result.Path),
                    result.Path,
                    result.Size == -1 ? "" : string.Format("{0:N0}", result.Size),
                    result.Date.ToString("dd/MM/yyyy HH:mm:ss")
                });

                lvi.Tag = result;

                SearchListView.Items.Add(lvi);
            }

            SearchListView.EndUpdate();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SearchListView_ItemActivate(object sender, EventArgs e)
        {
            if (SearchListView.SelectedItems.Count == 1)
            {
                Everything.SDK.SearchResult result = SearchListView.SelectedItems[0].Tag as Everything.SDK.SearchResult;

                m_selectionAction(result.Path);
            }
        }
    }
}
