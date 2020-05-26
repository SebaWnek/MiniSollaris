using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSollaris
{
    /// <summary>
    /// Class converting world coordinates into canvas coordinates
    /// </summary>
    class WindowCalculatorHelper
    {
        readonly double windowHeight;
        readonly double windowWidth;
        long horizontalRange;
        double scale;
        long[] viewCenter;
        long[] translationVector;

        public long[] ViewCenter { get => viewCenter; set => viewCenter = value; }
        public long HorizontalRange { get => horizontalRange; set => horizontalRange = value; }

        /// <summary>
        /// Constructor setting everything up
        /// </summary>
        /// <param name="height">Height of canvas which will be used as background to position all objects</param>
        /// <param name="width">Width of canvas which will be used as background to position all objects</param>
        /// <param name="range">Horizontal size of visible part of simulation</param>
        /// <param name="center">World coordinates of center of visible part of simulation</param>
        public WindowCalculatorHelper(double height, double width, long range, long[] center)
        {
            windowHeight = height;
            windowWidth = width;
            horizontalRange = range;
            viewCenter = center;

            scale = CalculateNewScale();
            translationVector = CalculateTranslationVector(viewCenter);
        }

        /// <summary>
        /// Calculate translation vector from middle of coordinate system to current view center 
        /// </summary>
        /// <param name="center">View center world coordinates</param>
        /// <returns>Translation vector from center of coordinate system to view center</returns>
        private long[] CalculateTranslationVector(long[] center)
        {
            long[] result = new long[2];
            result[0] = -center[0];
            result[1] = -center[1];
            return result;
        }

        /// <summary>
        /// Calculates screen coordinates from world coordinates.
        /// </summary>
        /// <param name="coordinates">World coordinates</param>
        /// <returns>Screen coordinates</returns>
        public double[] CalculateScreenPosition(long[] coordinates)
        {
            double[] result = new double[2];
            result[0] = windowWidth / 2 + scale * (coordinates[0] + translationVector[0]);
            result[1] = windowHeight / 2 - scale * (coordinates[1] + translationVector[1]);
            return result;
        }

        /// <summary>
        /// Calculates new scale to be used in successive calculations after changing horizontal range.
        /// </summary>
        /// <param name="range">New horizontal range</param>
        public void ChangeScale(long range)
        {
            horizontalRange = range;
            scale = CalculateNewScale();
        }

        /// <summary>
        /// Calculates new scale to be used in successive calculations.
        /// </summary>
        /// <returns>New scale</returns>
        private double CalculateNewScale()
        {
            return windowWidth / horizontalRange;
        }

        /// <summary>
        /// Changes center of view.
        /// </summary>
        /// <param name="center">World coordinates of new center of view</param>
        public void ChangeCenter(long[] center)
        {
            viewCenter = center;
            translationVector = CalculateTranslationVector(viewCenter);
        }
    }
}
