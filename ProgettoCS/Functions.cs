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
            return res / (e - s + 1);
        }


        public static double Modulus(double v1, double v2, double v3 = 0)
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

        public static void RadiansToDegrees(SlidingWindow<double> data, int s = 0, int e = -1)
        {
            e = e < 0 ? data.Count : e;

            for (int i = s; i < e; i++)
            {
                data[i] = data[i] * 180 / Math.PI;
            }
        }

        public static double FunzioneOrientamento(double z, double y)
        {
            return Math.Atan2(y, z);
        }
        
        public static void RemoveDiscontinuity(SlidingWindow<double> data)
        {

            for (int i = 1; i < data.Count; i++)
            {
                double height = data[i] - data[i - 1];

                int alfa = Convert.ToInt32(Math.Abs(height / Math.PI));

                if (height > 1.9)
                {
                    data[i] = data[i] - alfa * Math.PI;

                }
                else if (height < -1.9)
                {
                    data[i] = data[i] + alfa * Math.PI;
                }
            }

        }


        // x < 2.7 : lay; 2.7 <= x < 3.7 : lay/sit; 3.7 <= x < 7 : sit; x >= 7 : stand; 
        // 0 : lay
        // 1 : lay/sit
        // 2 : sit
        // 3 : stand
        public static List<int> sucaStoLayLaySitSitStand(List<double> data)
        {
            var lss = new List<int>(data.Count);

            for (int i = 0; i < data.Count; i++)
            {
                if (data[i] < 2.9)
                {
                    lss.Add(0);
                } else if (data[i] < 3.7 && data[i] >= 2.9)
                {
                    lss.Add(1);
                } else if (data[i] < 8.2 && data[i] >= 3.7)
                {
                    lss.Add(2);
                } else
                {
                    lss.Add(3);
                }
            }

            return lss;
        }

        public static void laySitBello(List<int> data, int window = 20)
        {
            for (var i = window + 1; i < data.Count - window; i++)
            {
                if (data[i - window] == data[i + window] && data[i] != data[i - window])
                {
                    for (var j = i - window; j < i + window; j++)
                    {
                        data[j] = data[i - window];
                    }
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

        public static double motoStazionario(double devStd)
        {
            int sogliola = 4;
            int sogliola2 = 5;

            if (devStd < sogliola) return 0;
            else if (devStd < sogliola2) return 3; // m/s Cammino
            else return 6; // m/s Corsa
        }

        public static List<List<double>> deadReckoning(List<double> devStd, List<double> ayaws, double lastX, double lastY)
        {
            var result = new List<List<double>>(devStd.Count);

            result.Add(new List<double>());
            result.Add(new List<double>());

            result[0].Add(lastX);
            result[1].Add(lastY); 

            for (var i = 0; i < devStd.Count; i++)
            {
                double velocita = motoStazionario(i);
                double spostamento = velocita * 0.02; //secondi

                result[0].Add(result[0][i] + spostamento * Math.Cos(ayaws[i]));
                result[1].Add(result[1][i] + spostamento * Math.Sin(ayaws[i]));
            }

            return result;
           
        }
    }
}
