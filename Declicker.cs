using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.IO;
using System.Reflection.Metadata;
using System.Globalization;
using System.Windows.Automation;
using System.Windows.Media.Media3D;

namespace RIKA_IMBANIKA_AUDIO
{
    public static class Declicker
    {
        public static float _clickThreshold = 2f;
        public static int _phasesSmooth = 15;
        public static float _horizontalThreshold = 1f;
        public static float _st;
        public static float _et;
        public static float _lowFreqCompensate = 1f;
        public static float _threshold = 1f;
        public static float _loudnessThreshold = 1f;
        public static float _maxThreshold = 1f;
        public static float _minThreshold = 1f;
        public static float _multiplier = 1f;

        public static double[] _phaseShifts;
        public static float[] _lfCompensations;
        public static int _bytesPerPixel;
        public static int _steps;

        public static Complex[][] _fft;
        public static float[][] _phases;
        public static float[][] _dphases;
        public static float[][] _smoothdphases;
        public static float[][] _rephases;
        public static float[] _clickValues;
        public static float[] _clickAverages;
        public static float[] _lowEma;
        public static float[] _ampEma;
        public static float[] _maxes;
        public static Wav _wav;
        public static int _width;
        public static int _step;
        public static int _startIndex;
        public static WriteableBitmap _bitmap1;
        public static WriteableBitmap _bitmap2;
        public static WriteableBitmap _bitmap3;

        public static float[] _samples;
        public static float _average = 0.025f;
        public static float[] _deltas = new float[_width];

        public static float _deltaRemembered;

        public static void Declick(Wav wav, string ct, string ps, string ht, string st, string et, string lfc, string mt, string lt, string maxt)
        {
            _wav = wav;

            InitFirst();
            InitSpectraliser();

            if (!Parse(ct, ps, ht, st, et, lfc, mt, lt, maxt))
                return;

            _phaseShifts = CalculatePhaseShifts(Params._windowSize, _step);

            CalculateLFCompensations();
            InitBitmapes();

            Thread mtr = new Thread(MT);
            mtr.Name = "Declicker";
            mtr.Start();

            void MT()
            {
                if (wav.Channels == 2)
                {
                    Logger.Log("Declicking started for L.");
                    ProcessOneChannel(false);
                    Logger.Log("Declicking started for R.");
                    ProcessOneChannel(true);
                }
                else
                {
                    Logger.Log("Declicking started for mono.");
                    ProcessOneChannel(false);
                }

                void ProcessOneChannel(bool LR)
                {
                    _samples = wav.Samples[0];

                    string gg = "L";
                    if (LR)
                    {
                        _samples = wav.Samples[1];
                        gg = "R";
                    }

                    Specralize($"Before {gg}", _samples);

                    bool clickWasHere = false;

                    float delta = 0;
                    float deltaRem = 0;
                    float prevDelta = 0;

                    Logger.Log("Declicking part 1 ended.");

                    Specralize($"After {gg}", _samples); //WTF

                    double[][] signals = new double[_width][];

                    for (int x = 0; x < _width; x++)
                    {
                        var signal = FFT.IFFT(_fft[x]);
                        
                        for (int y = 0; y < Params._windowSize / 2; y++)
                            _samples[x * _step + y] += Convert.ToSingle(signal[y].Real);
                    }

                    Wav.Save($"{Params.PF}Declick.wav", wav);

                    if (LR)
                        Logger.Log($"Declicking Right channel ended!");
                    else
                        Logger.Log($"Declicking Left channel ended!");
                }

                void SaveBitmap(WriteableBitmap bitmap, string name)
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));

                    using (var stream = new FileStream($"{Params.PF}{name}.png", FileMode.Create))
                    {
                        encoder.Save(stream);
                    }
                }

                void Specralize(string adder, float[] samples)
                {
                    InitSpectraliser();

                    double globalMax = 0;

                    ///////////////////////

                    for (int x = 0; x < _width; x++)
                    {
                        var input = WindowFunction.Do(samples, _startIndex, Params._windowSize, Params._windowType);
                        double max = 0;
                        _fft[x] = FFT.Do(input, out max); //
                        _phases[x] = new float[Params._windowSize / 2];
                        _dphases[x] = new float[Params._windowSize / 2];
                        _smoothdphases[x] = new float[Params._windowSize / 2];
                        _rephases[x] = new float[Params._windowSize / 2];

                        if (max > globalMax)
                            globalMax = max;

                        _maxes[x] += (float)max / _width;

                        if (x > 0)
                        {
                            _ampEma[x] = _ampEma[x - 1] * 0.95f + _maxes[x] * 0.05f;
                            _ampEma[0] += _ampEma[x] / _width;
                        }

                        _startIndex += _step;

                        //////////////////////////////////////

                        for (int y = 0; y < Params._windowSize / 2; y++)
                        {
                            Complex bin = _fft[x][y];
                            double phase = System.Math.Atan2(bin.Imaginary, bin.Real);

                            double compensatedPhase = phase - _phaseShifts[y] * x;

                            compensatedPhase %= 2 * System.Math.PI;
                            if (compensatedPhase < 0)
                                compensatedPhase += 2 * System.Math.PI;

                            _phases[x][y] = (float)(compensatedPhase / (2 * Math.PI));
                        }

                        SmoothPhases(x);

                        if (x > 0)
                        {
                            for (int y = 0; y < Params._windowSize / 2; y++)
                            {
                                _dphases[x][y] = (_phases[x - 1][y] - _phases[x][y] + 0.5f) % 1 - 0.5f;
                                _smoothdphases[0][y] += _dphases[x][y] / _width; //Important
                                _smoothdphases[x][y] = _smoothdphases[x - 1][y] * 0.95f + _dphases[x][y] * 0.05f;

                                _rephases[x][y] = _dphases[x][y] - System.Math.Abs(_smoothdphases[x][y]) * System.Math.Sign(_dphases[x][y]) * _horizontalThreshold;
                                //?

                                double magnitude = _fft[x][y].Magnitude;
                                magnitude /= globalMax;

                                float compensation = _lfCompensations[y];

                                if (_rephases[x][y] < 0 && _dphases[x][y] > 0 || _dphases[x][y] < 0 && _rephases[x][y] > 0)
                                    _rephases[x][y] = 0;

                                _rephases[x][y] = (_rephases[x][y] * (float)magnitude) * compensation * _multiplier / 2 + 0.5f;

                                float actRephase = Math.Clamp((_rephases[x][y] - 0.5f) * 500 * 1000, -500, 500);

                                if (actRephase > 500f * _threshold || actRephase < -500f * _threshold)
                                    _clickValues[x] += Math.Abs(_rephases[x][y]);
                            }

                            _clickAverages[x] = _clickAverages[x - 1] * 0.85f + MathF.Abs(_clickValues[x]) * 0.15f;
                            _lowEma[x] = _lowEma[x - 1] * 0.98f + _clickAverages[x] * 0.02f;
                        }
                    }

                    if (_clickAverages.Count() > 0)
                        _clickAverages[0] = _clickAverages.Average() * 2;
                    if (_lowEma.Count() > 0)
                        _lowEma[0] = _lowEma.Average() * 2;

                    _clickValues = new float[_width];

                    for (int x = 1; x < _width; x++)
                    {
                        for (int y = 0; y < Params._windowSize / 2; y++)
                        {
                            _smoothdphases[x][y] = _smoothdphases[x - 1][y] * 0.95f + Math.Abs(_dphases[x][y]) * 0.05f;

                            _rephases[x][y] = _dphases[x][y] - _smoothdphases[x][y] * Math.Sign(_dphases[x][y]) * _horizontalThreshold;

                            double magnitude = _fft[x][y].Magnitude;
                            magnitude /= globalMax;

                            float compensation = _lfCompensations[y];

                            if (_rephases[x][y] < 0 && _dphases[x][y] > 0 || _dphases[x][y] < 0 && _rephases[x][y] > 0)
                                _rephases[x][y] = 0;

                            _rephases[x][y] = (_rephases[x][y] * (float)magnitude) * compensation * _multiplier / 2 + 0.5f;

                            float actRephase = Math.Clamp((_rephases[x][y] - 0.5f)  * 500 * 1000, -500, 500);

                            if (actRephase > 500f * _threshold || actRephase < -500f * _threshold)
                                _clickValues[x] += 1;
                        }

                        _clickAverages[x] = _clickAverages[x - 1] * 0.85f + MathF.Abs(_clickValues[x]) * 0.15f;
                        _lowEma[x] = _lowEma[x - 1] * 0.98f + _clickAverages[x] * 0.02f;

                        _ampEma[x] = _ampEma[x - 1] * 0.95f + _maxes[x] * 0.05f;
                    }

                    //int bufferSize = Params._windowSize * _bytesPerPixel;
                    int stride = (_width * _bytesPerPixel + 3) & ~3;
                    int bufferSize = (Params._windowSize / 2) * stride;
                    //int bufferSize = _width * _bytesPerPixel;

                    float clickValuesMax = 1f;
                    if (_clickValues.Count() > 0)
                        clickValuesMax = _clickValues.Max();

                    InitBitmapes();

                    byte[] pixels1 = new byte[bufferSize];
                    byte[] pixels2 = new byte[bufferSize];
                    byte[] pixels3 = new byte[bufferSize];

                    for (int x = 0; x < _width; x++)
                    {
                        float clickValue = -Math.Abs(_clickValues[x]) / clickValuesMax / 4 * (float)Params._windowSize / 4;

                        float clickAverage = 0;
                        if (x > 0)
                            clickAverage = _clickThreshold * -_clickAverages[x - 1] / clickValuesMax / 4 * (float)Params._windowSize / 4;

                        float clickAverageEma = 0;
                        if (x > 0)
                            clickAverageEma = _loudnessThreshold * -_lowEma[x - 1] / clickValuesMax / 4 * (float)Params._windowSize / 4;

                        float ampLim = _ampEma[x] * _maxThreshold * 255;

                        for (int y = 0; y < Params._windowSize / 2; y++)
                        {
                            byte red = (byte)(Math.Abs(_fft[x][y].Real) / globalMax * 255);
                            byte blue = (byte)(Math.Abs(_fft[x][y].Imaginary) / globalMax * 255);
                            byte magnitude = (byte)(_fft[x][y].Magnitude / globalMax * 255);

                            float actPhase = (_phases[x][y] - 0.5f) * 255 * 2; //
                            byte phaseG = 0;
                            byte phaseB = 0;

                            if (actPhase > 0)
                                phaseG = (byte)(actPhase);
                            else
                                phaseB = (byte)(-actPhase);

                            float actRephase = Math.Clamp((_rephases[x][y] - 0.5f) * 500 * 1000, -500, 500);
                            byte rephaseG = 0;
                            byte rephaseB = 0;

                            if (actRephase > 0)
                                rephaseG = (byte)(actRephase);
                            else
                                rephaseB = (byte)(-actRephase);

                            int offset = y * stride + x * _bytesPerPixel;

                            float d = y - (Params._windowSize / 2 / 2);
                            bool isClickLiterally = d < 0 && clickValue < 0 && d > clickValue || d > 0 && clickValue > 0 && d < clickValue;
                            bool isAverage = d < 0 && d > clickAverage;
                            bool emaed = d < 0 && d < clickAverageEma;
   
                            bool loud = d < -Math.Abs(ampLim);

                            if ((int)d == (int)-ampLim)
                            {
                                pixels1[offset] = 0;
                                pixels1[offset + 1] = 255;
                                pixels1[offset + 2] = 255;
                            }
                            else if ((int)d == (int)clickAverage)
                            {
                                pixels1[offset] = 0;
                                pixels1[offset + 1] = 128;
                                pixels1[offset + 2] = 255;
                            }
                            else if ((int)d == (int)clickAverageEma)
                            {
                                pixels1[offset] = 0;
                                pixels1[offset + 1] = 255;
                                pixels1[offset + 2] = 0;
                            }
                            else if (!isClickLiterally)
                            {
                                pixels1[offset] = rephaseB;
                                pixels1[offset + 1] = rephaseG;
                                pixels1[offset + 2] = 0;
                            }
                            else if (!isAverage && emaed && loud)
                            {
                                pixels1[offset] = 0;
                                pixels1[offset + 1] = 0;
                                pixels1[offset + 2] = 255;
                            }
                            else if (isAverage && emaed)
                            {
                                pixels1[offset] = 255;
                                pixels1[offset + 1] = 0;
                                pixels1[offset + 2] = 0;
                            }
                            else
                            {
                                pixels1[offset] = 0;
                                pixels1[offset + 1] = 255;
                                pixels1[offset + 2] = 0;
                            }

                            pixels2[offset] = blue; // B
                            pixels2[offset + 1] = 0; // G
                            pixels2[offset + 2] = red; // R
                            //pixels2[offset + 3] = blue; //A


                            pixels3[offset] = phaseB;
                            pixels3[offset + 1] = phaseG;
                            pixels3[offset + 2] = magnitude;
                            //pixels3[offset + 3] = 255;
                        }
                    }

                    Int32Rect rect = new Int32Rect(0, 0, _width, Params._windowSize / 2);
                    stride = (_width * _bytesPerPixel + 3) & ~3;
                    _bitmap1.WritePixels(rect, pixels1, stride, 0);
                    
                    _bitmap2.WritePixels(rect, pixels2, stride, 0);
                    
                    _bitmap3.WritePixels(rect, pixels3, stride, 0);

                    SaveBitmap(_bitmap1, $"Fft Phase {adder}");
                    SaveBitmap(_bitmap2, $"Fft Amplitude {adder}");
                    SaveBitmap(_bitmap3, $"Fft Mixed {adder}");
                }
            }

            void InitSpectraliser()
            {
                _fft = new Complex[_width][];
                _phases = new float[_width][];
                _dphases = new float[_width][];
                _smoothdphases = new float[_width][];
                _rephases = new float[_width][];
                _clickValues = new float[_width];
                _clickAverages = new float[_width];
                _lowEma = new float[_width];
                _ampEma = new float[_width];
                _maxes = new float[_width];

                _startIndex = 0;
            }
        }

        public static void SmoothPhases(int x)
        {
            float[] newPhases = new float[Params._windowSize / 2];

            for (int y = 0; y < Params._windowSize / 2; y++)
            {
                float sum = 0f;
                int count = 0;

                for (int dy = -_phasesSmooth; dy <= _phasesSmooth; dy++)
                {
                    int neighborY = y + dy;

                    if (neighborY >= 0 && neighborY < Params._windowSize / 2)
                    {
                        sum += _phases[x][neighborY]; //////////////////////
                        count++;
                    }
                }

                newPhases[y] = sum / count;
            }

            for (int y = 0; y < Params._windowSize / 2; y++)
                _phases[x][y] = newPhases[y];
        }

        public static void InitFirst()
        {
            _step = (int)(Params._windowSize * (1 - Params._overlap));

            _width = (int)Math.Ceiling((double)(_wav.Samples[0].Length - Params._windowSize) / _step) + 1;

            _fft = new Complex[_width][];
            _phases = new float[_width][];
            _dphases = new float[_width][];
            _smoothdphases = new float[_width][];
            _rephases = new float[_width][];
            _clickValues = new float[_width];
            _clickAverages = new float[_width];
            _lowEma = new float[_width];
            _ampEma = new float[_width];
            _maxes = new float[_width];
            _average = 0.025f;
            _deltas = new float[_width];
        }

        public static void InitBitmapes()
        {
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

        public static void CalculateLFCompensations()
        {
            _lfCompensations = new float[Params._windowSize / 2];
/*            int nyquistBin = Params._windowSize / 2;
            double cutoff = 0.4375; // (43.75% from Nyquist)

            double fall = 1 / (_lowFreqCompensate);

            double j = _lfCompensations.Length - 1;
            double normalizedFreq = (double)j / nyquistBin;
            double energyFalloff = 1.0 / (1.0 + Math.Pow(normalizedFreq / cutoff, 2));
            double max = (float)(_lowFreqCompensate / energyFalloff);
            double _compensation = _lowFreqCompensate / energyFalloff;*/

            for (int x = 0; x < _lfCompensations.Length; x++)
            {
                /*                normalizedFreq = (double)i / nyquistBin; //yep
                                energyFalloff = 1.0 / (1.0 + Math.Pow(normalizedFreq / cutoff, 2)); //yep
                                _lfCompensations[i] = (float)(_lowFreqCompensate / energyFalloff / max);*/

                //_lfCompensations[x] = 1 - 1 / (x / _lowFreqCompensate + 1);
                _lfCompensations[x] = (float)Math.Pow(x / (Params._windowSize / 2f), 2 * _lowFreqCompensate);
            }
        }

        public static bool Parse(string ct, string ps, string ht, string st, string et, string lfc, string mt, string lt, string maxThreshold)
        {
            try
            {
                _clickThreshold = Convert.ToSingle(ct, CultureInfo.InvariantCulture);
                _phasesSmooth = Convert.ToInt32(ps, CultureInfo.InvariantCulture);
                _horizontalThreshold = Convert.ToSingle(ht, CultureInfo.InvariantCulture);
                _lowFreqCompensate = Convert.ToSingle(lfc, CultureInfo.InvariantCulture);
                _threshold = 0.5f;
                _loudnessThreshold = Convert.ToSingle(lt, CultureInfo.InvariantCulture);
                _maxThreshold = Convert.ToSingle(maxThreshold, CultureInfo.InvariantCulture);
                _multiplier = Convert.ToSingle(mt, CultureInfo.InvariantCulture);

                _st = 0;
                if (!string.IsNullOrWhiteSpace(st))
                    _st = Convert.ToSingle(st, CultureInfo.InvariantCulture);

                _et = 0;
                if (!string.IsNullOrWhiteSpace(et))
                    _et = Convert.ToSingle(et, CultureInfo.InvariantCulture);

                _bytesPerPixel = 3; ////////////////////

                _startIndex = 0;

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                return false;
            }
        }

        public static void InitFFR()
        {
            for (int x = 1; x < _samples.Length; x++)
            {
                bool clickWasHere = false;
                float delta = _samples[x] - _samples[x - 1];

                if (MathF.Abs(delta) > _average)
                {
                    _deltaRemembered = delta;
                    clickWasHere = true;
                    int k = 0;

                    while (MathF.Abs(delta) > _average * 2 && k < 5)
                    {
                        delta = (_deltaRemembered * 19 + delta) / 20;
                        k++;
                    }
                }

                if (clickWasHere)
                {
                    _samples[x] = _samples[x - 1] + delta;
                    Logger.Log($"Click on sample {x}. Delta downscaled from {_deltaRemembered} to {delta}. Average is {_average}.");
                    Logger.Log($"Declicked.");
                    clickWasHere = false;
                }

                _average = _average * 0.999f + MathF.Abs(delta) * 0.001f;
            }
        }

        public static double[] CalculatePhaseShifts(int windowSize, int step)
        {
            var frequencies = ComputeFFTFrequencies(windowSize);
            double[] res = new double[frequencies.Length];
            for (int i = 0; i < frequencies.Length; i++)
                res[i] = CalculatePhase(frequencies[i], step, windowSize);

            return res;
        }

        public static double[] ComputeFFTFrequencies(int N)
        {
            int numPoints = N / 2 + 1;
            double[] frequencies = new double[numPoints];

            for (int k = 0; k < numPoints; k++)
                frequencies[k] = (double)k / N;

            return frequencies;
        }

        public static double CalculatePhase(double frequencyPerCycle, int shift, int sampleSize)
        {
            double phase = (2 * Math.PI * frequencyPerCycle * shift) / sampleSize;
            phase %= 2 * Math.PI; // Приводим фазу к диапазону [-2π, 2π)

            // Корректируем отрицательные значения
            if (phase < 0)
                phase += 2 * Math.PI;

            return phase;
        }

        public static double CalculateLoudness(Complex[] fftResult, int sampleRate)
        {
            double P0 = 20e-6;

            int n = (fftResult.Length - 1) * 2;
            double weightedEnergy = 0.0;

            for (int k = 0; k < fftResult.Length; k++)
            {
                double frequency = (double)k * sampleRate / n;

                double aWeightDb = GetAWeighting(frequency);

                double aWeightLinear = Math.Pow(10, aWeightDb / 20);

                double amplitude = fftResult[k].Magnitude / n;

                double binEnergy = amplitude * amplitude * aWeightLinear;

                if (k > 0 && k != fftResult.Length - 1)
                    binEnergy *= 2;

                weightedEnergy += binEnergy;
            }

            double rms = Math.Sqrt(weightedEnergy);
            return 20 * Math.Log10(rms / P0);
        }

        private static double GetAWeighting(double frequency)
        {
            if (frequency < 20) return double.NegativeInfinity;

            double f2 = frequency * frequency;
            double numerator = 12194 * 12194 * f2 * f2;
            double denominator = (f2 + 20.6 * 20.6)
                              * Math.Sqrt((f2 + 107.7 * 107.7) * (f2 + 737.9 * 737.9))
                              * (f2 + 12194 * 12194);

            return 2.0 + 20 * Math.Log10(numerator / denominator);
        }
    }
}