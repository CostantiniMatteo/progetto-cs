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
        private Form f;

        private SlidingWindow<Packet> window;
        private bool firstWindow;
        private bool lastWindow;

        SlidingWindow<double>[] data;


        public Analyzer(Form f, PacketQueue packetQueue, PointsQueue pointsQueue)
        {
            this.f = f;
            this.window = new SlidingWindow<Packet>();
            this.packetQueue = packetQueue;
            this.pointsQueue = pointsQueue;
            this.firstWindow = true;
            this.data = new SlidingWindow<double>[7];
            for (var i = 0; i < data.Length; i++)
                data[i] = new SlidingWindow<double>();
        }

        public void Read()
        {

            while (!lastWindow && !Program.stop)
            {

                while (window.Count < window.Size() && !lastWindow && !Program.stop)
                {
                    var p = packetQueue.GetNextElement();

                    if (p != null)
                    {
                        if (p.IsLastPacket)
                            lastWindow = true;
                        else
                            window.Add(p);
                    }
                }

                if (!Program.stop) // lo so che non ha senso 
                {
                    Analyze();

                    firstWindow = false;

                    window.UpdateWindow();
                }
            }

        }

        private void Analyze()
        {

            Packet p = null;


            var i = firstWindow ? 0 : window.Size() / 2;
            for (; i < window.Count; i++)
            {
                p = window[i];

                // Modulo accelerometro
                data[0].Add(Functions.Modulus(p.GetAccX(0), p.GetAccY(0), p.GetAccZ(0)));

                // Theta
                data[1].Add(Functions.FunzioneOrientamento(p.GetMagZ(0), p.GetMagY(0)));
                // provaColella.Add(Functions.FunzioneOrientamento(p.GetMagZ(0), p.GetMagY(0)));

                // AYaw
                data[2].Add(Functions.Yaw(p.GetQuat(0, 0), p.GetQuat(0, 1), p.GetQuat(0, 2), p.GetQuat(0, 3)));

                // Pitch
                data[3].Add(Functions.Pitch(p.GetQuat(0, 0), p.GetQuat(0, 1), p.GetQuat(0, 2), p.GetQuat(0, 3)));

                // Roll
                data[4].Add(Functions.Roll(p.GetQuat(0, 0), p.GetQuat(0, 1), p.GetQuat(0, 2), p.GetQuat(0, 3)));

                // Asse Y Accelerometro
                data[5].Add(p.GetAccX(0));

                // Accelerazione su piano orizzonatale
                data[6].Add(Functions.Modulus(p.GetAccY(0), p.GetAccZ(0)));
            }

            i = firstWindow ? 0 : window.Size() / 2;

            // array pieno, Analizza
            int range = 10;
            int start = firstWindow ? 0 : data[0].Size() / 2 - 2 * range;


            int aDarioPiaceLungo = firstWindow ? 0 : data[0].Size() / 2 - range;

            Functions.RemoveDiscontinuity(data[1]);
            Functions.RemoveDiscontinuity(data[2]);
            Functions.RemoveDiscontinuity(data[4]);

            List<double> tempT = data[1].GetRange(start, data[1].Count - start);
            List<double> smoothedTheta = Functions.Smooth(tempT, range);

            List<double> tempA = data[0].GetRange(start, data[0].Count - start);
            List<double> smoothedAcc = Functions.Smooth(tempA, range);
            List<double> stdDevAcc = Functions.StdDev(tempA, range);


            List<double> tempY = data[2].GetRange(start, data[2].Count - start);
            List<double> smoothedYaw = Functions.Smooth(tempY, range);

            List<double> tempAccX = data[5].GetRange(start, data[5].Count - start);
            List<double> smoothedAccX = Functions.Smooth(tempAccX, range);
            List<int> lss = Functions.sucaStoLayLaySitSitStand(smoothedAccX);
            List<int> lss2 = Functions.sucaStoLayLaySitSitStand(smoothedAccX);
            Functions.laySitBello(lss);

            List<double> tempAV = data[6].GetRange(start, data[6].Count - start);
            List<double> smoothedAccV = Functions.Smooth(tempA, range);
            List<double> stdDevAccV = Functions.StdDev(tempA, range);
            List<List<double>> tUamaArdere = Functions.deadReckoning(stdDevAccV, smoothedYaw);


            for (var j = 0; j < smoothedTheta.Count; j++)
            {
                pointsQueue.EnqueueElement(new double[] { smoothedYaw[j]/*data[2][aDarioPiaceLungo]*/, lss2[j]/*smoothedAcc[j]*/, /*data[1][peppinoDiCapri]*/ tempAccX[j], /*smoothedTheta[j]lss[j]*/ tUamaArdere[INDICEINCREDIBILE] });
                aDarioPiaceLungo++;
            }

            for (var k = 0; k < data.Length; k++)
                data[k].UpdateWindow();


            

        }

    }
}
