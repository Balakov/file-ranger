using System.Windows.Forms;

namespace Ranger
{
    public class FlickerFreeListView : ListView
    {
        public FlickerFreeListView()
        {
            DoubleBuffered = true;
        }
    }
}
