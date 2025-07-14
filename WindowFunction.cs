using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RIKA_IMBANIKA_AUDIO
{
    public static class WindowFunction
    {
        public static class WindowCache
        {
            private static readonly ConcurrentDictionary<(int, string), double[]> _cache
                = new ConcurrentDictionary<(int, string), double[]>();

            public static double[] GetWindow(int size, string type)
            {
                return _cache.GetOrAdd((size, type), key => GenerateWindow(key.Item1, key.Item2));
            }

            private static double[] GenerateWindow(int size, string type)
            {
                var window = new double[size];

                switch (type)
                {
                    case "Hanning":
                        for (int i = 0; i < size; i++)
                            window[i] = 0.5 * (1 - Math.Cos(2 * Math.PI * i / (size - 1)));
                        break;

                    case "Hamming":
                        for (int i = 0; i < size; i++)
                            window[i] = 0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (size - 1));
                        break;

                    case "Rectangular":
                        Array.Fill(window, 1.0);
                        break;

                    default:
                        throw new ArgumentException($"Unknown window type: {type}");
                }

                return window;
            }
        }

        public static Complex[] Do(
            float[] samples,
            int startIndex,
            int windowSize,
            string windowType)
        {
            windowType ??= Params._windowType;

            if (samples == null) throw new ArgumentNullException(nameof(samples));
            if ((uint)startIndex >= (uint)samples.Length) throw new ArgumentOutOfRangeException(nameof(startIndex));

            int available = Math.Max(0, samples.Length - startIndex);
            int actualSamples = Math.Min(available, windowSize);

            double[] window = WindowCache.GetWindow(windowSize, windowType);

            Complex[] result = new Complex[windowSize];

            for (int i = 0; i < actualSamples; i++)
            {
                result[i] = new Complex(samples[startIndex + i] * window[i], 0);
            }

            return result;
        }

        public static Complex[] UnDo(
            float[] samples,
            int startIndex,
            int windowSize,
            string windowType)
        {
            windowType ??= Params._windowType;

            if (samples == null) throw new ArgumentNullException(nameof(samples));
            if ((uint)startIndex >= (uint)samples.Length) throw new ArgumentOutOfRangeException(nameof(startIndex));

            int available = Math.Max(0, samples.Length - startIndex);
            int actualSamples = Math.Min(available, windowSize);

            double[] window = WindowCache.GetWindow(windowSize, windowType);

            Complex[] result = new Complex[windowSize];

            for (int i = 0; i < actualSamples; i++)
            {
                result[i] = new Complex(Math.Clamp(samples[startIndex + i] / window[i], -1, 1), 0);
            }

            return result;
        }

        public static double[] UnDo(
            Complex[] samples,
            int startIndex,
            int windowSize,
            string windowType)
        {
            windowType ??= Params._windowType;

            if (samples == null) throw new ArgumentNullException(nameof(samples));
            if ((uint)startIndex >= (uint)samples.Length) throw new ArgumentOutOfRangeException(nameof(startIndex));

            int available = Math.Max(0, samples.Length - startIndex);
            int actualSamples = Math.Min(available, windowSize);

            double[] window = WindowCache.GetWindow(windowSize, windowType);

            double[] result = new double[windowSize];

            for (int i = 0; i < actualSamples; i++)
            {
                result[i] = Math.Clamp(samples[startIndex + i].Real / window[i], -1, 1);
            }

            return result;
        }
    }
}
