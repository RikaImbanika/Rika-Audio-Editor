using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIKA_AUDIO
{
    public static class AudioParser
    {
        public static AudioWave Parse(byte wavBytes)
        {
            var wav = new Wav();
            using var ms = new MemoryStream(wavBytes);
            using var reader = new BinaryReader(ms);

            reader.ReadBytes(4); // "RIFF"
            reader.ReadInt32(); // file size
            reader.ReadBytes(4); // "WAVE"

            reader.ReadBytes(4); // "fmt "
            int fmtSize = reader.ReadInt32();
            int formatTag = reader.ReadInt16();

            wav.Channels = reader.ReadInt16();
            wav.SampleRate = reader.ReadInt32();
            reader.ReadInt32(); // byte rate
            reader.ReadInt16(); // block align
            wav.BitDepth = reader.ReadInt16();
            wav.Samples = new float[2][];

            reader.ReadBytes(fmtSize - 16);

            while (reader.ReadInt32() != 0x61746164) // Find "data" chunk
            {
                int dataSize = reader.ReadInt32();
                reader.ReadBytes(dataSize);
            }

            int dataBytes = reader.ReadInt32();
            byte[] data = reader.ReadBytes(dataBytes);

            if (wav.Channels < 1 || wav.Channels > 2)
                throw new NotSupportedException($"Unsupported channels: {wav.Channels}");

            var (samplesL, samplesR) = (formatTag, wav.BitDepth) switch
            {
                (1, 16) => Convert16Bit(data, wav.Channels),
                (1, 32) => Convert32BitInt(data, wav.Channels),
                (3, 32) => Convert32BitFloat(data, wav.Channels),
                _ => throw new NotSupportedException($"Unsupported format: {formatTag}/{wav.BitDepth}bit")
            };

            wav.Samples[0] = samplesL;
            wav.Samples[1] = wav.Channels == 2 ? samplesR : null;

            return new AudioWave(); //
        }

        public static Wav Parse(byte[] wavBytes)
        {
            var wav = new Wav();
            using var ms = new MemoryStream(wavBytes);
            using var reader = new BinaryReader(ms);

            reader.ReadBytes(4); // "RIFF"
            reader.ReadInt32(); // file size
            reader.ReadBytes(4); // "WAVE"

            reader.ReadBytes(4); // "fmt "
            int fmtSize = reader.ReadInt32();
            int formatTag = reader.ReadInt16();

            wav.Channels = reader.ReadInt16();
            wav.SampleRate = reader.ReadInt32();
            reader.ReadInt32(); // byte rate
            reader.ReadInt16(); // block align
            wav.BitDepth = reader.ReadInt16();
            wav.Samples = new float[2][];

            reader.ReadBytes(fmtSize - 16);

            while (reader.ReadInt32() != 0x61746164) // Find "data" chunk
            {
                int dataSize = reader.ReadInt32();
                reader.ReadBytes(dataSize);
            }

            int dataBytes = reader.ReadInt32();
            byte[] data = reader.ReadBytes(dataBytes);

            if (wav.Channels < 1 || wav.Channels > 2)
                throw new NotSupportedException($"Unsupported channels: {wav.Channels}");

            var (samplesL, samplesR) = (formatTag, wav.BitDepth) switch
            {
                (1, 16) => Convert16Bit(data, wav.Channels),
                (1, 32) => Convert32BitInt(data, wav.Channels),
                (3, 32) => Convert32BitFloat(data, wav.Channels),
                _ => throw new NotSupportedException($"Unsupported format: {formatTag}/{wav.BitDepth}bit")
            };

            wav.Samples[0] = samplesL;
            wav.Samples[1] = wav.Channels == 2 ? samplesR : null;

            return wav;
        }

        private static AudioWave SConvert16Bit(byte[] data, int channels)
        {
            int sampleCount = data.Length / 2;
            int perChannel = sampleCount / channels;
            Sample32[] left = new Sample32[perChannel];
            Sample32[] right = channels == 2 ? new Sample32[perChannel] : null;

            for (int i = 0; i < perChannel; i++)
            {
                int offset = i * channels * 2;
                left[i].S = BitConverter.ToInt16(data, offset);
                if (channels == 2)
                    right[i].S = BitConverter.ToInt16(data, offset + 2);
            }
            return new AudioWave(); /////////
        }


        private static (float[] left, float[] right) Convert16Bit(byte[] data, int channels)
        {
            int sampleCount = data.Length / 2;
            int perChannel = sampleCount / channels;
            float[] left = new float[perChannel];
            float[] right = channels == 2 ? new float[perChannel] : null;

            for (int i = 0; i < perChannel; i++)
            {
                int offset = i * channels * 2;
                left[i] = BitConverter.ToInt16(data, offset) / 32768f;
                if (channels == 2)
                    right[i] = BitConverter.ToInt16(data, offset + 2) / 32768f;
            }
            return (left, right);
        }

        private static (float[] left, float[] right) Convert32BitFloat(byte[] data, int channels)
        {
            int sampleCount = data.Length / 4;
            int perChannel = sampleCount / channels;
            float[] left = new float[perChannel];
            float[] right = channels == 2 ? new float[perChannel] : null;

            for (int i = 0; i < perChannel; i++)
            {
                int offset = i * channels * 4;
                left[i] = BitConverter.ToSingle(data, offset);
                if (channels == 2)
                    right[i] = BitConverter.ToSingle(data, offset + 4);
            }
            return (left, right);
        }

        private static (float[] left, float[] right) Convert32BitInt(byte[] data, int channels)
        {
            int sampleCount = data.Length / 4;
            int perChannel = sampleCount / channels;
            float[] left = new float[perChannel];
            float[] right = channels == 2 ? new float[perChannel] : null;

            for (int i = 0; i < perChannel; i++)
            {
                int offset = i * channels * 4;
                left[i] = BitConverter.ToInt32(data, offset) / 2147483648f;
                if (channels == 2)
                    right[i] = BitConverter.ToInt32(data, offset + 4) / 2147483648f;
            }
            return (left, right);
        }
    }
}
