using System;
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
        GraphPane accelerometerGraph, gyroscopeGraph, magnetometerGraph;
        private TemplateQueue<string> logQueue;
        private TemplateQueue<List<List<double>>> valQueue;

        public Form()
        {
            InitializeComponent();

            accelerometerGraph = zedGraphControl1.GraphPane;
            gyroscopeGraph = zedGraphControl2.GraphPane;
            magnetometerGraph = zedGraphControl3.GraphPane;

            accelerometerGraph.Title.Text = "Accelerometro";
            gyroscopeGraph.Title.Text = "Giroscopio";
            magnetometerGraph.Title.Text = "Magnetometro";

            accelerometerGraph.XAxis.MajorGrid.IsVisible = true;
            accelerometerGraph.YAxis.MajorGrid.IsVisible = true;

            gyroscopeGraph.XAxis.MajorGrid.IsVisible = true;
            gyroscopeGraph.YAxis.MajorGrid.IsVisible = true;

            magnetometerGraph.XAxis.MajorGrid.IsVisible = true;
            magnetometerGraph.YAxis.MajorGrid.IsVisible = true;

            //myPane.Chart.Fill.Brush = new System.Drawing.SolidBrush(Color.DimGray);
            //Per settare valori massimi e minimi del grafico
            /*myPane.YAxis.Scale.Min = -1;
            myPane.YAxis.Scale.Max = 1;
            myPane.XAxis.Scale.Min = -1;
            myPane.XAxis.Scale.Max = 1;
            myPane.AxisChange();
            zedGraphControl1.Invalidate();*/
            logQueue = new TemplateQueue<string>();
            valQueue = new TemplateQueue<List<List<double>>>();

            listener = new Listener(valQueue, logQueue);

            Thread t1 = new Thread(drawModule);
            Thread t2 = new Thread(listener.parser);
            Thread t3 = new Thread(print);
            t1.Start();
            t2.Start();
            t3.Start();
        }


        public void drawModule()
        {
            // Conterra di volta in volta il pacchetto i-esimo invitato e parsato dal listener.
            List<List<double>> val;

            // Le liste che conterranno i punti delle tre curve da disegnare.
            PointPairList accelerometerPPL = new PointPairList();
            PointPairList gyroscopePPL = new PointPairList();
            PointPairList magnetometerPPL = new PointPairList();

            // Le liste che conterranno il modulo applicato rispettivamente
            // ai deti dell'accelerometro e del giroscopio.
            List<double> modAccelerometer = new List<double>();
            List<double> modGyroscope = new List<double>();

            // Il valore dell'asse X dei grafici (Espresso in unita' di tempo?).
            double x = 0;

            while (true)
            {
                // Provo a leggere dalla coda condivisa un nuovo pacchetto.
                val = valQueue.getNextElement();

                if (val != null)
                {

                    // Per ora ci interessano solo i dati del primo sensore
                    // quindi si accede sempre ad indici nella forma [0][i].
                    // Come da formato i primi tre valori sono relativi
                    // all'accelerometro. I tre successivi al giroscopio.
                    // Viene calcolato il modulo per entrambi e aggiunto
                    // alle due rispettive liste.

                    modAccelerometer.Add(modulus(val[0][0], val[0][1], val[0][2]));
                    modGyroscope.Add(modulus(val[0][3], val[0][4], val[0][5]));

                    // Aggiungo i punti (x, y) appena calcolati
                    accelerometerPPL.Add(x, modAccelerometer[modAccelerometer.Count - 1]);
                    gyroscopePPL.Add(x, modGyroscope[modGyroscope.Count - 1]);

                    // CALCOLO DELLA GIRATA
                    // Per il magnetometro mi interessano solo gli assi Y e Z
                    // che sono rispettivamente in posizione 7 e 8 nel pacchetto
                    // Come da specifiche si trova l'angolo di girata calcolando
                    // l'arcotg del rapporto tra la proiezione del vettore del
                    // magnetometro sull'asse Y e sull'asse Z.
                    double girata = Math.Atan(val[0][7] / val[0][8]);
                    girata = removeDiscontinuity(magnetometerPPL, girata, x);

                    magnetometerPPL.Add(x, girata);

                    // Aggiorno il tempo.
                    x++;

                    // Aggiorno le curve da disegnare.
                    // DOMANDA: Non c'e' un modo per evitare di fare Clear() e
                    // AddCurve() ogni volta?
                    accelerometerGraph.CurveList.Clear();
                    gyroscopeGraph.CurveList.Clear();
                    magnetometerGraph.CurveList.Clear();

                    accelerometerGraph.AddCurve("", accelerometerPPL, Color.Red, SymbolType.None);
                    gyroscopeGraph.AddCurve("", gyroscopePPL, Color.Blue, SymbolType.None);
                    magnetometerGraph.AddCurve("Theta", magnetometerPPL, Color.Green, SymbolType.None);

                    // Costa: Non so cosa fanno. Dovrai spiegarmele Dario.
                    zedGraphControl1.AxisChange();
                    zedGraphControl1.Invalidate();

                    zedGraphControl2.AxisChange();
                    zedGraphControl2.Invalidate();

                    zedGraphControl3.AxisChange();
                    zedGraphControl3.Invalidate();

                    // Costa: Non so cosa fa neanche questa.
                    //zedGraphControl1.Refresh();
                }
            }
        }



        // Per gli amici Rapporto Incrementale.
        // Per ora viene calcolato solo sugli ultimi due punti
        // quindi basterebbero solo i valori delle y.
        private double differenceQuotient(double y1, double x1, double y2, double x2)
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
            if (magnetometerPPL.Count > 0 && magnetometerPPL[magnetometerPPL.Count - 1] != null)
            {
                oldX = magnetometerPPL[magnetometerPPL.Count - 1].X;
                oldY = magnetometerPPL[magnetometerPPL.Count - 1].Y;

                incrRapp = differenceQuotient(oldY, oldX, y, x);

                // Se il rapporto incrementale supera una cerca soglia
                // aggiungo o sottraggo PiGreco all'ultimo valore.
                // Non sono sicuro sulla soglia pari a 3. Credo
                // Pinardi abbia detto Pi/2 o 1.2.
                if(incrRapp > 3)
                {
                    y = y - Math.PI;

                }else if (incrRapp < -3)
                {
                    y = y + Math.PI;
                }
            }

            return y;
        }


        private double modulus(double v1, double v2, double v3)
        {
            return Math.Sqrt(Math.Abs(v1) + Math.Abs(v2) + Math.Abs(v3));
        }


        public void print()
        {
            string logMsg;
            while (true)
            {
                logMsg = logQueue.getNextElement();

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
        }


    }
}
