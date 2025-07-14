using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIKA_IMBANIKA_AUDIO
{
    public static class STRETCHER
    {
        public static int _startIndex;
        public static int _width;
        public static int _step;
        public static Complex[][] _fft;
        public static double _globalMax;

        public static void RenderTrack(string path)
        {
            string name = GetName(path);

            Logger.Log($"Loading wav {name}...");

            byte[] wavBytes = File.ReadAllBytes(path);
            var wav = AudioParser.Parse(wavBytes);

            Logger.Log($"Wav {name} loaded.");

            Process(wav, name);
        }

        public static void Process(Wav wav, string name)
        {
            Logger.Log($"Starting processing of {name}.");

            if (wav.Channels == 2)
            {
                Logger.Log("Processing started for L.");
                ProcessOneChannel(false, $"L {name}", wav);
                Logger.Log("Processing started for R.");
                ProcessOneChannel(true, $"R {name}", wav);
            }
            else
            {
                Logger.Log("Processing started for mono.");
                ProcessOneChannel(false, $"Mono {name}", wav);
            }
        }

        public static void ProcessOneChannel(bool isRight, string adder, Wav wav)
        {
            if (isRight)
                wav.Samples[0] = Specralize(adder, wav.Samples[0], wav);
            else
                wav.Samples[0] = Specralize(adder, wav.Samples[0], wav);
        }

        static float[] Specralize(string adder, float[] samples, Wav wav)
        {
            InitSpectraliser();

            Logger.Log($"Spectralising ({adder})...");

            _globalMax = 0;

            ///////////////////////

            for (int x = 0; x < _width; x++)
            {
                var input = WindowFunction.Do(samples, _startIndex, Params._windowSize, Params._windowType);
                double max = 0;
                _fft[x] = FFT.Do(input, out max);

                _startIndex += _step;

                if (max > _globalMax)
                    _globalMax = max;
            }

            NormaliseFft();

            Draw(adder);

            return samples;
        }

        public static void Draw(string adder)
        {
            Logger.Log($"Kinda drawing {adder}.");
        }

        public static void NormaliseFft()
        {
            Logger.Log($"Normalising fft...");
            for (int x = 0; x < _width; x++)
                for (int y = 0; y < Params._windowSize; y++)
                    _fft[x][y] /= _globalMax;

            Logger.Log("Fft normalised.");
        }

        public static void InitFirst(Wav wav)
        {
            _startIndex = 0;
            _step = (int)(Params._windowSize * (1 - Params._overlap));
            _width = (int)Math.Ceiling((double)(wav.Samples[0].Length - Params._windowSize) / _step) + 1;
            _fft = new Complex[_width][];
        }

        public static void InitSpectraliser()
        {
            _fft = new Complex[_width][];

            _startIndex = 0;
        }

        static string GetName(string path) => System.IO.Path.GetFileNameWithoutExtension(path);
    }
}
