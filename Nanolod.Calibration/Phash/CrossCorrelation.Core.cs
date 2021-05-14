using System;
using System.Numerics;
using MathF = Nanomesh.MathF;

namespace Nanolod.Calibration.Phash
{
    public partial class CrossCorrelation
    {
        #region Internal Overloads

        internal static float GetCrossCorrelationCore(byte[] x, byte[] y, int length)
        {
            int sumX = 0;
            int sumY = 0;
            for (int i = 0; i < length; i++)
            {
                sumX += x[i];
                sumY += y[i];
            }

            float meanX = sumX / (float)length;
            float meanY = sumY / (float)length;

            float[] fx = new float[length];
            float[] fy = new float[length];

            for (int i = 0; i < length; i++)
            {
                fx[i] = x[i] - meanX;
                fy[i] = y[i] - meanY;
            }

            return GetCrossCorrelationCore(fx, fy);
        }

#if !NO_UNSAFE
        internal static unsafe float GetCrossCorrelationCore(byte* x, byte* y, int length)
        {
            int sumX = 0;
            int sumY = 0;
            for (int i = 0; i < length; i++)
            {
                sumX += x[i];
                sumY += y[i];
            }

            float meanX = sumX / (float)length;
            float meanY = sumY / (float)length;

            float[] fx = new float[length];
            float[] fy = new float[length];

            for (int i = 0; i < length; i++)
            {
                fx[i] = x[i] - meanX;
                fy[i] = y[i] - meanY;
            }

            return GetCrossCorrelationCore(fx, fy);
        }
#endif


        #endregion

        private static float GetCrossCorrelationCore(float[] x, float[] y)
        {
            float max = 0f;
            for (int d = 0; d < x.Length; d++)
            {
                float v;
                v = GetCrossCorrelationForOffset(x, y, d);
                max = Math.Max(max, v);
            }

            return MathF.Sqrt(max);
        }

        private static float GetCrossCorrelationForOffset(float[] x, float[] y, int offset)
        {
            float num = 0f;
            float denx = 0f;
            float deny = 0f;

            for (int j = 0; j < 2; j++)
            {
                int th = j == 0 ? x.Length - offset : x.Length;
                int i = j == 0 ? 0 : x.Length - offset;
                int yo = offset - j * x.Length;

                for (; i < th;)
                {
                    if (Vector<float>.Count > 1 && Vector.IsHardwareAccelerated)
                    {
                        int ni = i + Vector<float>.Count;
                        if (ni <= th)
                        {
                            Vector<float> vx = new Vector<float>(x, i);
                            Vector<float> vy = new Vector<float>(y, i + yo);
                            num += Vector.Dot(vx, vy);
                            denx += Vector.Dot(vx, vx);
                            deny += Vector.Dot(vy, vy);
                            i = ni;
                            continue;
                        }
                    }

                    float dx = x[i];
                    float dy = y[i + yo];
                    num += dx * dy;
                    denx += dx * dx;
                    deny += dy * dy;
                    i++;
                }
            }

            return num < 0 || denx == 0 || deny == 0 ? 0 : (num * num / (denx * deny));
        }

        internal static int GetHammingDistanceCore(ulong v)
        {
            unchecked
            {
                v = v - ((v >> 1) & 0x5555555555555555UL);
                v = (v & 0x3333333333333333UL) + ((v >> 2) & 0x3333333333333333UL);
                return (int)((((v + (v >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
            }
        }
    }
}