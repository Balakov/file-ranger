using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Ranger
{
    public class ThumbnailCreator
    {
        private Thread m_thread;
        private bool m_abort = false;

        public interface IAddThumbnail
        {
            void AddThumbnail(Image image, int index, ImageList currentImageList);
        }

        public ThumbnailCreator(List<string> files, IAddThumbnail thumbnailProcessor, int thumbnailWidth, int thumbnailHeight, ImageList currentImageList, ThumbnailCache thumbnailCache)
        {
            m_thread = new Thread(() =>
            {
                int index = 0;
                foreach (string file in files)
                {
                    if (m_abort)
                    {
                        break;
                    }

                    if (file == null)
                    {
                        var thumbnail = ImageLoader.LoadFolderThumbnail(thumbnailWidth, thumbnailHeight, thumbnailCache);
                        thumbnailProcessor.AddThumbnail(thumbnail, index, currentImageList);
                    }
                    else
                    {
                        var thumbnail = ImageLoader.Load(file, thumbnailWidth, thumbnailHeight, thumbnailCache);
                        thumbnailProcessor.AddThumbnail(thumbnail, index, currentImageList);
                    }

                    index++;
                }
            });

            m_thread.IsBackground = true;
            m_thread.Start();
        }

        public void Abort()
        {
            m_abort = true;
        }

    }
}
