using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIKA_IMBANIKA_AUDIO
{
    class FFT
    {
        private static bool IsPowerOfTwo(int n) =>
        n > 0 && (n & (n - 1)) == 0;

        public static Complex[] Do(Complex[] buffer, out double maxMagnitude, bool inverse = false)
        {
            int n = buffer.Length;
            if (!IsPowerOfTwo(n))
                throw new ArgumentException("Array length must be power of 2");

            if (inverse)
            {
                for (int i = 0; i < n; i++)
                {
                    if (i == 0 || i == n / 2) // DC и частота Найквиста
                    {
                        buffer[i].Real = buffer[i].Real * n;
                        buffer[i].Imaginary = buffer[i].Imaginary * n;
                    }
                    else
                    {
                        buffer[i].Real = buffer[i].Real * (n / 2);
                        buffer[i].Imaginary = buffer[i].Imaginary * (n / 2);
                    }
                }
            }

            // Bit-reversal permutation
            for (int i = 1, j = 0; i < n; i++)
            {
                int bit = n >> 1;
                for (; (j & bit) != 0; bit >>= 1)
                    j ^= bit;
                j ^= bit;

                if (i < j)
                    (buffer[i], buffer[j]) = (buffer[j], buffer[i]);
            }

            // Butterfly operations
            for (int len = 2; len <= n; len <<= 1)
            {
                double angle = 2 * Math.PI / len * (inverse ? -1 : 1);
                Complex w = new Complex(Math.Cos(angle), Math.Sin(angle));

                for (int i = 0; i < n; i += len)
                {
                    Complex currentW = new Complex(1, 0);
                    for (int j = 0; j < len / 2; j++)
                    {
                        int offset = i + j;
                        Complex u = buffer[offset];
                        Complex v = buffer[offset + len / 2] * currentW;

                        buffer[offset] = u + v;
                        buffer[offset + len / 2] = u - v;
                        currentW *= w;
                    }
                }
            }

            if (inverse)
            {
                for (int i = 0; i < n; i++)
                {
                    buffer[i].Real /= n;
                    buffer[i].Imaginary /= n;
                }
            }
            else
            {
                for (int i = 0; i < n; i++)
                {
                    if (i == 0 || i == n / 2) // DC и частота Найквиста
                    {
                        buffer[i].Real = buffer[i].Real / n;
                        buffer[i].Imaginary = buffer[i].Imaginary / n;
                    }
                    else
                    {
                        buffer[i].Real = buffer[i].Real / (n / 2);
                        buffer[i].Imaginary = buffer[i].Imaginary / (n / 2);
                    }
                }
            }

            // Calculate other
            maxMagnitude = 0;

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i].Magnitude = buffer[i].CalcMagnitude;
                buffer[i].Phase = buffer[i].CalcPhase;

                if (maxMagnitude < buffer[i].Magnitude)
                    maxMagnitude = buffer[i].Magnitude;

                if (double.IsNaN(buffer[i].Magnitude))
                {

                }
            }

            return buffer;
        }

        public static Complex[] IFFT(Complex[] buffer) => Do(buffer, out double maxAmplitude, inverse: true);

        public static double[] PadToPowerOfTwo(double[] input)
        {
            int newSize = 1;
            while (newSize < input.Length)
                newSize <<= 1;

            var padded = new double[newSize];
            Array.Copy(input, padded, input.Length);
            return padded;
        }
    }
}
