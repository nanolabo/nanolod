﻿using System;
using MathF = Nanomesh.MathF;
using Vector3 = System.Numerics.Vector3;

namespace Nanolod.Calibration.Phash.Imaging
{
    public static class ColorHelper
    {
        private static byte ToByte(this int i)
            => (byte)Math.Max(byte.MinValue, Math.Min(i, byte.MaxValue));

        private static byte ToByte(this float f)
            => (byte)Math.Max(byte.MinValue, Math.Min(MathF.Round(f), byte.MaxValue));

        public static byte GetIntensity(this Vector3 rgb)
            => Vector3.Dot(rgb, new Vector3(1 / 3f)).ToByte();

        public static byte GetLuminance(this Vector3 rgb)
            => (((int)(Math.Round(Vector3.Dot(rgb, new Vector3(66, 129, 25))) + 128) >> 8) + 16).ToByte();
    }
}
