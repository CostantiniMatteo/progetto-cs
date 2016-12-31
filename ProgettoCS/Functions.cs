using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgettoCS
{
    public static class Functions
    {

        public static List<double> Smooth(List<double> data, int range)
        {

            var smoothed = new List<double>(data.Count - (2 * range));

            for(var i = range; i < data.Count - range; i++)
            {
                smoothed.Add(Mean(data, i - range, i + range));
            }

            return smoothed;
        }


        public static double Mean(List<double> data, int s = 0, int e = -1)
        {
            e = e < 0 ? data.Count : e;

            double res = 0;
            for(var i = s; i <= e; i++)
                res += data[i];
            return res / (e-s);
        }


        public static double Modulus(double v1, double v2, double v3)
        {
            return Math.Sqrt(v1 * v1 + v2 * v2 + v3 * v3);
        }


        public static List<double> DifferenceQuotient(List<double> data, double increment = 0.02)
        {
            var res = new List<double>(data.Count - 1);

            for(var i = 1; i < data.Count; i++)
                res.Add(data[i] - data[i - 1] / increment);

            return res;

        }

        public static double[] StdDev(List<double> data, int range)
        {
            var res = new double[data.Count];
            double mean = Mean(data);

            for(var i = range; i < data.Count - range; i++)
                res[i] = _stdDev(data, i - range, i + range);

            return res;


        }

        private static double _stdDev(List<double> data, int s, int e)
        {
            e = e < 0 ? data.Count : e;

            double res = 0;
            double mean = Mean(data, s, e);

            for(var i = s; i <= e; i++)
                res += Math.Pow(data[i] - mean, 2);

            return Math.Sqrt(res / (e - s));
        }

        public static double FunzioneOrientamento(double z, double y)
        {
            return Math.Atan(y / z);
        }

        public static List<double> RemoveDiscontinuity(List<double> theta)
        {

            for (int i = 1; i < theta.Count; i++) {

                double disperazione;
                if(theta[i] - 0.6895 < 0.001 && theta[i] - 0.6895 > - 0.001)
                {
                    disperazione = theta[i];
                    disperazione = int.MaxValue;

                }

                double height = theta[i] - theta[i - 1];

                if (height > 1.2)
                {
                    theta[i] = theta[i] - Math.PI;

                }
                else if (height < - 1.2)
                {
                    theta[i] = theta[i] + Math.PI;
                }
            }

            return theta;
        }
    }
}
