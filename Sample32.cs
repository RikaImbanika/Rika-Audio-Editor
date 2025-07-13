using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIKA_AUDIO
{
    public struct Sample32 : Sample
    {
        private Int32 _bits;

        public Int32 S
        {
            get
            {
                return _bits;
            }
            set
            {
                _bits = value;
            }
        }
    }
}
