using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIKA_IMBANIKA_AUDIO
{
    public static class RikaMath
    {
        public static double _root;
        public static double _minDb;

        static RikaMath()
        {
            _minDb = -120; //We think that we can't hear less than that
            _root = 20;

            double minAbsolute = System.Math.Pow(10, _minDb / 20.0);

            //This is min we can hear but in double, not in decibells
            //Min of double is -1.7976931348623157E+308
            //Max of double is 1.7976931348623157E+308;
        }

        public static Int16 Logarythmise(Int16 x0)
        {
            bool negative = x0 < 0;
            double x = x0;

            if (negative)
                x = -x; ////////////////////////////////////

            const double minDB = 0;// -(Int16.MaxValue / _root);
            const double maxDB = 0.0;

            // Обработка нулевых и малых значений
            if (x <= minDB)
                return (short)System.Math.Pow(10, minDB / 20.0);

            // Прямое преобразование в децибелы
            double dB = 20.0 * System.Math.Log10(x);

            // Нормализация в диапазон [0, 1]
            double normalized = (dB - minDB) / (maxDB - minDB);

            if (double.IsNaN(normalized))
            {

            }

            return (short)System.Math.Max(0.0, System.Math.Min(1.0, normalized)); //
        }
    }
}
