using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RIKA_IMBANIKA_AUDIO
{
    public static class MASTERER
    {
        public static int _startIndex;
        public static int _width;
        public static int _step;
        public static Complex[][][] _fft;
        public static WriteableBitmap _bitmap1;
        public static WriteableBitmap _bitmap2;
        public static WriteableBitmap _bitmap3;
        public static int _stride;
        public static int _bufferSize;
        public static int _bytesPerPixel = 3; ////////////////////
        public static double _globalMax;
        public static float[] _samples;
        public static string[] _modelsPaths;
        public static Wav _ourWav;

        public static void MASTER(string wav, string outputName)
        {
            Thread master = new Thread(MASTERTHREAD);
            master.Name = "MASTERER";
            master.Start();
        
            void MASTERTHREAD()
            {
                float[,] trackModel = MakeOneModel(wav);
                float[,] idealModel = LoadModel(Params._selectedModelPath);
                float[][][] tuner = MakeTuner(trackModel, idealModel);

                Logger.Log($"MASTERING...");

                for (int sample = 0; sample < _fft[0].Length; sample++)
                {
                    for (int frq = 0; frq < Params._windowSize / 2; frq++)
                    {
                        TuneOne(0, sample, frq, tuner);
                        if (_ourWav.Channels == 2)
                            TuneOne(1, sample, frq, tuner);
                    }
                }

                DelogarythmiseFft(0);
                if (_ourWav.Channels == 2)
                    DelogarythmiseFft(1);

                Despectralise(0);
                if (_ourWav.Channels == 2)
                    Despectralise(1);

                Normalise(_ourWav.Samples[0]);
                if (_ourWav.Channels == 2)
                    Normalise(_ourWav.Samples[1]);

                Wav.Save($"{Params.PF}Output\\{outputName}.wav", _ourWav);
                Logger.Log($"MASTERING DONE!");
            }
        }

        public static void TuneOne(int channelId, int sample, int frq, float[][][] tuner)
        {
            int antifrq = Params._windowSize - frq - 1;

            double volume1 = _fft[channelId][sample][frq].Magnitude;
            double volume2 = _fft[channelId][sample][antifrq].Magnitude;
            double volume = (volume1 + volume2) / 2;

            if (double.IsNaN(volume))
            {

            }

            double tune = FindTune(volume, frq, tuner);
            tune = (volume + tune) / volume;

            if (double.IsNaN(tune))
            {

            }

            _fft[channelId][sample][frq] *= tune;
            _fft[channelId][sample][antifrq] *= tune;
        }

        public static double FindTune(double volume, int frqId, float[][][] tuner)
        {
            int i = 0;
            for (; i < Params._resolution - 1; i++)
                if (volume >= tuner[frqId][0][i] && volume < tuner[frqId][0][i + 1])
                    break;

            double progress;
            double tune;

            if (i < Params._resolution - 1)
            {
                progress = (volume - tuner[frqId][0][i]) / (tuner[frqId][0][i + 1] - tuner[frqId][0][i]);
                tune = tuner[frqId][1][i] * (1 - progress) + tuner[frqId][1][i + 1] * progress;
            }
            else
                tune = tuner[frqId][1][Params._resolution - 1];

            if (double.IsNaN(tune))
            {

            }

            return tune;
        }

        public static float[][][] MakeTuner(float[,] from, float[,] to)
        {
            int width = from.GetLength(0);
            float[][][] tuner = new float[width][][];

            for (int frqId = 0; frqId < width; frqId++)
                tuner[frqId] = TuneOneFrequency(from, to, frqId);

            return tuner;
        }

        public static float[][] TuneOneFrequency(float[,] from, float[,] to, int frqId)
        {
            float[] from0 = new float[Params._resolution];
            float[] to0 = new float[Params._resolution];

            for (int i = 0; i < Params._resolution; i++)
            {
                from0[i] = from[frqId, i];
                to0[i] = to[frqId, i];
            }

            float[][] tuner = new float[2][];

            tuner[0] = Distribute(from0, Params._resolution);
            tuner[1] = Distribute(to0, Params._resolution);

            for (int i = 0; i < tuner[1].Length; i++)
                tuner[1][i] = tuner[1][i] - tuner[0][i];

            return tuner;
        }

        public static float[] Distribute(float[] how, int count)
        {
            float[] result = new float[count];

            int[] counts = new int[how.Length];

            float sum = 0;
            for (int i = 0; i < how.Length; i++)
                sum += how[i];

            int sum2 = 0;

            double[] fractions = new double[how.Length]; // Массив дробных частей

            // Вычисление целых частей и дробных остатков
            for (int i = 0; i < how.Length; i++)
            {
                float value = how[i] * count / sum;
                counts[i] = (int)System.Math.Floor(value);
                fractions[i] = value - counts[i];
                sum2 += counts[i];
            }

            // Корректировка остатка
            int diff = count - sum2;
            if (diff != 0)
            {
                // Создание и сортировка индексов по убыванию дробной части
                int[] indices = new int[how.Length];
                for (int i = 0; i < how.Length; i++)
                    indices[i] = i;

                Array.Sort(indices, (a, b) => fractions[b].CompareTo(fractions[a]));

                // Распределение остатка
                if (diff > 0)
                    for (int i = 0; i < diff; i++)
                        counts[indices[i]]++;
                else
                    for (int i = 0; i < -diff; i++)
                        counts[indices[i]]--;
            }

            int jj = 0;

            float substep = 1f / how.Length;

            for (int i = 0; i < how.Length; i++)
            {
                float subsubstep = 0;
                if (counts[i] > 0)
                    subsubstep = substep / counts[i];

                for (int k = 0; k < counts[i]; k++)
                {
                    result[jj] = i / (how.Length * 1f) + k * subsubstep;
                    jj += 1;
                }
            }

            return result;
        }

        public static float[] GenerateDistributedArray(float[] dist, int n)
        {
            //Delme
            if (n == 0)
                return Array.Empty<float>();

            int m = dist.Length;
            if (m < 2)
            {
                // Если распределение задано некорректно, возвращаем равномерное распределение
                float[] uniformArray = new float[n];
                for (int i = 0; i < n; i++)
                    uniformArray[i] = (n == 1) ? 0.5f : (float)i / (n - 1);
                return uniformArray;
            }

            // Генерируем значения u (квантили)
            float[] uVals = new float[n];
            if (n == 1)
                uVals[0] = 0.5f;
            else
                for (int i = 0; i < n; i++)
                    uVals[i] = (float)i / (n - 1);

            float[] result = new float[n];
            for (int i = 0; i < n; i++)
            {
                float u = uVals[i];
                if (u <= dist[0])
                {
                    result[i] = 0.0f;
                }
                else if (u >= dist[m - 1])
                {
                    result[i] = 1.0f;
                }
                else
                {
                    // Находим интервал j, такой что dist[j] <= u < dist[j + 1]
                    int j = 0;
                    while (j < m - 1 && !(dist[j] <= u && u < dist[j + 1]))
                        j++;

                    // Вычисляем x_j и x_{j+1}
                    float x_j = (float)j / (m - 1);
                    float x_j1 = (float)(j + 1) / (m - 1);

                    // Линейная интерполяция
                    float segmentLength = dist[j + 1] - dist[j];
                    if (segmentLength == 0)
                    {
                        result[i] = x_j; // Если интервал вырожден, берем начало
                    }
                    else
                    {
                        float ratio = (u - dist[j]) / segmentLength;
                        result[i] = x_j + ratio * (x_j1 - x_j);
                    }
                }
            }

            return result;
        }

        public static float[] TransformDistribution(float[] from, float[] to)
        {
            //Delme
            int n = from.Length;
            float[] result = new float[n];

            // Рассчитываем кумулятивные распределения (CDF) для from и to
            float[] cdfFrom = new float[n];
            float[] cdfTo = new float[n];

            cdfFrom[0] = from[0];
            cdfTo[0] = to[0];

            for (int i = 1; i < n; i++)
            {
                cdfFrom[i] = cdfFrom[i - 1] + from[i];
                cdfTo[i] = cdfTo[i - 1] + to[i];
            }

            // Нормализуем CDF до 1.0 (устраняем погрешности float)
            float sumFrom = cdfFrom[n - 1];
            float sumTo = cdfTo[n - 1];

            for (int i = 0; i < n; i++)
            {
                cdfFrom[i] /= sumFrom;
                cdfTo[i] /= sumTo;
            }

            // Обрабатываем каждый бин исходного распределения
            for (int i = 0; i < n; i++)
            {
                // Вычисляем квантиль (середина текущего бина)
                float lowerBound = (i == 0) ? 0 : cdfFrom[i - 1];
                float upperBound = cdfFrom[i];
                float quantile = (lowerBound + upperBound) / 2f;

                // Находим целевой бин в распределении to
                int targetBin = 0;
                while (targetBin < n && quantile > cdfTo[targetBin])
                {
                    targetBin++;
                }

                // Корректируем для последнего бина
                if (targetBin >= n) targetBin = n - 1;

                // Вычисляем границы целевого бина
                float targetLower = (targetBin == 0) ? 0 : cdfTo[targetBin - 1];
                float targetUpper = cdfTo[targetBin];

                // Вычисляем позицию внутри целевого бина
                float binProbability = targetUpper - targetLower;
                float fraction = (binProbability < 0.0001f)
                    ? 0.5f
                    : (quantile - targetLower) / binProbability;

                // Линейно интерполируем значение в целевом бине
                // Предполагаем, что бины равномерно распределены в [0, 1]
                float binValue = (targetBin + fraction) / n;
                result[i] = binValue;
            }

            return result;
        }

        public static void MakeModel(string[] wavs, float[] weights, string name)
        {
            Thread mtr = new Thread(MT);
            mtr.Name = "MASTERER";
            mtr.Start();

            void MT()
            {
                WindowsManager.Close(typeof(HowToMasterWindow));

                Logger.Log($"Starting making model from {wavs.Length} audios.");

                weights = NormaliseWeights(weights);

                float[,] result = new float[Params._windowSize / 2, Params._resolution];

                for (int i = 0; i < wavs.Length; i++)
                {
                    var model = MakeOneModel(wavs[i]);
                    model = Multiply(model, weights[i]);
                    result = SummModels(result, model);
                }

                SaveModel(result, name);
                DrawModel(result, name);
                LoadModelsPaths();
                WindowsManager.Close(typeof(HowToMasterWindow));
                WindowsManager.Open(typeof(HowToMasterWindow));
                Thread.Sleep(250);
                SelectModel(name);

                Logger.Log($"Model maked. Done.");
            }
        }

        public static float[,] MakeOneModel(string wavPath)
        {
            byte[] wavBytes = File.ReadAllBytes(wavPath);
            _ourWav = AudioParser.Parse(wavBytes);
            string name = GetName(wavPath);
            return ProcessOne(_ourWav, name);
        }

        public static void SelectModel(string name)
        {
            if (WindowsManager._howToMasterWindow != null)
            {
                WindowsManager._howToMasterWindow.Dispatcher.Invoke(() =>
                {
                    Params._selectedModelId = WindowsManager._howToMasterWindow.ModelsComboBox.Items.IndexOf(name);
                    Params._selectedModelPath = _modelsPaths[Params._selectedModelId];

                    if (Params._selectedModelId >= 0)
                        WindowsManager._howToMasterWindow.ModelsComboBox.SelectedIndex = Params._selectedModelId;
                    else
                        WindowsManager._howToMasterWindow.ModelsComboBox.SelectedIndex = -1;
                });

                File.WriteAllText($"{Params.PF}Params\\SelectedModelName.txt", $"{name}");
            }
        }

        public static void LoadModelsPaths()
        {
            _modelsPaths = Directory.GetFiles($"{Params.PF}Models");
        }

        public static void LoadModelsToComboBox()
        {
            if (WindowsManager._howToMasterWindow != null)
            {
                string[] fileNames = _modelsPaths
                    .Select(Path.GetFileNameWithoutExtension)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToArray();

                WindowsManager._howToMasterWindow.ModelsComboBox.Items.Clear();
                WindowsManager._howToMasterWindow.ModelsComboBox.ItemsSource = fileNames;
                WindowsManager._howToMasterWindow.ModelsComboBox.SelectedIndex = Params._selectedModelId;
            }
        }

        public static void SaveModel(float[,] model, string name)
        {
            string path = $"{Params.PF}Models\\{name}.bin";
            using (var stream = File.OpenWrite(path))
            using (var writer = new BinaryWriter(stream))
            {
                // Записываем размерности массива
                writer.Write(model.GetLength(0));
                writer.Write(model.GetLength(1));

                // Записываем все элементы массива
                for (int i = 0; i < model.GetLength(0); i++)
                {
                    for (int j = 0; j < model.GetLength(1); j++)
                    {
                        writer.Write(model[i, j]);
                    }
                }
            }

            Logger.Log($"Model {name} saved.");
        }

        public static float[,] LoadModel(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var reader = new BinaryReader(stream))
            {
                // Читаем размерности массива
                int dim0 = reader.ReadInt32();
                int dim1 = reader.ReadInt32();

                // Создаем новый массив
                float[,] model = new float[dim0, dim1];

                // Читаем все элементы массива
                for (int i = 0; i < dim0; i++)
                {
                    for (int j = 0; j < dim1; j++)
                    {
                        model[i, j] = reader.ReadSingle();
                    }
                }

                return model;
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
            {
                Specralize($"The R {adder}", wav, 1);
                return FindModel(1);
            }
            else
            {
                Specralize($"The L {adder}", wav, 0);
                return FindModel(0);
            }
        }

        public static void LogarythmiseFft(int channelId)
        {
            Logger.Log($"Logarythmising fft...");

            for (int spectrum = 0; spectrum < _width; spectrum++)
                for (int frequencyId = 0; frequencyId < Params._windowSize; frequencyId++)
                {
                    double before = _fft[channelId][spectrum][frequencyId].CalcMagnitude;
                    double after = ToLogarithmic(before);
                    double d = after / before;
                    _fft[channelId][spectrum][frequencyId] *= d;
                    _fft[channelId][spectrum][frequencyId].Magnitude = after;
                    
                    if (double.IsNaN(_fft[channelId][spectrum][frequencyId].CalcMagnitude))
                    {

                    }
                }

            Logger.Log($"Fft logarythmised (FULL).");
        }

        public static void DelogarythmiseFft(int channelId)
        {
            Logger.Log($"Delogarythmising fft...");

            for (int spectrum = 0; spectrum < _width; spectrum++)
                for (int frequencyId = 0; frequencyId < Params._windowSize; frequencyId++)
                {
                    double before = _fft[channelId][spectrum][frequencyId].CalcMagnitude;
                    double after = FromLogarithmic(before);
                    double d = after / before;
                    _fft[channelId][spectrum][frequencyId] *= d;
                    _fft[channelId][spectrum][frequencyId].Magnitude = after;

                    if (double.IsNaN(_fft[channelId][spectrum][frequencyId].CalcMagnitude))
                    {

                    }
                }

            Logger.Log($"Fft delogarythmised!");
        }

        public static double ToLogarithmic(double x)
        {
            if (x < 0.0 || x > 1.0)
                throw new ArgumentOutOfRangeException(nameof(x), "Value must be in range [0, 1].");

            const double minDB = -80.0; // Минимальный динамический диапазон
            const double maxDB = 0.0;   // Максимальное значение (0 дБ)

            // Обработка нулевых и малых значений
            if (x <= System.Math.Pow(10, minDB / 20.0))
                return System.Math.Pow(10, minDB / 20.0);

            // Прямое преобразование в децибелы
            double dB = 20.0 * System.Math.Log10(x);

            // Нормализация в диапазон [0, 1]
            double normalized = (dB - minDB) / (maxDB - minDB);

            if (double.IsNaN(normalized))
            {

            }

            return System.Math.Max(0.0, System.Math.Min(1.0, normalized));
        }

        public static double FromLogarithmic(double y)
        {
            if (y < 0.0)
                return 0.0;
            if (y > 1.0)
                return 1.0;

            const double minDB = -80.0; // Должно совпадать с ToLogarithmic
            const double maxDB = 0.0;   // Должно совпадать с ToLogarithmic

            // Обработка нулевого значения
            if (y == 0.0)
                return 0.0;

            // Денормализация из [0, 1] в децибелы
            double dB = y * (maxDB - minDB) + minDB;

            // Преобразование из децибел в линейную величину
            double res = System.Math.Pow(10.0, dB / 20.0);

            if (double.IsNaN(res))
            {

            }

            return res;
        }
        public static float ToLogarithmic(float x)
        {
            if (x < 0f || x > 1f)
                throw new ArgumentOutOfRangeException(nameof(x), "Значение должно быть в диапазоне [0, 1].");

            const float minDB = -80f; // Минимальный динамический диапазон
            const float maxDB = 0f;   // Максимальное значение (0 дБ)

            // Обработка нулевых и малых значений
            if (x <= MathF.Pow(10, minDB / 20f))
                return 0f;

            // Прямое преобразование в децибелы
            float dB = 20f * MathF.Log10(x);

            // Нормализация в диапазон [0, 1]
            float normalized = (dB - minDB) / (maxDB - minDB);
            return MathF.Max(0f, MathF.Min(1f, normalized));
        }

        public static float FromLogarithmic(float y)
        {
            if (y < 0f || y > 1f)
                throw new ArgumentOutOfRangeException(nameof(y), "Значение должно быть в диапазоне [0, 1].");

            const float minDB = -80f; // Должно совпадать с ToLogarithmic
            const float maxDB = 0f;   // Должно совпадать с ToLogarithmic

            // Обработка нулевого значения
            if (y == 0f)
                return 0f;

            // Денормализация из [0, 1] в децибелы
            float dB = y * (maxDB - minDB) + minDB;

            // Преобразование из децибел в линейную величину
            return MathF.Pow(10f, dB / 20f);
        }

        static float[,] FindModel(int channelId)
        {
            float[,] model = new float[Params._windowSize / 2, Params._resolution];

            for (int spectrum = 0; spectrum < _width; spectrum++)
            {
                for (int frequencyId = 0; frequencyId < Params._windowSize / 2; frequencyId++)
                {
                    int antifrq = Params._windowSize - 1 - frequencyId;

                    double mag1 = _fft[channelId][spectrum][frequencyId].Magnitude;
                    double mag2 = _fft[channelId][spectrum][antifrq].Magnitude;

                    double mag = (mag1 + mag2) / 2;

                    var v = (int)System.Math.Floor(mag * Params._resolution);
                    if (v == Params._resolution)
                        v -= 1;
                    model[frequencyId, v] += 1f / _width;
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

        static void Despectralise(int channelId)
        {
            Logger.Log($"Despectralising of channel {channelId}...");

            _ourWav.Samples[channelId] = new float[_ourWav.Samples[channelId].Length];

            for (int x = 0; x * _step < _ourWav.Samples[channelId].Length - Params._windowSize; x++)
            {
                var signal = FFT.IFFT(_fft[channelId][x]); //HERE!!!!!!!!!!!

                for (int y = 0; y < Params._windowSize; y++) //!!!!!!!!!!!!!!!!!!!!!!
                    _ourWav.Samples[channelId][x * _step + y] += Convert.ToSingle(signal[y].Real);
            }

            _ourWav.Samples[channelId] = Normalise(_ourWav.Samples[channelId]);

            Logger.Log($"Despectralisation of channel ({channelId}) done.");
        }

        static void Specralize(string adder, Wav wav, int channelId)
        {
            InitSpectraliser();

            Logger.Log($"Spectralising ({adder})...");

            _globalMax = 0; //Is it a mistake?

            ///////////////////////

            for (int x = 0; x < _width; x++)
            {
                Complex[] input = WindowFunction.Do(wav.Samples[channelId], _startIndex, Params._windowSize, Params._windowType);
                double max = 0;
                _fft[channelId][x] = FFT.Do(input, out max);

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

            //NormaliseFft(channelId); //?????????
            LogarythmiseFft(channelId); /////////

            Draw(adder, channelId); //Turn off?

            Logger.Log($"Spectralisation of ({adder}) done.");
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
                float abs = System.Math.Abs(sample);
                if (abs > maxAbsolute) maxAbsolute = abs;
            }

            if (maxAbsolute == 0)
                return samples;

            for (int i = 0; i < samples.Length; i++)
                samples[i] /= maxAbsolute;

            Logger.Log("Samples normalised!");
            return samples;
        }

        public static void NormaliseFft(int channelId)
        {
            //Should I delete this?

            Logger.Log($"Normalising fft...");
            for (int x = 0; x < _width; x++)
                for (int y = 0; y < Params._windowSize; y++)
                {
                    _fft[channelId][x][y] /= _globalMax;

                    _fft[channelId][x][y].Magnitude = _fft[channelId][x][y].CalcMagnitude;

                    if (double.IsNaN(_fft[channelId][x][y].Magnitude))
                    {

                    }
                }

            Logger.Log("Fft normalised.");
        }

        public static void Draw(string adder, int channelId)
        {
            Logger.Log($"Drawing ({adder})...");

            InitBitmaps();

            byte[] pixels2 = new byte[_bufferSize];

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < Params._windowSize / 2; y++)
                {
                    double magnitude = _fft[channelId][x][y].Magnitude;
                    double phase = _fft[channelId][x][y].Phase;

                    byte blue = (byte)(magnitude * 255);

                    if (blue > 200)
                    {

                    }

                    double sinPhase = System.Math.Sin(phase); // Диапазон: [-1, 1]
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

        public static void DrawModel(float[,] model, string name)
        {
            Logger.Log($"Drawing model {name}...");

            Logger.Log($"Logarythmising model {name}...");

            model = ConvertToLogScale(model);

            Logger.Log($"Model {name} logarythmised.");

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

            SaveBitmap(bitmap, $"Model {name}");

            Logger.Log($"Model {name} drawn.");
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
                    float logValue = (float)System.Math.Log(value);
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
            if (_fft == null)
                _fft = new Complex[2][][];
            if (_fft[0] == null)
                _fft[0] = new Complex[_width][];
            if (_fft[1] == null)
                _fft[1] = new Complex[_width][];
            if (_fft[0].Length != _width)
                _fft[0] = new Complex[_width][];
            if (_fft[1].Length != _width)
                _fft[1] = new Complex[_width][];

            _startIndex = 0;
        }

        public static void SaveBitmap(WriteableBitmap bitmap, string name)
        {
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var stream = new FileStream($"{Params.PF}{name}.png", FileMode.Create))
            {
                encoder.Save(stream);
            }
        }

        public static void InitFirst(Wav wav)
        {
            _startIndex = 0;
            _step = (int)(Params._windowSize * (1 - Params._overlap));
            _width = (int)System.Math.Ceiling((double)(wav.Samples[0].Length - Params._windowSize) / _step) + 1;

            if (_fft == null)
                _fft = new Complex[2][][];
            if (_fft[0] == null)
                _fft[0] = new Complex[_width][];
            if (_fft[1] == null)
                _fft[1] = new Complex[_width][];
        }

        static string GetName(string path) => System.IO.Path.GetFileNameWithoutExtension(path);
    }
}