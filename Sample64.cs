using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIKA_AUDIO
{
    public struct Sample64 : Sample
    {
        private Int64 _sample;

        public Int64 S
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
