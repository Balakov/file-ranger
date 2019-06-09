using System.Windows.Forms;

namespace Ranger
{
    public class ToolStripButtonWithContextMenu : ToolStripButton
    {
        public ContextMenuStrip ContextMenuStrip { get; set; }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (ContextMenuStrip != null)
                {
                    ContextMenuStrip.Show(Cursor.Position);
                    ContextMenuStrip.Tag = this;
                }
            }
        }
    }
}
