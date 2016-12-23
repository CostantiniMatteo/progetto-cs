﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;

namespace ProgettoCS
{
    public partial class Form : System.Windows.Forms.Form
    {

        private Listener listener;
        GraphPane accelerometerGraph, gyroscopeGraph, magnetometerGraph, pane4;
        private ConcurrentQueue<string> logQueue;
        private PacketQueue valQueue;

        public Form()
        {
            InitializeComponent();
            Resize();

            accelerometerGraph = zedGraphControl1.GraphPane;
            gyroscopeGraph = zedGraphControl2.GraphPane;
            magnetometerGraph = zedGraphControl3.GraphPane;
            pane4 = zedGraphControl4.GraphPane;

            accelerometerGraph.YAxis.Title.Text = "m/s²";
            accelerometerGraph.XAxis.Title.Text = "second";

            gyroscopeGraph.YAxis.Title.Text = "Grade";
            gyroscopeGraph.XAxis.Title.Text = "second";

            magnetometerGraph.YAxis.Title.Text = "π";
            magnetometerGraph.XAxis.Title.Text = "second";

            pane4.YAxis.Title.Text = "π";
            pane4.XAxis.Title.Text = "second";

            accelerometerGraph.IsFontsScaled = false;
            gyroscopeGraph.IsFontsScaled = false;
            magnetometerGraph.IsFontsScaled = false;
            pane4.IsFontsScaled = false;

            accelerometerGraph.Title.Text = "Accelerometro";
            gyroscopeGraph.Title.Text = "Giroscopio";
            magnetometerGraph.Title.Text = "Magnetometro senza discontinuità";
            pane4.Title.Text = "Magnetometro con discontinuità";

            accelerometerGraph.XAxis.MajorGrid.IsVisible = true;
            accelerometerGraph.YAxis.MajorGrid.IsVisible = true;

            gyroscopeGraph.XAxis.MajorGrid.IsVisible = true;
            gyroscopeGraph.YAxis.MajorGrid.IsVisible = true;

            magnetometerGraph.XAxis.MajorGrid.IsVisible = true;
            magnetometerGraph.YAxis.MajorGrid.IsVisible = true;

            pane4.XAxis.MajorGrid.IsVisible = true;
            pane4.YAxis.MajorGrid.IsVisible = true;

            //myPane.Chart.Fill.Brush = new System.Drawing.SolidBrush(Color.DimGray);
            //Per settare valori massimi e minimi del grafico
            /*myPane.YAxis.Scale.Min = -1;
            myPane.YAxis.Scale.Max = 1;
            myPane.XAxis.Scale.Min = -1;
            myPane.XAxis.Scale.Max = 1;
            myPane.AxisChange();
            zedGraphControl1.Invalidate();*/
            logQueue = new ConcurrentQueue<string>();
            valQueue = new PacketQueue();

            listener = new Listener(valQueue, logQueue);

            Thread t1 = new Thread(DrawModule);
            Thread t2 = new Thread(listener.Parse);
            //Thread t3 = new Thread(Print);
            t1.Start();
            t2.Start();
            //t3.Start();
        }

        private void Resize()
        {
            this.Location = new Point(0, 0);
            this.Size = Screen.PrimaryScreen.WorkingArea.Size;
            tableLayoutPanel1.Size = new Size(this.Width - 250, this.Height - 63);
            zedGraphControl1.Size = new Size(tableLayoutPanel1.Size.Width / 2, tableLayoutPanel1.Size.Height / 2);
            zedGraphControl2.Size = new Size(tableLayoutPanel1.Size.Width / 2, tableLayoutPanel1.Size.Height / 2);
            zedGraphControl3.Size = new Size(tableLayoutPanel1.Size.Width / 2, tableLayoutPanel1.Size.Height / 2);
            zedGraphControl4.Size = new Size(tableLayoutPanel1.Size.Width / 2, tableLayoutPanel1.Size.Height / 2);       

        }

        public void DrawModule()
        {
            // Conterra di volta in volta il pacchetto i-esimo invitato e parsato dal listener.
            Packet val;

            // Le liste che conterranno i punti delle tre curve da disegnare.
            PointPairList accelerometerPPL = new PointPairList();
            PointPairList gyroscopePPL = new PointPairList();
            PointPairList magnetometerPPL = new PointPairList();
            PointPairList magnetometerPPLDis = new PointPairList();
            PointPairList smoothed = new PointPairList();

            // Le liste che conterranno il modulo applicato rispettivamente
            // ai deti dell'accelerometro e del giroscopio.
            List<double> modAccelerometer = new List<double>();
            List<double> modGyroscope = new List<double>();
            
            // Il valore dell'asse X dei grafici (Espresso in unita' di tempo?).
            double x = 0;
            int range = 10;

            while (true)
            {
                // Provo a leggere dalla coda condivisa un nuovo pacchetto.
                val = valQueue.GetNextElement();

                if (val != null)
                {

                    // Per ora ci interessano solo i dati del primo sensore
                    // quindi si accede sempre ad indici nella forma [0][i].
                    // Come da formato i primi tre valori sono relativi
                    // all'accelerometro. I tre successivi al giroscopio.
                    // Viene calcolato il modulo per entrambi e aggiunto
                    // alle due rispettive liste.

                    modAccelerometer.Add(Modulus(val.GetAccX(0), val.GetAccY(0), val.GetAccZ(0)));
                    modGyroscope.Add(Modulus(val.GetGyrX(0), val.GetGyrY(0), val.GetGyrZ(0)));

                    // Aggiungo i punti (x, y) appena calcolati
                    accelerometerPPL.Add(x, modAccelerometer[modAccelerometer.Count - 1]);
                    gyroscopePPL.Add(x, modGyroscope[modGyroscope.Count - 1]);

                    // CALCOLO DELLA GIRATA
                    // Per il magnetometro mi interessano solo gli assi Y e Z
                    // che sono rispettivamente in posizione 7 e 8 nel pacchetto
                    // Come da specifiche si trova l'angolo di girata calcolando
                    // l'arcotg del rapporto tra la proiezione del vettore del
                    // magnetometro sull'asse Y e sull'asse Z.
                    double girata = Math.Atan(val.GetMagY(0) / val.GetMagZ(0));
                    magnetometerPPLDis.Add(x, girata);
                    girata = removeDiscontinuity(magnetometerPPL, girata, x);

                    magnetometerPPL.Add(x, girata);

                    // si passa smoothed e si modifica direttamente quella
                    // oppure si fa ritornare una lista di valori mediati
                    // che si assegna a smoothed
                    smoothing(smoothed, magnetometerPPL, range);

                    // Aggiorno il tempo.
                    x = x + 0.02;

                    // Aggiorno le curve da disegnare.
                    // DOMANDA: Non c'e' un modo per evitare di fare Clear() e
                    // AddCurve() ogni volta?
                    accelerometerGraph.CurveList.Clear();
                    gyroscopeGraph.CurveList.Clear();
                    magnetometerGraph.CurveList.Clear();
                    pane4.CurveList.Clear();

                    accelerometerGraph.AddCurve("", accelerometerPPL, Color.Red, SymbolType.None);
                    gyroscopeGraph.AddCurve("", gyroscopePPL, Color.Blue, SymbolType.None);
                    magnetometerGraph.AddCurve("Theta", magnetometerPPL, Color.Green, SymbolType.None);
                    pane4.AddCurve("Theta", magnetometerPPLDis, Color.DeepPink, SymbolType.None);

                    // Costa: Non so cosa fanno. Dovrai spiegarmele Dario.
                    zedGraphControl1.AxisChange();
                    zedGraphControl1.Invalidate();

                    zedGraphControl2.AxisChange();
                    zedGraphControl2.Invalidate();

                    zedGraphControl3.AxisChange();
                    zedGraphControl3.Invalidate();

                    zedGraphControl4.AxisChange();
                    zedGraphControl4.Invalidate();

                    // Costa: Non so cosa fa neanche questa.
                    //zedGraphControl1.Refresh();
                }
            }
        }

        private void smoothing(PointPairList smoothed, PointPairList magnetometerPPL, int range)
        {
            if(magnetometerPPL != null && magnetometerPPL.Count >= (range*2 + 1))
            {
                
            }

        }



        // Per gli amici Rapporto Incrementale.
        // Per ora viene calcolato solo sugli ultimi due punti
        // quindi basterebbero solo i valori delle y.
        private double DifferenceQuotient(double y1, double x1, double y2, double x2)
        {
            return (y2 - y1) / (x2 - x1);
        }

        // Rimuove la discontinuita', se presente, nell'ultimo valore
        // del magnetometro.
        private double removeDiscontinuity(PointPairList magnetometerPPL, double y, double x)
        {
            double oldY, oldX;
            double incrRapp;

            // Costa: Non mi sono chiari i controlli:
            // Come fa ad essere null l'ultimo valore nella PPL?
            if (magnetometerPPL.Count > 0 )
            {
                oldX = magnetometerPPL[magnetometerPPL.Count - 1].X;
                oldY = magnetometerPPL[magnetometerPPL.Count - 1].Y;

                incrRapp = DifferenceQuotient(oldY, oldX, y, x);

                // Se il rapporto incrementale supera una cerca soglia
                // aggiungo o sottraggo PiGreco all'ultimo valore.
                // Non sono sicuro sulla soglia pari a 3. Credo
                // Pinardi abbia detto Pi/2 o 1.2.

                // Dario: si esatto è giusto Pi/2 come "limite", ma infatti
                // la funzione fa lo scalino quando passa da Pi/2 a -Pi/2 
                // (e viceversa) e quindi fa un salto di Pi. Ho messo 3
                // semplicemente perchè non sono sempre precisissimi i calcoli
                // quindi se mettessi 3.14 magari potrei avere uno scalino 
                // di 3.13 e l'algoritmo non lo beccherebbe.

                // Per Costa: prova ad aprire il file "camm svolta" e guarda
                // gli scalini che fa, poi sostituisci negli if qui sotto
                // 2.4 al posto 3 e riapri il file, dovrebbe sistemarli.

                // ho messo le x reali, quindi ogni x si incrementa di 0,02
                // quindi rapporto diventa 2,4/0,02

                if(incrRapp > 120)
                {
                    y = y - Math.PI;

                }else if (incrRapp < -120)
                {
                    y = y + Math.PI;
                }
            }

            return y;
        }


        private double Modulus(double v1, double v2, double v3)
        {
            return Math.Sqrt(Math.Abs(v1) + Math.Abs(v2) + Math.Abs(v3));
        }


        /*public void Print()
        {
            string logMsg;
            while (true)
            {
                logMsg = logQueue.GetNextElement();

                if (logMsg != null)
                    try
                    {
                        this.Invoke((MethodInvoker)delegate ()
                        {
                            richTextBox1.Text += logMsg;
                        });
                    }
                    catch (Exception) { }
                Thread.Sleep(100);
            }

            
        }*/


    }
}
