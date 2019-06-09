using System.Collections.Generic;

namespace Ranger
{
    public class PathHistory
    {
        private List<string> m_paths = new List<string>();
        private Dictionary<string, string> m_previouslySelectedDirectories = new Dictionary<string, string>();
        private int m_currentIndex = -1;
        private readonly char[] m_separators = new char[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar };

        public void PushPath(string path)
        {
            if (m_currentIndex != m_paths.Count - 1)
            {
                // Clear the history in front of this entry
                m_paths.RemoveRange(m_currentIndex + 1, (m_paths.Count - 1) - m_currentIndex);
            }

            m_paths.Add(path);
            m_currentIndex = m_paths.Count - 1;
        }

        public string Back()
        {
            if (m_currentIndex > 0)
            {
                m_currentIndex--;
            }

            return m_paths[m_currentIndex];
        }

        public string Forward()
        {
            if (m_currentIndex < m_paths.Count-1)
            {
                m_currentIndex++;
            }

            return m_paths[m_currentIndex];
        }

        public void SetPreviouslySelectedDirectoryForPath(string path, string lastSelectedDirectory)
        {
            if (path != null)
            {
                path = path.TrimEnd(m_separators);
                lastSelectedDirectory = lastSelectedDirectory.TrimEnd(m_separators);

                if (!m_previouslySelectedDirectories.ContainsKey(path))
                {
                    m_previouslySelectedDirectories.Add(path, lastSelectedDirectory);
                }
                else
                {
                    m_previouslySelectedDirectories[path] = lastSelectedDirectory;
                }
            }
        }

        public string GetPreviouslySelectedDirectoryForPath(string path)
        {
            if (path != null)
            {
                path = path.TrimEnd(m_separators);

                if (m_previouslySelectedDirectories.ContainsKey(path))
                {
                    return m_previouslySelectedDirectories[path];
                }
            }

            return null;
        }
    }
}
