﻿using Nanolod.Calibration.Phash.Imaging;

namespace Nanolod.Calibration.Phash
{
    /// <summary>
    /// Radon Projection info
    /// </summary>
    public class Projections
    {
        public Projections(int regionWidth, int regionHeight, int lineCount)
        {
            Region = new Imaging.FloatImage(regionWidth, regionHeight);
            PixelsPerLine = new int[lineCount];
        }

        /// <summary>
        /// contains projections of image of angled lines through center
        /// </summary>
        public FloatImage Region { get; }

        /// <summary>
        /// int array denoting the number of pixels of each line
        /// </summary>
        public int[] PixelsPerLine { get; }
    }
}
