using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIKA_AUDIO
{
    public struct Complex
    {
        public double Real;
        public double Imaginary;
        public double Magnitude;
        public double Phase;

        public Complex(double real, double imaginary, double magnitude = 0, double phase = 0)
        {
            Real = real;
            Imaginary = imaginary;
            Magnitude = magnitude;
            Phase = phase;
        }

        public double CalcMagnitude => System.Math.Sqrt(Real * Real + Imaginary * Imaginary);

        public double CalcPhase => System.Math.Atan2(Imaginary, Real);

        public static Complex operator +(Complex a, Complex b)
        {
            var res = new Complex(a.Real + b.Real, a.Imaginary + b.Imaginary);
/*            res.Magnitude = res.CalcMagnitude;
            res.Phase = res.CalcPhase;*/
            return res;
        }

        public static Complex operator -(Complex a, Complex b)
        {
            var res = new Complex(a.Real - b.Real, a.Imaginary - b.Imaginary);
/*            res.Magnitude = res.CalcMagnitude;
            res.Phase = res.CalcPhase;*/
            return res;
        }

        public static Complex operator *(Complex a, Complex b)
        {
            var res = new Complex(
                a.Real * b.Real - a.Imaginary * b.Imaginary,
                a.Real * b.Imaginary + a.Imaginary * b.Real
            );

            return res;
        }

        public static Complex operator /(Complex a, double scalar)
        {
            if (System.Math.Abs(scalar) < double.Epsilon)
                throw new DivideByZeroException("Division by zero is not allowed");

            return new Complex(a.Real / scalar, a.Imaginary / scalar, a.Magnitude / scalar, a.Phase);
        }

        public static Complex operator *(Complex a, double scalar)
        {
            return new Complex(a.Real * scalar, a.Imaginary * scalar, a.Magnitude * scalar, a.Phase);
        }
    }
}
