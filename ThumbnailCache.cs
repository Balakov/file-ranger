using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Ranger
{
    public class ThumbnailCache
    {
        private class ImageEntry
        {
            public DateTime m_cacheDate;
            public Image m_image;
        }

        private Dictionary<string, ImageEntry> m_cache = new Dictionary<string, ImageEntry>();
        private int c_cacheSizeLimit = 1000;

        public Image Get(string path)
        {
            if (m_cache.TryGetValue(path, out var entry))
            {
                entry.m_cacheDate = DateTime.Now;

                // Return a copy of the image so the caller can call Dispose() without destroying the cache
                return entry.m_image; //.Clone() as Image;
            }

            return null;
        }
        
        public void Set(string path, Image image)
        {
            if (m_cache.Count > c_cacheSizeLimit)
            {
                var entriesToDelete = m_cache.OrderBy(x => x.Value.m_cacheDate).Take(100).ToList();
                foreach (var entry in entriesToDelete)
                {
                    RemoveFromCache(entry.Key);
                }
            }

            if (!m_cache.ContainsKey(path))
            {
                m_cache.Add(path, new ImageEntry() 
                { 
                    m_image = image, //.Clone() as Image, // Store a copy of the image so the caller can call Dispose() without destroying the cache
                    m_cacheDate = DateTime.Now 
                });
            }
        }

        private void RemoveFromCache(string path)
        {
            if (m_cache.TryGetValue(path, out var entry))
            {
                entry.m_image.Dispose();
                m_cache.Remove(path);
            }
        }

        public void ClearCache()
        {
            foreach (var pair in m_cache)
            {
                pair.Value.m_image.Dispose();
            }

            m_cache.Clear();
        }
    }
}
