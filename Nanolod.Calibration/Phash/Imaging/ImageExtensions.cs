using System;

namespace Nanolod.Calibration.Phash.Imaging
{
    public static class ImageExtensions
    {
        public static void TransposeTo<T>(this IImage<T> source, IImage<T> dest)
            where T : struct, IEquatable<T>
        {
            int w = source.Width;
            int h = source.Height;

            if (dest.Height != w || dest.Width != h)
            {
                throw new ArgumentException();
            }

            T[] sa = (source as IArrayImage<T>)?.Array;
            T[] da = (dest as IArrayImage<T>)?.Array;

            if (sa != null && da != null)
            {
                int i = 0;
                for (int sy = 0; sy < h; sy++)
                {
                    for (int sx = 0; sx < w; sx++)
                    {
                        da[sy + h * sx] = sa[i++];
                    }
                }
            }
            else
            {
                for (int sy = 0; sy < h; sy++)
                {
                    for (int sx = 0; sx < w; sx++)
                    {
                        dest[sy, sx] = source[sx, sy];
                    }
                }
            }
        }
    }
}