using System;
using System.Numerics;
using System.Runtime;
using MathF = Nanomesh.MathF;

namespace Nanolod.Calibration.Phash.Imaging
{
    public sealed partial class FloatImage : IArrayImage<float>
    {
        public FloatImage(int width, int height)
        {
            Width = width;
            Height = height;
            Array = new float[width * height];
        }

        public FloatImage(int width, int height, float value)
        {
            Width = width;
            Height = height;
            Array = new float[width * height];
            for (int i = 0; i < Array.Length; i++)
            {
                Array[i] = value;
            }
        }

        public FloatImage(int width, int height, float[] data)
        {
            Width = width;
            Height = height;
            Array = data;
        }

        public int Width
        {
#if !NO_SERIALIZABLE
            [TargetedPatchingOptOut("")]
#endif
            get;
        }

        public int Height
        {
#if !NO_SERIALIZABLE
            [TargetedPatchingOptOut("")]
#endif
            get;
        }

        public float[] Array
        {
#if !NO_SERIALIZABLE
            [TargetedPatchingOptOut("")]
#endif
            get;
        }

        public float this[int x, int y]
        {
#if !NO_SERIALIZABLE
            [TargetedPatchingOptOut("")]
#endif
            get => Array[x + y * Width];
#if !NO_SERIALIZABLE
            [TargetedPatchingOptOut("")]
#endif
            set => Array[x + y * Width] = value;
        }

        public FloatImage Resize(int w, int h)
        {
            // TODO:bilinearにする

            FloatImage r = new FloatImage(w, h);
            float xr = w / (float)Width;
            float yr = h / (float)Height;
            for (int sy = 0; sy < Height; sy++)
            {
                int dy = (int)Math.Max(0, Math.Min(sy * yr, h - 1));
                for (int sx = 0; sx < Width; sx++)
                {
                    int dx = (int)Math.Max(0, Math.Min(sx * xr, w - 1));

                    r[dx, dy] += this[sx, sy];
                }
            }

            return r;
        }

        public void ApplyGamma(float gamma)
        {
            for (int i = 0; i < Array.Length; i++)
            {
                Array[i] = MathF.Pow(Array[i], gamma);
            }
        }

        public static FloatImage operator *(FloatImage image, float coefficient)
        {
            float[] d = new float[image.Array.Length];
            Multiply(image.Array, d, coefficient);
            return new FloatImage(image.Width, image.Height, d);
        }

        public static FloatImage operator *(float coefficient, FloatImage image)
            => image * coefficient;

        public static FloatImage operator /(FloatImage image, float divider)
            => image * (1 / divider);

        public void MultiplyInplace(float coefficient)
        {
            Multiply(Array, Array, coefficient);
        }

        private static void Multiply(float[] source, float[] dest, float coefficient)
        {
            int vc = Vector<float>.Count;
            if (vc > 1 && Vector.IsHardwareAccelerated)
            {
                for (int i = 0; i < source.Length;)
                {
                    int ni = i + vc;
                    if (ni <= source.Length)
                    {
                        (new Vector<float>(source, i) * coefficient).CopyTo(dest, i);
                        i = ni;
                    }
                    else
                    {
                        dest[i] = source[i] * coefficient;
                        i++;
                    }
                }
            }
            else
            {
                for (int i = 0; i < source.Length; i++)
                {
                    dest[i] = source[i] * coefficient;
                }
            }
        }

        public void DivideInplace(float divider)
            => MultiplyInplace(1 / divider);

        public FloatImage Multiply(FloatImage other)
        {
            if (other.Width < Width || other.Height < Height)
            {
                throw new InvalidOperationException();
            }

            int vc = Vector<float>.Count;
            if (vc > 1 && Vector.IsHardwareAccelerated)
            {
                float[] oa = other.Array;
                float[] d = new float[Width * Height];
                if (oa.Length == d.Length)
                {
                    for (int i = 0; i < d.Length;)
                    {
                        int ni = i + vc;
                        if (ni <= Array.Length)
                        {
                            (new Vector<float>(Array, i) * new Vector<float>(oa, i)).CopyTo(d, i);
                            i = ni;
                        }
                        else
                        {
                            d[i] = Array[i] * oa[i];
                            i++;
                        }
                    }
                }
                else
                {
                    int i = 0;
                    for (int y = 0; y < Height; y++)
                    {
                        int j = y * other.Width;
                        for (int x = 0; x < Width;)
                        {
                            int ni = x + vc;
                            if (ni <= Width)
                            {
                                (new Vector<float>(Array, i) * new Vector<float>(oa, j)).CopyTo(d, i);
                                i += vc;
                                j += vc;
                                x = ni;
                            }
                            else
                            {
                                d[i] = Array[i] * oa[j];
                                i++;
                                j++;
                                x++;
                            }
                        }
                    }
                }
                return new FloatImage(Width, Height, d);
            }

            FloatImage r = new FloatImage(Width, Height);

            for (int sy = 0; sy < Height; sy++)
            {
                for (int sx = 0; sx < Width; sx++)
                {
                    r[sx, sy] = this[sx, sy] * other[sx, sy];
                }
            }
            return r;
        }

        public unsafe FloatImage MatrixMultiply(FloatImage other, bool isTransposed = false)
        {
            if (Width != (isTransposed ? other.Width : other.Height))
            {
                throw new InvalidOperationException();
            }

            FloatImage r = new FloatImage(isTransposed ? other.Height : other.Width, Height);
            int vc = Vector<float>.Count;
            if (Width >= vc
                && vc > 1
                && Vector.IsHardwareAccelerated)
            {
                FloatImage transp = isTransposed ? other : other.Transpose();

                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < transp.Height; x++)
                    {
                        float v = 0f;
                        for (int i = 0; i < Width;)
                        {
                            int ni = i + vc;
                            if (ni <= Width)
                            {
                                v += Vector.Dot(
                                        new Vector<float>(Array, y * Width + i),
                                        new Vector<float>(transp.Array, x * Width + i));

                                i = ni;
                            }
                            else
                            {
                                v += this[i, y] * transp[i, x];
                                i++;
                            }
                        }

                        r[x, y] = v;
                    }
                }
            }
            else if (isTransposed)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < other.Height; x++)
                    {
                        float v = 0f;
                        for (int i = 0; i < Width; i++)
                        {
                            v += this[i, y] * other[i, x];
                        }
                        r[x, y] = v;
                    }
                }
            }
            else
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < other.Width; x++)
                    {
                        float v = 0f;
                        for (int i = 0; i < Width; i++)
                        {
                            v += this[i, y] * other[x, i];
                        }
                        r[x, y] = v;
                    }
                }
            }

            return r;
        }

        public static FloatImage CreateGaussian(int radius, float sigma)
        {
            int r = radius > 0 ? radius : (int)MathF.Round(3 * sigma);
            int w = 2 * r + 1;

            FloatImage vs = new FloatImage(w, w);
            float s2 = sigma * sigma;
            float i2s2 = 0.5f / s2;
            float i2pis2 = 1 / (2 * MathF.PI * s2);
            for (int y = 0; y <= r; y++)
            {
                for (int x = y; x <= r; x++)
                {
                    int d2 = x * x + y * y;
                    float v = MathF.Exp(-d2 * i2s2) * i2pis2;
                    vs[r - y, r - x] = v;
                    vs[r - y, r + x] = v;
                    vs[r + y, r - x] = v;
                    vs[r + y, r + x] = v;
                    if (x != y)
                    {
                        vs[r - x, r - y] = v;
                        vs[r - x, r + y] = v;
                        vs[r + x, r - y] = v;
                        vs[r + x, r + y] = v;
                    }
                }
            }

            return vs;
        }

        public float Max()
            => Array.Max();

        public float Min()
        {
            float r = 0;
            for (int i = 0; i < Array.Length; i++)
            {
                r = Math.Min(Array[i], r);
            }
            return r;
        }

        public float Sum()
            => Array.Sum();

        public FloatImage Transpose()
        {
            FloatImage r = new FloatImage(Height, Width);
            this.TransposeTo(r);
            return r;
        }
    }
}
