﻿using System;
using System.Text;

namespace Nanolod.Calibration.Phash
{
    [Serializable]
    public class Digest
    {
        internal const int LENGTH = 40;

        public Digest()
        {
            _Coefficients = new byte[LENGTH];
        }

        [Obsolete("Use Coefficients instead")]
        public byte[] Coefficents
        {
            get => Coefficients;
            set => Coefficients = value;
        }

        private readonly byte[] _Coefficients;

        /// <summary>
        /// the digest integer coefficient array
        /// </summary>
        public byte[] Coefficients
        {
            get => _Coefficients;
            set
            {
                if (value == null)
                {
                    Array.Clear(_Coefficients, 0, _Coefficients.Length);
                }
                else if (value.Length == _Coefficients.Length)
                {
                    Array.Copy(value, _Coefficients, value.Length);
                }
                else
                {
                    throw new ArgumentException();
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(Coefficients.Length * 2 + 2);
            sb.Append("0x");
            foreach (byte b in Coefficients)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }
    }
}