using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIKA_AUDIO
{
    public static class Calculator
    {
        public static void Calculate()
        {
            double a = 0;
            double b = 0;

            a = System.Math.Log10(2 ^ 15);
            b = 20.0 * a;

            Logger.Log($"a = {a}");
            Logger.Log($"b = {b}");
        }
    }
}