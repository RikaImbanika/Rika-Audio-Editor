using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIKA_IMBANIKA_AUDIO
{
    public struct Sample128 : Sample
    {
        private Int128 _sample;

        public Int128 S
        {
            get
            {
                return _sample;
            }
            set
            {
                _sample = value;
            }
        }
    }
}
