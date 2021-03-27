using ImageMagick;
using System;
using System.Drawing;
using System.IO;

namespace Ranger
{
    static class ImageLoader
    {
        public static Image Load(string path, int width=0, int height=0, ThumbnailCache cache=null)
        {
            if (width != 0 && height != 0 && cache != null)
            {
                Image cachedThumbnail = cache.Get(path);
                if (cachedThumbnail != null)
                {
                    return cachedThumbnail;
                }
            }

            string extension = Path.GetExtension(path).ToLower();

            if (extension == ".afdesign" ||
                extension == ".afpub" ||
                extension == ".afphoto")
            {
                var bytes = File.ReadAllBytes(path);
                byte[] header = { 137, 80, 78, 71, 13, 10, 26, 10 };

                int max = bytes.Length - 8;
                for (int i = 0; i < max; i++)
                {
                    if (bytes[i + 0] == header[0] &&
                        bytes[i + 1] == header[1] &&
                        bytes[i + 2] == header[2] &&
                        bytes[i + 3] == header[3] &&
                        bytes[i + 4] == header[4] &&
                        bytes[i + 5] == header[5] &&
                        bytes[i + 6] == header[6] &&
                        bytes[i + 7] == header[7])
                    {
                        int pngSize = bytes.Length - i;
                        byte[] pngBytes = new byte[pngSize];

                        Array.Copy(bytes, i, pngBytes, 0, pngSize);

                        using (var magickImage = new MagickImage(pngBytes, MagickFormat.Png))
                        {
                            if (width != 0 && height != 0)
                            {
                                return CacheAndReturnImage(cache, path, CreateThumbnail(magickImage, width, height));
                            }
                            else
                            {
                                return CacheAndReturnImage(cache, path, magickImage.ToBitmap());
                            }
                        }
                    }
                }

                return null;
            }
            else
            {
                try
                {
                    using (var magickImage = new MagickImage(path))
                    {
                        magickImage.AutoOrient();

                        if (width != 0 && height != 0)
                        {
                            return CacheAndReturnImage(cache, path, CreateThumbnail(magickImage, width, height));
                        }
                        else
                        {
                            return CacheAndReturnImage(cache, path, magickImage.ToBitmap());
                        }
                    }
                }
                catch
                {
                    if (width != 0 && height != 0)
                    {
                        var badBitmap = new Bitmap(width, height);
                        return CacheAndReturnImage(cache, path, badBitmap);
                    }
                    else
                    {
                        return CacheAndReturnImage(cache, path, new Bitmap(64,64));
                    }
                }
            }
        }

        private static Image CacheAndReturnImage(ThumbnailCache cache, string path, Image image)
        {
            if (cache != null)
            {
                cache.Set(path, image);
            }

            return image;
        }

        private static Image CreateThumbnail(MagickImage magickImage, int width, int height)
        {
            magickImage.Thumbnail(width, height);
            var bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(bmp))
            {
                using (var magicBitmap = magickImage.ToBitmap())
                {
                    g.DrawImage(magicBitmap, (width - magickImage.Width) / 2, (height - magicBitmap.Height) / 2);
                }
            }

            return bmp;
        }

        public static Image LoadFolderThumbnail(int width, int height, ThumbnailCache cache)
        {
            if (cache != null)
            {
                Image cachedThumbnail = cache.Get("FOLDER");
                if (cachedThumbnail != null)
                {
                    return cachedThumbnail;
                }
            }

            var bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Image img = Properties.Resources.Folder;
            int size = Math.Min(width, height);

            using (var g = Graphics.FromImage(bmp))
            {
                g.DrawImage(img, (width - size) / 2, (height - size) / 2, size, size);
            }

            if (cache != null)
            {
                cache.Set("FOLDER", bmp);
            }

            return bmp;
        }
    }
}
