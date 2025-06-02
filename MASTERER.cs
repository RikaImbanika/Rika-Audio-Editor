using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Rika_Audio
{
    public static class MASTERER
    {
        public static int _startIndex;
        public static int _width;
        public static int _step;
        public static Complex[][] _fft;
        public static WriteableBitmap _bitmap1;
        public static WriteableBitmap _bitmap2;
        public static WriteableBitmap _bitmap3;
        public static int _stride;
        public static int _bufferSize;
        public static int _bytesPerPixel = 3; ////////////////////
        public static double _globalMax;
        public static float[] _samples;
        public static void MakeModel(string[] wavs, float[] weights)
        {
            Thread mtr = new Thread(MT);
            mtr.Name = "MASTERER";
            mtr.Start();

            void MT()
            {
                Logger.Log($"Starting making model from {wavs.Length} audios.");

                weights = NormaliseWeights(weights);

                float[,] result = new float[Params._windowSize / 2, Params._resolution];

                for (int i = 0; i < wavs.Length; i++)
                {
                    byte[] wavBytes = File.ReadAllBytes(wavs[i]);
                    var wav = WavParser.Parse(wavBytes);
                    string name = GetName(wavs[i]);
                    float[,] model = ProcessOne(wav, name);
                    model = Multiply(model, weights[i]);
                    result = SummModels(result, model);
                }

                DrawModel(result);

                Logger.Log($"Model maked.");
            }
        }

        public static float[,] Multiply(float[,] array, float weight)
        { 
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    array[i, j] *= weight;
                }
            }

            return array;
        }

        public static float[] NormaliseWeights(float[] weights)
        {
            float sum = 0;

            for (int i = 0; i < weights.Length; i++)
                sum += weights[i];

            for (int i = 0; i < weights.Length; i++)
                weights[i] /= sum;

            return weights;
        }

        public static float[,] ProcessOne(Wav wav, string name)
        {
            InitFirst(wav);

            Logger.Log($"Starting processing of {name}.");

            float[,] model;

            if (wav.Channels == 2)
            {
                Logger.Log("Processing started for L.");
                var modelL = ProcessOneChannel(false, wav, name);
                Logger.Log("Processing started for R.");
                var modelR = ProcessOneChannel(true, wav, name);

                model = SummModels(modelL, modelR);
            }
            else
            {
                Logger.Log("Processing started for mono.");
                model = ProcessOneChannel(false, wav, name);
            }

            return model;

            //Wav.Save($"{Params._pf}Sined test for {name}.wav", wav);
        }

        public static float[,] ProcessOneChannel(bool isRight, Wav wav, string adder)
        {
            if (isRight)
                wav.SamplesR = Specralize($"The R {adder}", wav.SamplesR, wav);
            else
                wav.SamplesL = Specralize($"The L {adder}", wav.SamplesL, wav);

            float[,] model = FindModel();

            return model;
        }


        public static void LogarythmiseFft()
        {
            for (int spectrum = 0; spectrum < _width; spectrum++)
            {
                for (int frequencyId = 0; frequencyId < Params._windowSize; frequencyId++)
                {
                    _fft[spectrum][frequencyId].Magnitude = ToLogarithmic(_fft[spectrum][frequencyId].Magnitude);
                }
            }
        }

        public static double ToLogarithmic(double x, double baseLog = 10.0)
        {
            if (x < 0 || x > 1)
                throw new ArgumentOutOfRangeException(nameof(x), "Значение должно быть в диапазоне [0, 1].");

            if (x == 0)
                return 0;

            // Нормализуем логарифмическое значение в [0, 1]
            double logValue = Math.Log(x, baseLog);
            double minLog = Math.Log(1.0 / baseLog, baseLog); // Минимальный логарифм для x = 0.1 (пример)

            // Масштабируем в [0, 1]
            double normalized = (logValue + Math.Abs(minLog)) / Math.Abs(minLog);

            // Ограничиваем, чтобы избежать выхода за границы из-за погрешностей
            return Math.Max(0, Math.Min(1, normalized));
        }

        public static double FromLogarithmic(double y, double baseLog = 10.0)
        {
            if (y < 0 || y > 1)
                throw new ArgumentOutOfRangeException(nameof(y), "Значение должно быть в диапазоне [0, 1].");

            if (y == 0)
                return 0;

            double minLog = Math.Log(1.0 / baseLog, baseLog);
            double logValue = y * Math.Abs(minLog) - Math.Abs(minLog);

            double x = Math.Pow(baseLog, logValue);

            // Ограничиваем, чтобы избежать выхода за границы из-за погрешностей
            return Math.Max(0, Math.Min(1, x));
        }

        public static float ToLogarithmic(float x, float baseLog = 10f)
        {
            if (x < 0f || x > 1f)
                throw new ArgumentOutOfRangeException(nameof(x), "Значение должно быть в диапазоне [0, 1].");

            if (x == 0f)
                return 0f;

            // Нормализуем логарифмическое значение в [0, 1]
            float logValue = MathF.Log(x, baseLog);
            float minLog = MathF.Log(1f / baseLog, baseLog); // Минимальный логарифм для x = 0.1

            // Масштабируем в [0, 1]
            float normalized = (logValue + MathF.Abs(minLog)) / MathF.Abs(minLog);

            // Ограничиваем, чтобы избежать выхода за границы из-за погрешностей
            return MathF.Max(0f, MathF.Min(1f, normalized));
        }

        public static float FromLogarithmic(float y, float baseLog = 10f)
        {
            if (y < 0f || y > 1f)
                throw new ArgumentOutOfRangeException(nameof(y), "Значение должно быть в диапазоне [0, 1].");

            if (y == 0f)
                return 0f;

            float minLog = MathF.Log(1f / baseLog, baseLog);
            float logValue = y * MathF.Abs(minLog) - MathF.Abs(minLog);

            float x = MathF.Pow(baseLog, logValue);

            // Ограничиваем, чтобы избежать выхода за границы из-за погрешностей
            return MathF.Max(0f, MathF.Min(1f, x));
        }

        static float[,] FindModel()
        {
            float[,] model = new float[Params._windowSize / 2, Params._resolution];

            for (int spectrum = 0; spectrum < _width; spectrum++)
            {
                for (int frequencyId = 0; frequencyId < Params._windowSize / 2; frequencyId++)
                {
                    model[frequencyId, (int)Math.Floor(_fft[spectrum][frequencyId].Magnitude * Params._resolution - 1e-10)] += 1f / _width;
                }
            }

            Logger.Log("That model created.");

            return model;
        }

        static float[,] SummModels(float[,] model1, float[,] model2)
        {
            int w = model1.GetLength(0);
            int h = model1.GetLength(1);
            float[,] model = new float[w, h];

            for (int frequencyId = 0; frequencyId < Params._windowSize / 2; frequencyId++)
            {
                for (int resolutionId = 0; resolutionId < Params._resolution; resolutionId++)
                {
                    model[frequencyId, resolutionId] = (model1[frequencyId, resolutionId] + model2[frequencyId, resolutionId]) / 2;
                }
            }

            Logger.Log("Models added.");

            return model;
        }

        static float[] Specralize(string adder, float[] samples, Wav wav)
        {
            InitSpectraliser();

            Logger.Log($"Spectralising ({adder})...");

            _globalMax = 0; //Is it a mistake?

            ///////////////////////

            for (int x = 0; x < _width; x++)
            {
                var input = WindowFunction.Do(samples, _startIndex, Params._windowSize, Params._windowType);
                double max = 0;
                _fft[x] = FFT.Do(input, out max);

                _startIndex += _step;

                if (max > _globalMax)
                    _globalMax = max; //float?
            }

           /* _globalMax = 0;

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < Params._windowSize / 2; y++)
                {
                    float power = (MathF.Sin(y * MathF.PI * 2 * 7 / (Params._windowSize / 2) + x / 3f) + 1) / 2f;
                    _fft[x][y] *= power;

                    int antiY = Params._windowSize - 1 - y;
                    _fft[x][antiY] *= power;

                    var max = _fft[x][y].Magnitude;
                    if (max > _globalMax)
                        _globalMax = max; //float?
                }
            }

            samples = new float[samples.Length];

            for (int x = 0; x * _step < samples.Length - Params._windowSize; x++)
            {
                var signal = FFT.IFFT(_fft[x]); //HERE!!!!!!!!!!!

                for (int y = 0; y < Params._windowSize; y++) //!!!!!!!!!!!!!!!!!!!!!!
                    samples[x * _step + y] += Convert.ToSingle(signal[y].Real);
            }

            samples = Normalise(samples);

            Logger.Log($"Equalisation of ({adder}) done.");*/

            NormaliseFft();

            Draw(adder);

            Logger.Log($"Spectralisation of ({adder}) done.");

            return samples;
        }

        public static float[] Normalise(float[] samples)
        {
            if (samples == null || samples.Length == 0)
            {
                Logger.Log("Empty array!");
                return samples;
            }

            float maxAbsolute = 0;

            foreach (float sample in samples)
            {
                float abs = Math.Abs(sample);
                if (abs > maxAbsolute) maxAbsolute = abs;
            }

            if (maxAbsolute == 0)
                return samples;

            for (int i = 0; i < samples.Length; i++)
                samples[i] /= maxAbsolute;

            Logger.Log("Samples normalised!");
            return samples;
        }

        public static void NormaliseFft()
        {
            Logger.Log($"Normalising fft...");
            for (int x = 0; x < _width; x++)
                for (int y = 0; y < Params._windowSize; y++)
                    _fft[x][y] /= _globalMax;

            Logger.Log("Fft normalised.");
        }

        public static void Draw(string adder)
        {
            Logger.Log($"Drawing ({adder})...");

            InitBitmaps();

            byte[] pixels2 = new byte[_bufferSize];

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < Params._windowSize / 2; y++)
                {
                    double magnitude = _fft[x][y].Magnitude;
                    double phase = _fft[x][y].Phase;

                    byte blue = (byte)(magnitude * 255);

                    if (blue > 200)
                    {

                    }

                    double sinPhase = Math.Sin(phase); // Диапазон: [-1, 1]
                    byte red = (byte)(255 * magnitude);
                    byte green = (byte)(255 * magnitude);
                    if (sinPhase > 0)
                        red = (byte)((1 - sinPhase) * magnitude * 255);
                    else
                        green = (byte)((1 + sinPhase) * magnitude * 255);

                    int offset = y * _stride + x * _bytesPerPixel;

                    pixels2[offset] = blue; // B
                    pixels2[offset + 1] = green; // G
                    pixels2[offset + 2] = red; // R
                }
            }

            Int32Rect rect = new Int32Rect(0, 0, _width, Params._windowSize / 2);
            _stride = (_width * _bytesPerPixel + 3) & ~3;

            _bitmap2.WritePixels(rect, pixels2, _stride, 0);

            SaveBitmap(_bitmap2, $"The FFT Amplitude {adder}");
        }

        public static void DrawModel(float[,] model)
        {
            Logger.Log("Drawing model...");

            model = ConvertToLogScale(model);

            Logger.Log("Model logarythmised.");

            // Получаем размеры модели
            int width = model.GetLength(0);
            int height = model.GetLength(1);

            // Рассчитываем stride для данного изображения (а не используем глобальный _stride)
            int stride = (width * _bytesPerPixel + 3) & ~3;

            // Выделяем буфер для пикселей с правильным размером
            byte[] pixels = new byte[height * stride];

            WriteableBitmap bitmap = new WriteableBitmap(
                width,
                height,
                96,
                96,
                PixelFormats.Bgr24,
                null
            );

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Преобразуем значение в байт (0-255)
                    byte value = (byte)(model[x, y] * 255);

                    // Вычисляем позицию в буфере
                    int offset = y * stride + x * _bytesPerPixel;

                    // Записываем цвет (grayscale)
                    pixels[offset] = value;     // B
                    pixels[offset + 1] = value; // G
                    pixels[offset + 2] = value; // R
                }
            }

            // Копируем пиксели в bitmap
            bitmap.WritePixels(
                new Int32Rect(0, 0, width, height),
                pixels,
                stride,
                0
            );

            SaveBitmap(bitmap, $"Model");

            Logger.Log("Model drawn.");
        }

        public static float[,] ConvertToLogScale(float[,] array)
        {
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            float[,] logArray = new float[rows, cols];

            // Найдём минимальное и максимальное значения после логарифмирования
            float minLog = float.MaxValue;
            float maxLog = float.MinValue;

            // Применяем логарифм и находим min/max
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    float value = array[i, j];
                    // Заменяем 0 на очень маленькое число
                    if (value == 0.0f)
                        value = 1e-10f;
                    // Применяем натуральный логарифм (можно Math.Log10 для log10)
                    float logValue = (float)Math.Log(value);
                    logArray[i, j] = logValue;

                    if (logValue < minLog) minLog = logValue;
                    if (logValue > maxLog) maxLog = logValue;
                }
            }

            // Нормализуем в [0, 1]
            float range = maxLog - minLog;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (range == 0)
                        logArray[i, j] = 0; // Если все значения одинаковы
                    else
                        logArray[i, j] = (logArray[i, j] - minLog) / range;
                }
            }

            return logArray;
        }

        public static void InitBitmaps()
        {
            _stride = (_width * _bytesPerPixel + 3) & ~3;
            _bufferSize = (Params._windowSize / 2) * _stride;

            _bitmap1 = new WriteableBitmap(
                        _width,
                        Params._windowSize / 2,
                        96,
                        96,
                        PixelFormats.Bgr24,
                        null
                    );

            _bitmap2 = new WriteableBitmap(
                _width,
                Params._windowSize / 2,
                96,
                96,
                PixelFormats.Bgr24,
                null
            );

            _bitmap3 = new WriteableBitmap(
                _width,
                Params._windowSize / 2,
                96,
                96,
                PixelFormats.Bgr24,
                null
            );
        }

        public static void InitSpectraliser()
        {
            _fft = new Complex[_width][];

            _startIndex = 0;
        }

        public static void SaveBitmap(WriteableBitmap bitmap, string name)
        {
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var stream = new FileStream($"{Params._pf}{name}.png", FileMode.Create))
            {
                encoder.Save(stream);
            }
        }

        public static void InitFirst(Wav wav)
        {
            _startIndex = 0;
            _step = (int)(Params._windowSize * (1 - Params._overlap));
            _width = (int)Math.Ceiling((double)(wav.SamplesL.Length - Params._windowSize) / _step) + 1;
            _fft = new Complex[_width][];
        }

        static string GetName(string path) => System.IO.Path.GetFileNameWithoutExtension(path);
    }
}