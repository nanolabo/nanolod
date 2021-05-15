using System;

namespace Nanolod.Calibration.Phash.Imaging
{
    public interface IImage<T>
        where T : struct, IEquatable<T>
    {
        int Width { get; }

        int Height { get; }

        T this[int x, int y] { get; set; }
    }
}