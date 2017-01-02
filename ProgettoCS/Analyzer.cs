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
            this.data = new SlidingWindow<double>[5];
            for(var i = 0; i < data.Length; i++)
                data[i] = new SlidingWindow<double>();
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
                        if(p.IsLastPacket)
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

            Packet p = null;


            var i = firstWindow ? 0 : window.Size() / 2;
            for(; i < window.Count; i++)
            {
                p = window[i];

                // Modulo accelerometro
                data[0].Add(Functions.Modulus(p.GetAccX(0),
                    p.GetAccY(0), p.GetAccZ(0)));

                // Theta
                data[1].Add(Functions.FunzioneOrientamento(p.GetMagZ(0), p.GetMagY(0)));
            }


            // array pieno, Analizza
            int range = 10;
            int start = firstWindow ? 0 : data[0].Size() / 2 - 2 * range;


            int cacca = firstWindow ? 0 : data[0].Size() / 2 - range;


            Functions.RemoveDiscontinuity(data[1]);
            List<double> tempT = data[1].GetRange(start, data[1].Count - start);
            List<double> smoothedTheta = Functions.Smooth(tempT, range);

            List<double> tempA = data[0].GetRange(start, data[0].Count - start);
            List<double> smoothedAcc = Functions.Smooth(tempA, range);

            /*foreach (var d in smoothedAcc)
            {
                pointsQueue.EnqueueElement(new double[] { d, modAcc[cacca] });
                cacca++;
            }*/

            for(var nomeDiUnaVariabileCheUsoComeContatorePerIterareInUnCicloFor = 0; nomeDiUnaVariabileCheUsoComeContatorePerIterareInUnCicloFor < smoothedTheta.Count; nomeDiUnaVariabileCheUsoComeContatorePerIterareInUnCicloFor++)
            {
                pointsQueue.EnqueueElement(new double[] { smoothedAcc[nomeDiUnaVariabileCheUsoComeContatorePerIterareInUnCicloFor], data[0][cacca], smoothedTheta[nomeDiUnaVariabileCheUsoComeContatorePerIterareInUnCicloFor], data[1][cacca] });
                cacca++;
            }

            for(var unAltroNomeDiVariabilePerIterare = 0; unAltroNomeDiVariabilePerIterare < data.Length; unAltroNomeDiVariabilePerIterare++)
                data[unAltroNomeDiVariabilePerIterare].UpdateWindow();




        }

    }
}
