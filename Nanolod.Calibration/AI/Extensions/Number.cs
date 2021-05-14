using System;

namespace Nanolod.Calibration.Extensions
{
    public class Number
    {
        private static readonly Random rand = new Random(DateTime.Now.Millisecond);

        public static double Rnd(double multiplicand = 1)
        {
            return rand.NextDouble() * multiplicand;
        }
    }
}
