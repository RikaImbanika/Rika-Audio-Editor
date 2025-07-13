using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIKA_AUDIO
{
    public class TrackOnLine
    {
        private byte _bitDepth;
        private int _finalLength;
        public UInt32 _sampleRate;

        public static Sample[] _finalSamplesL;
        public static Sample[] _finalSamplesR;

        public static Sample[] _origSamplesL;
        public static Sample[] _origSamplesR;

        public byte BitDepth
        {
            get
            {
                return _bitDepth;
            }
            set
            {
                _bitDepth = value;
            }
        }

        public Int32 OrigLength
        {
            get
            {
                return _origSamplesL.Length;
            }
        }

        public Int32 FinalLength
        {
            get
            {
                return _finalLength;
            }
            set
            {
                _finalLength = value;
            }
        }

        public double Speed
        {
            get
            {
                return 1.0 * _origSamplesL.Length / _finalLength;
            }
            set
            {
                _finalLength = (int)(_origSamplesL.Length * value);
            }
        }
    }
}
