using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rika_Audio
{
    public class Wav
    {
        public int Channels { get; set; }
        public int SampleRate { get; set; }
        public int BitDepth { get; set; }
        public float[][] Samples { get; set; }

        public static void Save(string path, Wav wav)
        {
            ValidateWavParameters(wav);

            Logger.Log($"Saving wav...");

            using (var writer = new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                WriteRiffHeader(writer);
                WriteFmtChunk(writer, wav);
                long dataChunkPosition = WriteDataHeader(writer);
                byte[] audioData = ConvertSamplesToBytes(wav);
                writer.Write(audioData);
                UpdateFileSizes(writer, dataChunkPosition, audioData.Length);
            }

            Logger.Log($"Saved wav.");
        }

        private static void ValidateWavParameters(Wav wav)
        {
            if (wav.Channels <= 0)
                throw new ArgumentException("Channels must be positive");
            if (wav.SampleRate <= 0)
                throw new ArgumentException("Sample rate must be positive");
            if (wav.BitDepth % 8 != 0 || wav.BitDepth < 8 || wav.BitDepth > 32)
                throw new ArgumentException("Bit depth must be 8, 16, 24, or 32");
            if (wav.Samples[0] == null)
                throw new ArgumentNullException(nameof(wav.Samples));
        }

        private static void WriteRiffHeader(BinaryWriter writer)
        {
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(0u); // Placeholder for file size
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));
        }

        private static void WriteFmtChunk(BinaryWriter writer, Wav wav)
        {
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16u); // fmt chunk size (16 bytes for PCM)

            ushort audioFormat = (ushort)(wav.BitDepth == 32 ? 3 : 1); // 1 = PCM, 3 = IEEE float
            ushort blockAlign = (ushort)(wav.Channels * (wav.BitDepth / 8));
            uint byteRate = (uint)(wav.SampleRate * blockAlign);

            writer.Write(audioFormat);
            writer.Write((ushort)wav.Channels);
            writer.Write(wav.SampleRate);
            writer.Write(byteRate);
            writer.Write(blockAlign);
            writer.Write((ushort)wav.BitDepth);
        }

        private static long WriteDataHeader(BinaryWriter writer)
        {
            writer.Write(Encoding.ASCII.GetBytes("data"));
            long dataSizePosition = writer.BaseStream.Position;
            writer.Write(0u); // Placeholder for data size
            return dataSizePosition;
        }

        private static byte[] ConvertSamplesToBytes(Wav wav)
        {
            using var ms = new MemoryStream();
            using var dataWriter = new BinaryWriter(ms);

            bool isStereo = wav.Channels == 2 && wav.Samples[1] != null;
            int samplesCount = wav.Samples[0].Length;

            if (isStereo && wav.Samples[1].Length != samplesCount)
                throw new InvalidDataException("Каналы имеют разную длину");

            for (int i = 0; i < samplesCount; i++)
            {
                float clampedL = Math.Clamp(wav.Samples[0][i], -1.0f, 1.0f);
                WriteSampleData(wav.BitDepth, dataWriter, clampedL);

                if (isStereo)
                {
                    float clampedR = Math.Clamp(wav.Samples[1][i], -1.0f, 1.0f);
                    WriteSampleData(wav.BitDepth, dataWriter, clampedR);
                }
            }

            return ms.ToArray();

            void WriteSampleData(int bitDepth, BinaryWriter writer, float clampedValue)
            {
                switch (bitDepth)
                {
                    case 8:
                        writer.Write((byte)((clampedValue * 127.0f) + 128));
                        break;
                    case 16:
                        writer.Write((short)(clampedValue * 32767.0f));
                        break;
                    case 24:
                        int int24 = (int)(clampedValue * 8388607.0f);
                        writer.Write((byte)(int24));
                        writer.Write((byte)(int24 >> 8));
                        writer.Write((byte)(int24 >> 16));
                        break;
                    case 32:
                        writer.Write(clampedValue); // Для 32-bit float
                        break;
                    default:
                        throw new NotSupportedException($"Bit depth {bitDepth} not supported");
                }
            }
        }
        

        private static void UpdateFileSizes(BinaryWriter writer, long dataSizePosition, int dataLength)
        {
            // Update RIFF chunk size
            writer.Seek(4, SeekOrigin.Begin);
            writer.Write((uint)(writer.BaseStream.Length - 8));

            // Update data chunk size
            writer.Seek((int)dataSizePosition, SeekOrigin.Begin);
            writer.Write((uint)dataLength);
        }
    }
}