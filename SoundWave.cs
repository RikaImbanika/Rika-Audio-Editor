using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIKA_IMBANIKA_AUDIO
{
    public class SoundWave
    {
        public int _bitDepth;
        public bool _stereo;

        public Int16[] _left16bit;
        public Int16[] _right16bit;

        public Int32[] _left32bit;
        public Int32[] _right32bit;

        public Int64[] _left64bit;
        public Int64[] _right64bit;
    }
}
