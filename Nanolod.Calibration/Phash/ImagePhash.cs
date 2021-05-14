using Nanolod.Calibration.Phash.Imaging;
using System;
using System.Numerics;
using MathF = Nanomesh.MathF;

namespace Nanolod.Calibration.Phash
{
    public class ImagePhash
    {
        protected ImagePhash()
        { }

        private static float GetRoundingFactor(float x)
            => x >= 0 ? 0.5f : -0.5f;

        private const float SQRT_TWO = 1.4142135623730950488016887242097f;
        protected const float DEFAULT_SIGMA = 3.5f;
        protected const float DEFAULT_GAMMA = 1.0f;
        protected const int DEFAULT_NUMBER_OF_ANGLES = 180;
        protected const float DEFAULT_THRESHOLD = 0.9f;

        #region CompareImages

        /// <summary>
        /// compare 2 images
        /// </summary>
        /// <param name="imA">CImg object of first image</param>
        /// <param name="imB">CImg object of second image</param>
        /// <param name="pcc">double value for peak of cross correlation</param>
        /// <param name="sigma">double value for the deviation of gaussian filter</param>
        /// <param name="gamma">double value for gamma correction of images</param>
        /// <param name="numberOfAngles">int number for the number of angles of radon projections</param>
        /// <param name="threshold">double value for the threshold</param>
        /// <returns>false for different images, 1 true for same image,</returns>
        public static bool CompareImages(IByteImage imA, IByteImage imB, out float pcc, float sigma = DEFAULT_SIGMA, float gamma = DEFAULT_GAMMA, int numberOfAngles = DEFAULT_NUMBER_OF_ANGLES, float threshold = DEFAULT_THRESHOLD)
        {
            Digest digestA = ComputeDigest(imA, sigma, gamma, numberOfAngles);

            Digest digestB = ComputeDigest(imB, sigma, gamma, numberOfAngles);

            pcc = GetCrossCorrelation(digestA, digestB);
            return pcc > threshold;
        }

        #endregion CompareImages

        #region ComputeDigest

        /// <summary>
        /// Compute the image digest for an image given the input image
        /// </summary>
        /// <param name="image">CImg object representing an input image</param>
        /// <param name="sigma">double value for the deviation for a gaussian filter function</param>
        /// <param name="gamma">double value for gamma correction on the input image</param>
        /// <param name="numberOfAngles">int value for the number of angles to consider.</param>
        /// <returns></returns>
        public static Digest ComputeDigest(IByteImage image, float sigma = DEFAULT_SIGMA, float gamma = DEFAULT_GAMMA, int numberOfAngles = DEFAULT_NUMBER_OF_ANGLES)
        {
            FloatImage blurred = image.Blur(sigma);

            blurred.DivideInplace(blurred.Max());
            blurred.ApplyGamma(gamma);

            Projections projs = FindRadonProjections(blurred, numberOfAngles);
            float[] features = ComputeFeatureVector(projs);

            return ComputeDct(features);
        }

        #endregion ComputeDigest

        /// <summary>
        /// return dct matrix, C Return DCT matrix of square size, <paramref name="size" />
        /// </summary>
        /// <param name="size">int denoting the size of the square matrix to create.</param>
        /// <returns>size <paramref name="size" />x<paramref name="size" /> containing the dct matrix</returns>
        internal static FloatImage CreateDctMatrix(int size)
        {
            FloatImage ret = new FloatImage(size, size, 1 / (float)Math.Sqrt(size));

            float c1 = MathF.Sqrt(2f / size);
            float rad = MathF.PI / 2 / size;
            for (int x = 0; x < size; x++)
            {
                for (int y = 1; y < size; y++)
                {
                    ret[x, y] = c1 * MathF.Cos(rad * y * (2 * x + 1));
                }
            }

            return ret;
        }

        /// <summary>
        /// Compute the dct of a given vector
        /// </summary>
        /// <param name="featureVector">vector of input series</param>
        /// <returns>the dct of R</returns>
        internal static Digest ComputeDct(float[] featureVector)
        {
            int N = featureVector.Length;

            float[] R = featureVector;

            float[] coefficients = new float[Digest.LENGTH];
            float max = 0f;
            float min = 0f;
            float div2n = MathF.PI / (2 * N);
            float divSq = 1 / MathF.Sqrt(N);
            for (int k = 0; k < Digest.LENGTH; k++)
            {
                float sum = 0f;
                for (int n = 0; n < N; n++)
                {
                    sum += R[n] * MathF.Cos((2 * n + 1) * k * div2n);
                }

                float v = coefficients[k] = sum * divSq * (k == 0 ? 1 : SQRT_TWO);
                max = Math.Max(max, v);
                min = Math.Min(min, v);
            }

            return NormalizeDigest(coefficients, max, min);
        }

        internal static Digest NormalizeDigest(float[] coefficients, float max, float min)
        {
            Digest digest = new Digest();
            float coeff = byte.MaxValue / (max - min);
            byte[] dest = digest.Coefficients;

            int i = 0;
            int vc = Vector<float>.Count;
            if (Vector.IsHardwareAccelerated
                && vc > 1)
            {
                Vector<float> minV = new Vector<float>(min);
                for (; i < Digest.LENGTH;)
                {
                    int ni = i + vc;
                    if (ni <= Digest.LENGTH)
                    {
                        Vector<float> d = (new Vector<float>(coefficients, i) - minV) * coeff;
                        for (int j = 0; j < vc; j++)
                        {
                            dest[i + j] = (byte)d[j];
                        }
                        i = ni;
                    }
                }
            }

            for (; i < Digest.LENGTH; i++)
            {
                dest[i] = (byte)((coefficients[i] - min) * coeff);
            }

            return digest;
        }

        #region ComputeDctHash

        private static FloatImage _DctMatrix;

        /// <summary>
        /// compute dct robust image hash
        /// </summary>
        /// <param name="image">An image to compute DCT hash.</param>
        /// <returns>hash of type ulong</returns>
        public static ulong ComputeDctHash(IByteImage image)
        {
            FloatImage img = image.Convolve(new FloatImage(7, 7, 1));

            FloatImage resized = img.Resize(32, 32);
            FloatImage coeff = _DctMatrix ?? (_DctMatrix = CreateDctMatrix(32));
            FloatImage dctImage = coeff.MatrixMultiply(resized).MatrixMultiply(coeff, isTransposed: true);

            float median = GetMedianOf64(dctImage);

            ulong r = 0ul;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    r |= dctImage[x + 1, y + 1] > median ? (1ul << (x + 8 * y)) : 0;
                }
            }

            return r;
        }

        internal static float GetMedianOf64(FloatImage dctImage)
        {
            float[] buf = new float[32];
            int count = 0;
            for (int y = 1; y <= 8; y++)
            {
                for (int x = 1; x <= 8; x++)
                {
                    float v = dctImage[x, y];
                    int i = Array.BinarySearch(buf, 0, count, v);
                    if (i < 0)
                    {
                        i = ~i;
                    }
                    if (i < buf.Length)
                    {
                        for (int j = Math.Min(count - 1, buf.Length - 2); j >= i; j--)
                        {
                            buf[j + 1] = buf[j];
                        }
                        buf[i] = v;
                        count = Math.Min(buf.Length, count + 1);
                    }
                }
            }
            return buf[buf.Length - 1];
        }

        #endregion ComputeDctHash

        /// <summary>
        /// Find radon projections of N lines running through the image center for lines angled 0 to 180 degrees from horizontal.
        /// </summary>
        /// <param name="img">CImg src image</param>
        /// <param name="numberOfLines">int number of angled lines to consider.</param>
        /// <returns>Projections struct</returns>
        internal static Projections FindRadonProjections(FloatImage img, int numberOfLines)
        {
            int width = img.Width;
            int height = img.Height;
            int D = (width > height) ? width : height;
            float x_center = width / 2f;
            float y_center = height / 2f;
            int x_off = (int)MathF.Floor(x_center + GetRoundingFactor(x_center));
            int y_off = (int)MathF.Floor(y_center + GetRoundingFactor(y_center));

            Projections projs = new Projections(numberOfLines, D, numberOfLines);

            FloatImage radonMap = projs.Region;
            int[] ppl = projs.PixelsPerLine;

            for (int k = 0; k < numberOfLines / 4 + 1; k++)
            {
                float theta = k * MathF.PI / numberOfLines;
                float alpha = MathF.Tan(theta);
                for (int x = 0; x < D; x++)
                {
                    float y = alpha * (x - x_off);
                    int yd = (int)MathF.Floor(y + GetRoundingFactor(y));
                    if ((yd + y_off >= 0) && (yd + y_off < height) && (x < width))
                    {
                        radonMap[k, x] = img[x, yd + y_off];
                        ppl[k] += 1;
                    }
                    if ((yd + x_off >= 0) && (yd + x_off < width) && (k != numberOfLines / 4) && (x < height))
                    {
                        radonMap[numberOfLines / 2 - k, x] = img[yd + x_off, x];
                        ppl[numberOfLines / 2 - k] += 1;
                    }
                }
            }
            int j = 0;
            for (int k = 3 * numberOfLines / 4; k < numberOfLines; k++)
            {
                float theta = k * MathF.PI / numberOfLines;
                float alpha = MathF.Tan(theta);
                for (int x = 0; x < D; x++)
                {
                    float y = alpha * (x - x_off);
                    int yd = (int)MathF.Floor(y + GetRoundingFactor(y));
                    if ((yd + y_off >= 0) && (yd + y_off < height) && (x < width))
                    {
                        radonMap[k, x] = img[x, yd + y_off];
                        ppl[k] += 1;
                    }
                    if ((y_off - yd >= 0) && (y_off - yd < width) && (2 * y_off - x >= 0) && (2 * y_off - x < height) && (k != 3 * numberOfLines / 4))
                    {
                        radonMap[k - j, x] = img[-yd + y_off, -(x - y_off) + y_off];
                        ppl[k - j] += 1;
                    }
                }
                j += 2;
            }

            return projs;
        }

        /// <summary>
        /// compute the feature vector from a radon projection map.
        /// </summary>
        /// <param name="projections">Projections struct</param>
        /// <returns>Features struct</returns>
        internal static float[] ComputeFeatureVector(Projections projections)
        {
            FloatImage map = projections.Region;
            int[] ppl = projections.PixelsPerLine;
            int N = ppl.Length;
            int D = map.Height;

            float[] fv = new float[N];

            float sum = 0f;
            float sum_sqd = 0f;
            for (int k = 0; k < N; k++)
            {
                float line_sum = 0f;
                float line_sum_sqd = 0f;
                int nb_pixels = ppl[k];
                for (int i = 0; i < D; i++)
                {
                    line_sum += map[k, i];
                    line_sum_sqd += map[k, i] * map[k, i];
                }
                fv[k] = (float)(nb_pixels > 0 ? (line_sum_sqd / nb_pixels) - (line_sum * line_sum) / (nb_pixels * nb_pixels) : 0);
                sum += fv[k];
                sum_sqd += fv[k] * fv[k];
            }
            float mean = sum / N;
            float var = 1 / MathF.Sqrt((sum_sqd / N) - (sum * sum) / (N * N));

            for (int i = 0; i < N; i++)
            {
                fv[i] = (fv[i] - mean) * var;
            }

            return fv;
        }

        /// <summary>
        /// cross correlation for 2 series. Compute the cross correlation of two series vectors
        /// </summary>
        /// <param name="x">Digest struct</param>
        /// <param name="y">Digest struct</param>
        /// <returns>double value the peak of cross correlation</returns>
        public static float GetCrossCorrelation(Digest x, Digest y)
            => GetCrossCorrelation(x.Coefficients, y.Coefficients);

        public static float GetCrossCorrelation(byte[] coefficients1, byte[] coefficients2)
            => CrossCorrelation.GetCrossCorrelationCore(coefficients1, coefficients2, Math.Min(coefficients1.Length, coefficients2.Length));

        public static unsafe float GetCrossCorrelation(byte* coefficients1, byte* coefficients2)
            => CrossCorrelation.GetCrossCorrelationCore(coefficients1, coefficients2, 40);


        internal static FloatImage CreateMHKernel(float alpha, float level)
        {
            int sigma = (int)(4 * MathF.Pow(alpha, level));

            FloatImage kernel = new FloatImage(2 * sigma + 1, 2 * sigma + 1);

            for (int y = 0; y < kernel.Width; y++)
            {
                for (int x = 0; x < kernel.Height; x++)
                {
                    float xpos = MathF.Pow(alpha, -level) * (x - sigma);
                    float ypos = MathF.Pow(alpha, -level) * (y - sigma);
                    float A = xpos * xpos + ypos * ypos;
                    kernel[x, y] = (float)((2 - A) * MathF.Exp(-A / 2));
                }
            }
            return kernel;
        }

        #region GetHammingDistance

        public static int GetHammingDistance(long x, long y)
            => GetHammingDistance(x ^ y);

        public static int GetHammingDistance(ulong x, ulong y)
            => GetHammingDistance(x ^ y);

        public static int GetHammingDistance(long v)
            => GetHammingDistance(unchecked((ulong)v));

        public static int GetHammingDistance(ulong v)
            => CrossCorrelation.GetHammingDistanceCore(v);

        #endregion GetHammingDistance
    }
}
