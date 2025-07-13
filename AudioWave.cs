using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIKA_AUDIO
{
    public struct AudioWave
    {
        public static Sample[] _l;
        public static Sample[] _r;

        public static Sample[] L
        {
            get
            {
                return _l;
            }
            set
            {
                _l = value;
            }
        }

        public static Sample[] R
        {
            get
            {
                return _r;
            }
            set
            {
                _r = value;
            }
        }
    }
}
