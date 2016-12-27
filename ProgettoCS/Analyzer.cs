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
                data[i] = new double[window.Size()];

        }

        public void Analize()
        {
            while(true)
            {
                while(window.Count < window.Size())
                {
                    var p = packetQueue.GetNextElement();

                    if(p != null)
                    {
                        data[0][window.Count] = Functions.Modulus(p.GetAccX(0),
                            p.GetAccY(0), p.GetAccZ(0));
                        data[1][window.Count] = Functions.Modulus(p.GetMagX(0),
                            p.GetMagY(0), p.GetMagZ(0));
                        data[2][window.Count] = Functions.Modulus(p.GetGyrX(0),
                            p.GetGyrY(0), p.GetGyrZ(0));

                        double[] odioilmondo = { data[0][window.Count], data[1][window.Count] }; 
                        //pointsQueue.EnqueueElement(odioilmondo);

                        window.Add(p);
                    }
                }

                // Analizza
                double[] smoothed = Functions.Smooth(data[0], 2);
                pointsQueue.EnqueueElement(smoothed);

                window.UpdateWindow();
                


            }
        }

    }
}
