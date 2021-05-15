using System;

namespace Nanolod.Calibration.Phash.Imaging
{
    internal interface IArrayImage<T> : IImage<T>
        where T : struct, IEquatable<T>
    {
        T[] Array { get; }
    }
}