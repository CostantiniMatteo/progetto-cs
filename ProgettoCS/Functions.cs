using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgettoCS
{
    public static class Functions
    {

        public static double[] Smooth(double[] data, int range)
        {

            var smoothed = new double[data.Length];

            for(var i = range; i < data.Length - range; i++)
            {
                smoothed[i] = Mean(data, i - range, i + range);
            }

            return null;
        }


        public static double Mean(double[] data, int s = 0, int e = -1)
        {
            e = e < 0 ? data.Length : e;

            double res = 0;
            for(var i = s; i <= e; i++)
                res += data[i];
            return res / data.Length;
        }


        public static double Modulus(double v1, double v2, double v3)
        {
            return Math.Sqrt(v1 * v1 + v2 * v2 + v3 * v3);
        }


        public static double[] DifferenceQuotient(double[] data, double increment = 0.02)
        {
            var res = new double[data.Length];

            for(var i = 0; i < data.Length - 1; i++)
                res[i] = data[i + 1] - data[i] / increment;

            return res;

        }

        public static double[] StdDev(double[] data, int range)
        {
            var res = new double[data.Length];
            double mean = Mean(data);

            for(var i = range; i < data.Length - range; i++)
                res[i] = _stdDev(data, i - range, i + range);

            return res;


        }

        private static double _stdDev(double[] data, int s, int e)
        {
            e = e < 0 ? data.Length : e;

            double res = 0;
            double mean = Mean(data, s, e);

            for(var i = s; i <= e; i++)
                res += Math.Pow(data[i] - mean, 2);

            return Math.Sqrt(res / (e - s));
        }

    }
}
