using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgettoCS
{
    public class Analyzer
    {
        private PacketQueue packetQueue;
        private PointsQueue pointsQueue;
        private SlidingWindow window;
        private Form f;
        private double[][] data;
        

        public Analyzer(Form f, PacketQueue packetQueue, PointsQueue pointsQueue)
        {
            this.f = f;
            this.window = new SlidingWindow();
            this.packetQueue = packetQueue;
            this.pointsQueue = pointsQueue;

            data = new double[3][];
            for (var i = 0; i < 3; i++)
                data[i] = new double[window.Size() / 2];

        }

        public void Analize()
        {
            int index = 0;
            while(true)
            {
                while(window.Count < window.Size())
                {
                    var p = packetQueue.GetNextElement();

                    if(p != null)
                    {
                        data[0][index] = Functions.Modulus(p.GetAccX(0),
                            p.GetAccY(0), p.GetAccZ(0));
                        data[1][index] = Functions.Modulus(p.GetMagX(0),
                            p.GetMagY(0), p.GetMagZ(0));
                        data[2][index] = Functions.Modulus(p.GetGyrX(0),
                            p.GetGyrY(0), p.GetGyrZ(0));
                        index++;
                        window.Add(p);

                        if (index == (window.Size() / 2))
                        {
                            // array pieno, Analizza
                            double[] smoothedAcc = Functions.Smooth(data[0], 10);
                            double[] smoothedGyr = Functions.Smooth(data[2], 10);

                            pointsQueue.EnqueueElement(new double[][] { smoothedAcc, data[0] });
                            index = 0;
                        }
                    }
                }

                window.UpdateWindow();
                


            }
        }

    }
}
