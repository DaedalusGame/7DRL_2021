using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    public static class LerpHelper
    {
        public delegate double Delegate(double a, double b, double amt);

        public static Delegate Invert(this Delegate lerp)
        {
            return (a, b, amt) => lerp(b, a, amt);
        }

        public static Delegate ForwardReverse(float start, float end, Delegate lerpStart, Delegate lerpEnd)
        {
            return (a, b, amt) => ForwardReverse(a, b, amt, start, end, lerpStart, lerpEnd);
        }

        public static double ForwardReverse(double a, double b, double amt)
        {
            if (amt < 0.5)
                return Linear(a, b, amt * 2);
            else
                return Linear(b, a, (amt - 0.5) * 2);
        }

        public static double ForwardReverse(double a, double b, double amt, float start, float end, Delegate lerpStart, Delegate lerpEnd)
        {
            if (amt < start)
                return lerpStart(a, b, Util.ReverseLerp(amt, 0, start));
            else if (amt < end)
                return b;
            else
                return lerpEnd(b, a, Util.ReverseLerp(amt, end, 1));
        }

        public static double Flick(double a, double b, double amt)
        {
            if (amt < 1)
                return a;
            else
                return b;
        }

        public static double Linear(double a, double b, double amt)
        {
            return a * (1 - amt) + b * amt;
        }

        public static double QuadraticIn(double a, double b, double amt)
        {
            return Linear(a, b, amt * amt);
        }

        public static double QuadraticOut(double a, double b, double amt)
        {
            return Linear(a, b, 1 - (amt - 1) * (amt - 1));
        }

        public static double Quadratic(double a, double b, double amt)
        {
            amt *= 2;
            if (amt < 1)
                return Linear(a, b, 0.5 * amt * amt);
            else
                return Linear(a, b, -0.5 * ((amt - 1) * (amt - 3) - 1));
        }

        public static double CubicIn(double a, double b, double amt)
        {
            return Linear(a, b, amt * amt * amt);
        }

        public static double CubicOut(double a, double b, double amt)
        {
            return Linear(a, b, 1 + (amt - 1) * (amt - 1) * (amt - 1));
        }

        public static double Cubic(double a, double b, double amt)
        {
            amt *= 2;
            if (amt < 1)
                return Linear(a, b, 0.5 * amt * amt * amt);
            else
                return Linear(a, b, 0.5 * ((amt - 2) * (amt - 2) * (amt - 2) + 2));
        }

        public static double QuarticIn(double a, double b, double amt)
        {
            return Linear(a, b, amt * amt * amt * amt);
        }

        public static double QuarticOut(double a, double b, double amt)
        {
            return Linear(a, b, 1 - (amt - 1) * (amt - 1) * (amt - 1) * (amt - 1));
        }

        public static double Quartic(double a, double b, double amt)
        {
            amt *= 2;
            if (amt < 1)
                return Linear(a, b, 0.5 * amt * amt * amt * amt);
            else
                return Linear(a, b, -0.5 * ((amt - 2) * (amt - 2) * (amt - 2) * (amt - 2) - 2));
        }

        public static double QuinticIn(double a, double b, double amt)
        {
            return Linear(a, b, amt * amt * amt * amt * amt);
        }

        public static double QuinticOut(double a, double b, double amt)
        {
            return Linear(a, b, 1 + (amt - 1) * (amt - 1) * (amt - 1) * (amt - 1) * (amt - 1));
        }

        public static double Quintic(double a, double b, double amt)
        {
            amt *= 2;
            if (amt < 1)
                return Linear(a, b, 0.5 * amt * amt * amt * amt * amt);
            else
                return Linear(a, b, 0.5 * ((amt - 2) * (amt - 2) * (amt - 2) * (amt - 2) * (amt - 2) + 2));
        }

        public static double SineIn(double a, double b, double amt)
        {
            return Linear(a, b, 1 - Math.Cos(amt * Math.PI / 2));
        }

        public static double SineOut(double a, double b, double amt)
        {
            return Linear(a, b, Math.Sin(amt * Math.PI / 2));
        }

        public static double Sine(double a, double b, double amt)
        {
            return Linear(a, b, 0.5 * (1 - Math.Cos(amt * Math.PI)));
        }

        public static double ExponentialIn(double a, double b, double amt)
        {
            if (amt <= 0)
                return a;
            if (amt >= 1)
                return b;
            return Linear(a, b, Math.Pow(1024, amt - 1));
        }

        public static double ExponentialOut(double a, double b, double amt)
        {
            if (amt <= 0)
                return a;
            if (amt >= 1)
                return b;
            return Linear(a, b, 1 - Math.Pow(2, -10 * amt));
        }

        public static double Exponential(double a, double b, double amt)
        {
            if (amt <= 0)
                return a;
            if (amt >= 1)
                return b;
            amt *= 2;
            if (amt < 1)
                return Linear(a, b, 0.5 * Math.Pow(1024, amt - 1));
            else
                return Linear(a, b, -0.5 * Math.Pow(2, -10 * (amt - 1)) + 1);
        }

        public static double CircularIn(double a, double b, double amt)
        {
            return Linear(a, b, 1 - Math.Sqrt(1 - amt * amt));
        }

        public static double CircularOut(double a, double b, double amt)
        {
            return Linear(a, b, Math.Sqrt(1 - (amt - 1) * (amt - 1)));
        }

        public static double Circular(double a, double b, double amt)
        {
            amt *= 2;
            if (amt < 1)
                return Linear(a, b, -0.5 * (Math.Sqrt(1 - amt * amt) - 1));
            else
                return Linear(a, b, 0.5 * (Math.Sqrt(1 - (amt - 2) * (amt - 2)) + 1));
        }

        public static double ElasticIn(double a, double b, double amt)
        {
            return ElasticInCustom(a, b, 0.1, 0.4, amt);
        }

        public static double ElasticInCustom(double a, double b, double k, double p, double amt)
        {
            if (amt <= 0)
                return a;
            if (amt >= 1)
                return b;

            double s;

            if (k < 1)
            {
                k = 1;
                s = p / 4;
            }
            else
            {
                s = p * Math.Asin(1 / k) / (Math.PI * 2);
            }

            return Linear(a, b, -k * Math.Pow(2, 10 * (amt - 1)) * Math.Sin(((amt - 1) - s) * (2 * Math.PI) / p));
        }

        public static double ElasticOut(double a, double b, double amt)
        {
            return ElasticOutCustom(a, b, 0.1, 0.4, amt);
        }

        public static double ElasticOutCustom(double a, double b, double k, double p, double amt)
        {
            if (amt <= 0)
                return a;
            if (amt >= 1)
                return b;

            double s;

            if (k < 1)
            {
                k = 1;
                s = p / 4;
            }
            else
            {
                s = p * Math.Asin(1 / k) / (Math.PI * 2);
            }

            return Linear(a, b, k * Math.Pow(2, -10 * amt) * Math.Sin((amt - s) * (2 * Math.PI) / p) + 1);
        }

        public static double Elastic(double a, double b, double amt)
        {
            return ElasticCustom(a, b, 0.1, 0.4, amt);
        }

        public static double ElasticCustom(double a, double b, double k, double p, double amt)
        {
            if (amt <= 0)
                return a;
            if (amt >= 1)
                return b;

            double s;

            if (k < 1)
            {
                k = 1;
                s = p / 4;
            }
            else
            {
                s = p * Math.Asin(1 / k) / (Math.PI * 2);
            }

            amt *= 2;
            if (amt < 1)
                return Linear(a, b, -0.5 * k * Math.Pow(2, 10 * (amt - 1)) * Math.Sin(((amt - 1) - s) * (2 * Math.PI) / p));
            else
                return Linear(a, b, 0.5 * k * Math.Pow(2, -10 * (amt - 1)) * Math.Sin(((amt - 1) - s) * (2 * Math.PI) / p) + 1);
        }


        public static double BackIn(double a, double b, double amt) => BackInCustom(a, b, 1.7f, amt);

        public static double BackInCustom(double a, double b, double c, double amt)
        {
            if (amt <= 0)
                return a;
            if (amt >= 1)
                return b;

            return (c + 1) * amt * amt * amt;
        }

        public static double BackOut(double a, double b, double amt) => BackOutCustom(a, b, 1.7f, amt);

        public static double BackOutCustom(double a, double b, double c, double amt)
        {
            if (amt <= 0)
                return a;
            if (amt >= 1)
                return b;

            return 1 + (c + 1) * Math.Pow(amt - 1, 3) + c * Math.Pow(amt - 1, 2);
        }
    }
}
