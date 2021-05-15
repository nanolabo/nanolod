using System;
using System.Numerics;

namespace Nanolod.Calibration.Phash.Imaging
{
    internal class ByteImageOperations<T> : IByteImageOperations
        where T : IByteImageWrapper
    {
        public FloatImage Convolve(T image, FloatImage kernel)
        {
            int vc = Vector<float>.Count;
            if (Vector.IsHardwareAccelerated
                && vc > 1
                && kernel.Width > 1)
            {
                return ConvolveVector(image, PackKernel(kernel, vc), kernel.Width, kernel.Height);
            }

            return ConvolveSingle(image, kernel, kernel.Width, kernel.Height);
        }

        private static float[] PackKernel(FloatImage kernel, int vectorSize)
        {
            int rowSize = (kernel.Width + vectorSize - 1) / vectorSize;
            if (rowSize * vectorSize == kernel.Width)
            {
                return kernel.Array;
            }
            else
            {
                float[] ka = new float[vectorSize * rowSize * kernel.Height];
                for (int y = 0; y < kernel.Height; y++)
                {
                    Array.Copy(kernel.Array, y * kernel.Width, ka, y * vectorSize * rowSize, kernel.Width);
                }
                return ka;
            }
        }

        internal static FloatImage ConvolveSingle(T image, FloatImage kernel, int kernelWidth, int kernelHeight)
        {
            int kernelXRadius = kernel.Width >> 1;
            int kernelYRadius = kernelHeight >> 1;

            FloatImage r = new FloatImage(image.Width, image.Height);
            float total = kernel.Sum();

            for (int dy = 0; dy < image.Height; dy++)
            {
                for (int dx = 0; dx < image.Width; dx++)
                {
                    float v = 0f;
                    float sum = 0f;
                    for (int ky = 0; ky < kernelHeight; ky++)
                    {
                        int sy = dy + ky - kernelYRadius;
                        if (sy < 0 || image.Height <= sy)
                        {
                            continue;
                        }

                        for (int kx = 0; kx < kernelWidth; kx++)
                        {
                            int sx = dx + kx - kernelXRadius;
                            if (sx < 0 || image.Width <= sx)
                            {
                                continue;
                            }

                            byte sv = image[sx, sy];
                            float kv = kernel[kx, ky];
                            v += sv * kv;
                            sum += kv;
                        }
                    }

                    r[dx, dy] = total == sum ? v : (v * total / sum);
                }
            }

            return r;
        }

        private static unsafe FloatImage ConvolveVector(T image, float[] kernel, int kernelWidth, int kernelHeight)
        {
            int vc = Vector<float>.Count;
            int kernelXRadius = kernelWidth >> 1;
            int kernelYRadius = kernelHeight >> 1;

            int lineSize = image.Width + kernelWidth + vc;
            float[] lines = new float[lineSize * kernelHeight];

            for (int y = 0; y < kernelHeight - kernelYRadius; y++)
            {
                LoadLine(image, kernelHeight, kernelXRadius, lines, lineSize, y);
            }

            FloatImage r = new FloatImage(image.Width, image.Height);

            float total = kernel.Sum();

            int xBatch = (kernelWidth + vc - 1) / vc;
            fixed (float* fp = kernel)
            fixed (float* _ = lines)
            {
                for (int dy = 0; dy < image.Height; dy++)
                {
                    if (dy > 0)
                    {
                        LoadLine(image, kernelHeight, kernelXRadius, lines, lineSize, dy + kernelHeight - kernelYRadius - 1);
                    }

                    bool isYEdge = dy - kernelYRadius < 0 || image.Height <= dy + kernelHeight - kernelYRadius - 1;

                    for (int dx = 0; dx < image.Width; dx++)
                    {
                        bool isEdge = isYEdge || dx - kernelXRadius < 0 || image.Width <= dx + kernelWidth - kernelXRadius - 1;

                        float result = isEdge ? ConvolveVectorPixel(image, lines, lineSize, fp, kernelWidth, kernelHeight, kernelXRadius, kernelYRadius, total, xBatch, dx, dy)
                                    : ConvolveVectorPixel(lines, lineSize, fp, kernelHeight, kernelYRadius, xBatch, dx, dy);
                        r[dx, dy] = result;
                    }
                }
            }

            return r;
        }

        private static unsafe float ConvolveVectorPixel(float[] lines, int lineSize, float* kernel, int kernelHeight, int kernelYRadius, int xBatch, int x, int y)
        {
            int vc = Vector<float>.Count;
            float v = 0f;
            for (int ky = 0; ky < kernelHeight; ky++)
            {
                int sy = y + ky - kernelYRadius;

                int ly = (sy % kernelHeight) * lineSize;
                for (int ri = 0; ri < xBatch; ri++)
                {
                    int sx = x + ri * vc;
                    Vector<float> kv = ((Vector<float>*)kernel)[ri + ky * xBatch];
                    Vector<float> vv = new Vector<float>(lines, ly + sx);

                    float dv = Vector.Dot(vv, kv);
                    v += dv;
                }
            }

            return v;
        }

        private static unsafe float ConvolveVectorPixel(T image, float[] lines, int lineSize, float* kernel, int kernelWidth, int kernelHeight, int kernelXRadius, int kernelYRadius, float kernelSum, int xBatch, int x, int y)
        {
            int vc = Vector<float>.Count;
            float v = 0f;
            float sum = 0f;
            float* bases = stackalloc float[Vector<float>.Count];
            for (int ky = 0; ky < kernelHeight; ky++)
            {
                int sy = y + ky - kernelYRadius;
                if (sy < 0 || image.Height <= sy)
                {
                    continue;
                }

                int ly = (sy % kernelHeight) * lineSize;
                for (int ri = 0; ri < xBatch; ri++)
                {
                    int sx = x + ri * vc - kernelXRadius;
                    Vector<float> kv = ((Vector<float>*)kernel)[ri + ky * xBatch];
                    Vector<float> vv = new Vector<float>(lines, ly + sx + kernelXRadius);
                    Vector<float> bv;
                    if (kernelXRadius <= x && x < kernelWidth - kernelXRadius)
                    {
                        bv = new Vector<float>(1);
                    }
                    else
                    {
                        for (int i = 0; i < vc; i++)
                        {
                            bases[i] = sx + i < 0 || image.Width <= sx + i ? 0 : 1;
                        }
                        bv = *(Vector<float>*)bases;
                    }

                    float dv = Vector.Dot(vv, kv);
                    float ds = Vector.Dot(bv, kv);
                    v += dv;
                    sum += ds;
                }
            }

            float result = kernelSum == sum ? v : (v * kernelSum / sum);
            return result;
        }

        private static void LoadLine(T image, int kernelHeight, int kernelXRadius, float[] lines, int lineSize, int y)
        {
            int i = ((y + kernelHeight) % kernelHeight) * lineSize;

            if (0 <= y && y < image.Height)
            {
                for (int x = 0; x < lineSize; x++)
                {
                    int sx = x - kernelXRadius;
                    lines[i++] = 0 <= sx && sx < image.Width ? image[sx, y] : 0;
                }
            }
        }

        FloatImage IByteImageOperations.Convolve(IByteImageWrapper image, FloatImage kernel)
            => Convolve((T)image, kernel);
    }
}
