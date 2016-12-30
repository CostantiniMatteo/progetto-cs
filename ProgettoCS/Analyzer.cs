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
        private bool firstWindow;
        private bool lastWindow;


        public Analyzer(Form f, PacketQueue packetQueue, PointsQueue pointsQueue)
        {
            this.f = f;
            this.window = new SlidingWindow();
            this.packetQueue = packetQueue;
            this.pointsQueue = pointsQueue;
            this.firstWindow = true;

            data = new double[3][];
            for (var i = 0; i < 3; i++)
                data[i] = new double[window.Size()];

        }

        public void Read()
        {
            
            while(!lastWindow)
            {
                while(window.Count < window.Size() && !lastWindow)
                {
                    var p = packetQueue.GetNextElement();

                    if(p != null)
                    {
                        if (p.IsLastPacket)
                            lastWindow = true;
                        else
                            window.Add(p);
                    }
                }
                Analyze();

                firstWindow = false;

                window.UpdateWindow();
            }
        }

        private void Analyze()
        {
            var modAcc = new List<double>(window.Count);
            var theta = new List<double>(window.Count);
            Packet p = null;

            for (var i = 0; i < window.Count; i++)
            {
                p = window.Get(i);
                modAcc.Add(Functions.Modulus(p.GetAccX(0),
                    p.GetAccY(0), p.GetAccZ(0)));
                theta.Add(Functions.FunzioneOrientamento(p.GetAccZ(0), p.GetAccY(0)));
                /*data[0][i] = Functions.Modulus(p.GetAccX(0),
                p.GetAccY(0), p.GetAccZ(0));
                data[1][i] = Functions.Modulus(p.GetMagX(0),
                    p.GetMagY(0), p.GetMagZ(0));
                data[2][i] = Functions.Modulus(p.GetGyrX(0),
                    p.GetGyrY(0), p.GetGyrZ(0));
                */

            }

            List<double> contTheta = Functions.RemoveDiscontinuity(theta);

            // array pieno, Analizza
            int range = 10;
            int start = firstWindow ? 0 : modAcc.Count / 2 - 2 * range;
            int cacca = firstWindow ? 0 : modAcc.Count / 2 - range;
            List<double> temp = modAcc.GetRange(start, modAcc.Count - start);
            List<double> smoothedAcc = Functions.Smooth(temp, range);

            foreach (var d in smoothedAcc)
            {
                pointsQueue.EnqueueElement(new double[] { d, modAcc[cacca] });
                cacca++;
            }


        }

    }
}
