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
            Thread t3 = new Thread(stampa);
            t1.Start();
            t2.Start();
            t3.Start();
        }


        public void draw()
        {
            List<List<double>> val;
            PointPairList accelerometerPPL = new PointPairList();
            PointPairList gyroscopePPL = new PointPairList();
            PointPairList magnetometerPPL = new PointPairList();

            while (true)
            {
                val = valQueue.getNextElement();
                if (val != null)
                {
                    int size = val.Count();
                    for (int i = 1; i < size; i++)
                    {
                        accelerometerPPL.Add(val[i][0], val[i][1]);
                        gyroscopePPL.Add(val[i][3], val[i][4]);
                        magnetometerPPL.Add(val[i][6], val[i][7]);
                    }

                    zedGraphControl1.GraphPane.CurveList.Clear();
                    zedGraphControl2.GraphPane.CurveList.Clear();
                    zedGraphControl3.GraphPane.CurveList.Clear();

                    LineItem accelerometerCurve = accelerometerGraph.AddCurve("", accelerometerPPL, Color.Red, SymbolType.None);
                    LineItem gyroscopeCurve = gyroscopeGraph.AddCurve("", gyroscopePPL, Color.Blue, SymbolType.None);
                    LineItem magnetometerCurve = magnetometerGraph.AddCurve("", magnetometerPPL, Color.Green, SymbolType.None);

                    zedGraphControl1.AxisChange();
                    zedGraphControl1.Invalidate();

                    zedGraphControl2.AxisChange();
                    zedGraphControl2.Invalidate();

                    zedGraphControl3.AxisChange();
                    zedGraphControl3.Invalidate();

                    //zedGraphControl1.Refresh();
                }
            }
        }


        public void drawModule()
        {
            List<List<double>> val;
            PointPairList accelerometerPPL = new PointPairList();
            PointPairList gyroscopePPL = new PointPairList();
            PointPairList magnetometerPPL = new PointPairList();

            List<double> modacc = new List<double>();
            List<double> modgyr = new List<double>();
            double x = 0;

            while (true)
            {
                val = valQueue.getNextElement();
                if (val != null)
                {

                    modacc.Add(modulo(val[0][0], val[0][1], val[0][2]));
                    modgyr.Add(modulo(val[0][3], val[0][4], val[0][5]));

                    accelerometerPPL.Add(x, modacc[modacc.Count - 1]);
                    gyroscopePPL.Add(x, modgyr[modgyr.Count - 1]);

                    double y = Math.Atan(val[0][7] / val[0][8]);
                    y = deleteDiscontinuity(magnetometerPPL, y, x);


                    magnetometerPPL.Add(x, y);
                    x++;

                    accelerometerGraph.CurveList.Clear();
                    gyroscopeGraph.CurveList.Clear();
                    magnetometerGraph.CurveList.Clear();

                    accelerometerGraph.AddCurve("", accelerometerPPL, Color.Red, SymbolType.None);
                    gyroscopeGraph.AddCurve("", gyroscopePPL, Color.Blue, SymbolType.None);
                    magnetometerGraph.AddCurve("Theta", magnetometerPPL, Color.Green, SymbolType.None);

                    zedGraphControl1.AxisChange();
                    zedGraphControl1.Invalidate();

                    zedGraphControl2.AxisChange();
                    zedGraphControl2.Invalidate();

                    zedGraphControl3.AxisChange();
                    zedGraphControl3.Invalidate();

                    //zedGraphControl1.Refresh();
                }
            }
        }


        private double incremRapp(double y1, double x1, double y2, double x2)
        {
            return (y2 - y1) / (x2 - x1);
        }


        private double deleteDiscontinuity(PointPairList pointPl3, double y, double x)
        {
            double oldY, oldX;
            double incrRapp;
            if (pointPl3.Count > 0 && pointPl3[pointPl3.Count - 1] != null)
            {
                oldX = pointPl3[pointPl3.Count - 1].X;
                oldY = pointPl3[pointPl3.Count - 1].Y;

                incrRapp = incremRapp(oldY, oldX, y, x);

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


        private double modulo(double v1, double v2, double v3)
        {
            return Math.Sqrt(Math.Abs(v1) + Math.Abs(v2) + Math.Abs(v3));
        }


        public void stampa()
        {
            string stringa;
            while (true)
            {
                stringa = logQueue.getNextElement();

                if (stringa != null)
                    try
                    {
                        this.Invoke((MethodInvoker)delegate ()
                        {
                            richTextBox1.Text += stringa;
                        });
                    }
                    catch (Exception e) { }
                Thread.Sleep(100);
            }
        }


    }
}
