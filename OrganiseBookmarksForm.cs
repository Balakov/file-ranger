using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Ranger
{
    public partial class OrganiseBookmarksForm : Form
    {
        public IEnumerable<BookmarkTag> Bookmarks => ToolBarBookmarksListView.Items.Cast<ListViewItem>().Select(x => x.Tag as BookmarkTag);

        public OrganiseBookmarksForm()
        {
            InitializeComponent();
        }

        public OrganiseBookmarksForm(List<BookmarkTag> bookmarks)
        {
            InitializeComponent();

            foreach (var item in bookmarks)
            {
                ToolBarBookmarksListView.Items.Add(new ListViewItem(item.DisplayName) { Tag = item });
            }

            ToolBarBookmarksListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private enum MoveType
        {
            Up,
            Down
        }

        private static void MoveItem(ListView listView, MoveType moveType)
        {
            if (listView.SelectedItems.Count == 1)
            {
                var selectedItem = listView.SelectedItems[0];
                var selectedIndex = listView.SelectedIndices[0];

                if (moveType == MoveType.Up)
                {
                    int newIndex = selectedIndex - 1;
                    if (newIndex >= 0)
                    {
                        listView.Items.Remove(selectedItem);
                        listView.Items.Insert(newIndex, selectedItem);
                    }
                }
                else if (moveType == MoveType.Down)
                {
                    int newIndex = selectedIndex + 1;
                    if (newIndex < listView.Items.Count)
                    {
                        listView.Items.Remove(selectedItem);
                        listView.Items.Insert(newIndex, selectedItem);
                    }
                }
            }
        }

        private void ToolBarUpButton_Click(object sender, EventArgs e)
        {
            MoveItem(ToolBarBookmarksListView, MoveType.Up);
        }

        private void ToolBarDownButton_Click(object sender, EventArgs e)
        {
            MoveItem(ToolBarBookmarksListView, MoveType.Down);
        }

        private void MyCancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void MyOKButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
