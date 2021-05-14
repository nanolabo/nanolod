using System.Numerics;

namespace Nanolod.Calibration.Phash.Imaging
{
    public class Bgr24Image : IVector3Image
    {
        // TODO: use Span<byte> if the System.Memory.dll released
        internal readonly byte[] _data;

        internal readonly int _offset;
        internal readonly int _stride;
        internal readonly int _pixelSize;

        public Bgr24Image(int width, int height, byte[] data, int offset, int stride, int pixelSize)
        {
            Width = width;
            Height = height;
            _data = data;
            _offset = offset;
            _stride = stride;
            _pixelSize = pixelSize;
        }

        public int Width { get; }

        public int Height { get; }

        public Vector3 this[int x, int y]
        {
            get
            {
                int i = _offset + y * _stride + x * _pixelSize;
                return new Vector3(_data[i + 2], _data[i + 1], _data[i]);
            }
        }
    }
}