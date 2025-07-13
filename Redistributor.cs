using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIKA_AUDIO
{
    public static class Redistributor
    {
        public static float[] Redistribute(
            float[] balls,
            float[] currentBoxes,
            float[] idealBoxes)
        {
            // 1. Считаем "ступеньки" (CDF) для текущих и идеальных коробок
            float[] cdfCurrent = CalculateCdf(currentBoxes);
            float[] cdfIdeal = CalculateCdf(idealBoxes);

            // 2. Перемещаем каждый шарик
            float[] newBalls = new float[balls.Length];
            for (int i = 0; i < balls.Length; i++)
            {
                float ball = balls[i];
                int boxIndex = FindBoxIndex(cdfCurrent, ball);
                newBalls[i] = MoveBallToNewBox(cdfCurrent, cdfIdeal, boxIndex, ball);
            }

            return newBalls;
        }

        // Считаем "ступеньки" (CDF)
        private static float[] CalculateCdf(float[] boxes)
        {
            float[] cdf = new float[boxes.Length];
            cdf[0] = boxes[0];
            for (int i = 1; i < boxes.Length; i++)
                cdf[i] = cdf[i - 1] + boxes[i];
            return cdf;
        }

        // Находим, в какую коробку попадает шарик
        private static int FindBoxIndex(float[] cdf, float ball)
        {
            for (int i = 0; i < cdf.Length; i++)
                if (ball <= cdf[i])
                    return i;
            return cdf.Length - 1;
        }

        // Переносим шарик в новую коробку
        private static float MoveBallToNewBox(
            float[] cdfCurrent,
            float[] cdfIdeal,
            int boxIndex,
            float ball)
        {
            if (boxIndex == 0)
                return ball * (cdfIdeal[0] / cdfCurrent[0]);

            float lowerCurrent = cdfCurrent[boxIndex - 1];
            float upperCurrent = cdfCurrent[boxIndex];
            float progress = (ball - lowerCurrent) / (upperCurrent - lowerCurrent);

            float lowerIdeal = cdfIdeal[boxIndex - 1];
            float upperIdeal = cdfIdeal[boxIndex];
            return lowerIdeal + progress * (upperIdeal - lowerIdeal);
        }
    }
}
