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
    public partial class Form1 : Form
    {

        private Listener l1;
        GraphPane myPane, myPane2, myPane3;
        private TemplateQueue<string> stringQueue;
        private TemplateQueue<List<List<double>>> valQueue;

        public Form1()
        {
            InitializeComponent();
            myPane = zedGraphControl1.GraphPane;
            myPane2 = zedGraphControl2.GraphPane;
            myPane3 = zedGraphControl3.GraphPane;
            myPane.Title.Text = "Accelerometro";
            myPane2.Title.Text = "Giroscopio";
            myPane3.Title.Text = "Magnetometro";
            myPane.XAxis.MajorGrid.IsVisible = true;
            myPane.YAxis.MajorGrid.IsVisible = true;
            myPane2.XAxis.MajorGrid.IsVisible = true;
            myPane2.YAxis.MajorGrid.IsVisible = true;
            myPane3.XAxis.MajorGrid.IsVisible = true;
            myPane3.YAxis.MajorGrid.IsVisible = true;

            //myPane.Chart.Fill.Brush = new System.Drawing.SolidBrush(Color.DimGray);
            //Per settare valori massimi e minimi del grafico
            /*myPane.YAxis.Scale.Min = -1;
            myPane.YAxis.Scale.Max = 1;
            myPane.XAxis.Scale.Min = -1;
            myPane.XAxis.Scale.Max = 1;
            myPane.AxisChange();
            zedGraphControl1.Invalidate();*/
            stringQueue = new TemplateQueue<string>();
            valQueue = new TemplateQueue<List<List<double>>>();

            l1 = new Listener(valQueue, stringQueue);

            Thread t1 = new Thread(drawModule);
            Thread t2 = new Thread(l1.parser);
            Thread t3 = new Thread(stampa);
            t1.Start();
            t2.Start();
            t3.Start();
        }

        public void draw()
        {
            List<List<double>> val;
            PointPairList pointPl = new PointPairList();
            PointPairList pointPl2 = new PointPairList();
            PointPairList pointPl3 = new PointPairList();

            while (true)
            {
                val = valQueue.getNextElement();
                if (val != null)
                {
                    int size = val.Count();
                    for (int i = 1; i < size; i++)
                    {
                        pointPl.Add(val[i][0], val[i][1]);
                        pointPl2.Add(val[i][3], val[i][4]);
                        pointPl3.Add(val[i][6], val[i][7]);
                    }
                    zedGraphControl1.GraphPane.CurveList.Clear();
                    zedGraphControl2.GraphPane.CurveList.Clear();
                    zedGraphControl3.GraphPane.CurveList.Clear();
                    LineItem myCurve = myPane.AddCurve("", pointPl, Color.Red, SymbolType.None);
                    LineItem myCurve2 = myPane2.AddCurve("", pointPl2, Color.Blue, SymbolType.None);
                    LineItem myCurve3 = myPane3.AddCurve("", pointPl3, Color.Green, SymbolType.None);

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
            PointPairList pointPl = new PointPairList();
            PointPairList pointPl2 = new PointPairList();
            PointPairList pointPl3 = new PointPairList();
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

                    pointPl.Add(x, modacc[modacc.Count - 1]);
                    pointPl2.Add(x, modgyr[modgyr.Count - 1]);

                    double y = Math.Atan(val[0][7] / val[0][8]);
                    y = deleteDiscontinuity(pointPl3, y, x);


                    pointPl3.Add(x, y);
                    x++;

                    myPane.CurveList.Clear();
                    myPane2.CurveList.Clear();
                    myPane3.CurveList.Clear();

                    myPane.AddCurve("", pointPl, Color.Red, SymbolType.None);
                    myPane2.AddCurve("", pointPl2, Color.Blue, SymbolType.None);
                    myPane3.AddCurve("Theta", pointPl3, Color.Green, SymbolType.None);

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
                stringa = stringQueue.getNextElement();

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
