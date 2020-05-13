using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSollaris
{
    class WindowCalculatorHelper
    {
        double windowHeight;
        double windowWidth;
        long horizontalRange;
        double scale;
        long[] viewCenter;
        long[] translationVector;

        public long[] ViewCenter { get => viewCenter; set => viewCenter = value; }
        public long HorizontalRange { get => horizontalRange; set => horizontalRange = value; }

        public WindowCalculatorHelper(double height, double width, long range, long[] center)
        {
            windowHeight = height;
            windowWidth = width;
            horizontalRange = range;
            viewCenter = center;

            scale = CalculateNewScale();
            translationVector = CalculateTranslationVector(viewCenter);
        }

        private long[] CalculateTranslationVector(long[] center)
        {
            long[] result = new long[2];
            result[0] = -center[0];
            result[1] = -center[1];
            return result;
        }

        private double CalculateNewScale()
        {
            return windowWidth / horizontalRange;
        }

        public double[] CalculateScreenPosition(long[] coordinates)
        {
            double[] result = new double[2];
            result[0] = windowWidth / 2 + scale * (coordinates[0] + translationVector[0]);
            result[1] = windowHeight / 2 - scale * (coordinates[1] + translationVector[1]);
            return result;
        }

        public void ChangeScale(long range)
        {
            horizontalRange = range;
            scale = CalculateNewScale();
        }

        public void ChangeCenter(long[] center)
        {
            viewCenter = center;
            translationVector = CalculateTranslationVector(viewCenter);
        }
    }
}
