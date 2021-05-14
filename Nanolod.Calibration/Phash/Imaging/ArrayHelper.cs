using System;
using System.Linq;
using System.Numerics;

namespace Nanolod.Calibration.Phash.Imaging
{
    internal static class ArrayHelper
    {
        public static unsafe float Max(this float[] array)
        {
            int vc = Vector<float>.Count;

            if (vc > 1
                && Vector.IsHardwareAccelerated
                && array.Length >= 2 * vc)
            {
                Vector<float> mv = new Vector<float>(array, 0);
                int i = vc;
                for (; i < array.Length;)
                {
                    int ni = i + vc;
                    if (ni <= array.Length)
                    {
                        mv = Vector.Max(mv, new Vector<float>(array, i));
                        i = ni;
                    }
                    else
                    {
                        break;
                    }
                }

                float max = mv.MaxComponent();

                for (; i < array.Length; i++)
                {
                    max = Math.Max(max, array[i]);
                }

                return max;
            }
            else
            {
                return Enumerable.Max(array);
            }
        }

        public static float Sum(this float[] array)
        {
            float r = 0f;
            int i = 0;
            int vc = Vector<float>.Count;

            if (vc > 1
                && Vector.IsHardwareAccelerated
                && array.Length >= vc)
            {
                Vector<float> sum = new Vector<float>(array, 0);
                i = vc;
                for (; i < array.Length;)
                {
                    int ni = i + vc;
                    if (ni <= array.Length)
                    {
                        sum = Vector.Add(sum, new Vector<float>(array, i));
                        i = ni;
                    }
                    else
                    {
                        break;
                    }
                }
                r = Vector.Dot(sum, new Vector<float>(1));
            }

            for (; i < array.Length; i++)
            {
                r += array[i];
            }

            return r;
        }
    }
}