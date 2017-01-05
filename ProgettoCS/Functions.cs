﻿using System;
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

            for (var i = range; i < data.Count - range; i++)
            {
                smoothed.Add(Mean(data, i - range, i + range));
            }

            return smoothed;
        }


        public static double Mean(List<double> data, int s = 0, int e = -1)
        {
            e = e < 0 ? data.Count : e;

            double res = 0;
            for (var i = s; i <= e; i++)
                res += data[i];
            return res / (e - s);
        }


        public static double Modulus(double v1, double v2, double v3)
        {
            return Math.Sqrt(v1 * v1 + v2 * v2 + v3 * v3);
        }


        public static List<double> DifferenceQuotient(List<double> data, double increment = 0.02)
        {
            var res = new List<double>(data.Count - 1);

            for (var i = 1; i < data.Count; i++)
                res.Add(data[i] - data[i - 1] / increment);

            return res;

        }

        public static List<double> StdDev(List<double> data, int range)
        {
            var res = new List<double>(data.Count);
            double mean = Mean(data, 0, data.Count - 1);

            for (var i = range; i < data.Count - range; i++)
                res.Add(_stdDev(data, i - range, i + range));

            return res;


        }

        private static double _stdDev(List<double> data, int s, int e)
        {
            e = e < 0 ? data.Count : e;

            double res = 0;
            double mean = Mean(data, s, e);

            for (var i = s; i <= e; i++)
                res += Math.Pow(data[i] - mean, 2);

            return Math.Sqrt(res / (e - s));
        }

        public static double FunzioneOrientamento(double z, double y)
        {
            return Math.Atan(y / z);
        }

        public static void RemoveDiscontinuity(SlidingWindow<double> data)
        {

            for (int i = 1; i < data.Count; i++)
            {
                double height = data[i] - data[i - 1];

                int alfa = Convert.ToInt32(Math.Abs(height / Math.PI));

                if (height > 2.4)
                {
                    data[i] = data[i] - alfa * Math.PI;

                }
                else if (height < -2.4)
                {
                    data[i] = data[i] + alfa * Math.PI;
                }
            }

        }

        public static double Yaw(double q0, double q1, double q2, double q3)
        {
            return Math.Atan((2 * q1 * q2 + 2 * q0 * q3) / (q0 * q0 * 2 + 2 * q1 * q1 - 1));
        }

        public static double Pitch(double q0, double q1, double q2, double q3)
        {
            return -Math.Asin(2 * q1 * q3 - 2 * q0 * q2);
        }

        public static double Roll(double q0, double q1, double q2, double q3)
        {
            return Math.Atan((2 * q2 * q3 + 2 * q0 * q1) / (2 * q0 * q0 + 2 * q3 * q3 - 1));
        }
    }
}
