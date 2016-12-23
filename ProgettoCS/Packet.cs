using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgettoCS
{
    class Packet
    {
        private List<List<double>> data;

        public Packet(List<List<double>> data)
        {
            this.data = data;
        }

        public double GetAccX(int sensor)
        {
            return data[sensor][0];
        }

        public double GetAccY(int sensor)
        {
            return data[sensor][1];
        }

        public double GetAccZ(int sensor)
        {
            return data[sensor][2];
        }

        public double GetGyrX(int sensor)
        {
            return data[sensor][3];
        }

        public double GetGyrY(int sensor)
        {
            return data[sensor][4];
        }

        public double GetGyrZ(int sensor)
        {
            return data[sensor][5];
        }

        public double GetMagX(int sensor)
        {
            return data[sensor][6];
        }

        public double GetMagY(int sensor)
        {
            return data[sensor][7];
        }

        public double GetMagZ(int sensor)
        {
            return data[sensor][8];
        }

        public double GetQuat(int sensor, int quaternion)
        {
            return data[sensor][quaternion + 9];
        }

    }
}
