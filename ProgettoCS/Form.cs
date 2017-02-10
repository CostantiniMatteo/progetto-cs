using System;
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

        private GraphPane accelerometerGraph, gyroscopeGraph, thetaGraph, laySitGraph,
            accelerometerXYGraph, yawGraph, stdDevGraph, deadReckoningGraph;
        private PointsQueue pointsQueue;
        private IEnumerable<Control> zedList;
        private IEnumerable<Control> groupBoxList;
        private int lastLss;
        private int groupBoxCount;
        private double lastTime;
        //StreamWriter writer = new StreamWriter("../output.csv");

        public Form(PointsQueue pq)
        {
            zedList = GetAll(this, typeof(ZedGraphControl));
            zedList = zedList.OrderBy(ZedGraphControl => ZedGraphControl.Name);

            File.Delete(".../output.csv");

            InitializeComponent();
            Resize();
            groupBoxCount = 3;
            lastTime = 0;

            accelerometerGraph = zedGraphControl1.GraphPane;
            gyroscopeGraph = zedGraphControl2.GraphPane;
            thetaGraph = zedGraphControl3.GraphPane;
            laySitGraph = zedGraphControl4.GraphPane;
            accelerometerXYGraph = zedGraphControl5.GraphPane;
            yawGraph = zedGraphControl6.GraphPane;
            stdDevGraph = zedGraphControl7.GraphPane;
            deadReckoningGraph = zedGraphControl8.GraphPane;

            accelerometerGraph.YAxis.Title.Text = "m/s²";
            accelerometerGraph.XAxis.Title.Text = "second";

            gyroscopeGraph.YAxis.Title.Text = "rad/s";
            gyroscopeGraph.XAxis.Title.Text = "second";

            thetaGraph.YAxis.Title.Text = "radiants";
            thetaGraph.XAxis.Title.Text = "second";

            laySitGraph.YAxis.Title.Text = "State";
            laySitGraph.XAxis.Title.Text = "second";

            accelerometerXYGraph.YAxis.Title.Text = "m/s²";
            accelerometerXYGraph.XAxis.Title.Text = "second";

            yawGraph.YAxis.Title.Text = "radiants";
            yawGraph.XAxis.Title.Text = "second";

            stdDevGraph.YAxis.Title.Text = "m/s²";
            stdDevGraph.XAxis.Title.Text = "second";

            deadReckoningGraph.YAxis.Title.Text = "m";
            deadReckoningGraph.XAxis.Title.Text = "m";


            accelerometerGraph.Title.Text = "Modulo accelerometro smoothed";
            gyroscopeGraph.Title.Text = "Modulo giroscopio smoothed";
            thetaGraph.Title.Text = "Angolo theta";
            laySitGraph.Title.Text = "Lay Sit Stand";
            accelerometerXYGraph.Title.Text = "Modulo accelerometro smoothed sul piano XY";
            yawGraph.Title.Text = "Angolo Yaw";
            stdDevGraph.Title.Text = "Deviazione standard del modulo dell'accelerazione";
            deadReckoningGraph.Title.Text = "Dead Reckoning";

            for (int i = 0; i < zedList.Count(); i++)
            {
                ZedGraphControl z = (ZedGraphControl)zedList.ElementAt(i);
                GraphPane myPane = z.GraphPane;

                myPane.IsFontsScaled = false;
                myPane.XAxis.MajorGrid.IsVisible = true;
                myPane.YAxis.MajorGrid.IsVisible = true;
            }

            this.pointsQueue = pq;
        }


#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        private void Resize()
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
        {
            this.Location = new Point(0, 0);
            this.Size = Screen.PrimaryScreen.WorkingArea.Size;
            this.WindowState = FormWindowState.Maximized;
            tabControl1.Size = new Size(this.Width - 250, this.Height - 63);
            tableLayoutPanel1.Size = new Size(tabControl1.Size.Width - 8, tabControl1.Size.Height - 13);
            tableLayoutPanel2.Size = new Size(tabControl1.Size.Width - 8, tabControl1.Size.Height - 13);

            for (int i = 0; i < zedList.Count(); i++)
            {
                zedList.ElementAt(i).Size = new Size(tableLayoutPanel1.Size.Width / 2, tableLayoutPanel1.Size.Height / 2);
            }

        }




        public void Draw()
        {
            Color c = Color.Red;
            double x = 0;
            lastTime = 0;

            while (!Program.stop)
            {
                double[] points = pointsQueue.GetNextElement();

                if (points != null && !Program.stop)
                {
                    for (int i = 0; i < points.Length - 3; i++)
                    {
                        UpgradeGraph((ZedGraphControl)zedList.ElementAt(i), x, points[i], c);
                    }

                    generateGroupBox(points[7], x);

                    lastLss = (int)points[7];

                    UpgradeGraph((ZedGraphControl)zedList.ElementAt(7), points[8], points[9], c);


                    Thread.Sleep(15);
                    x += 0.02;
                }
            }
        }

        private void generateGroupBox(double p, double x)
        {
            if (p != lastLss && lastTime != x )
            {
                GroupBox gbx = new GroupBox();
                PictureBox pic = new PictureBox();
                System.Windows.Forms.Label l1, l2;

                pic.SizeMode = PictureBoxSizeMode.StretchImage;
                pic.Image = Image.FromFile("../Pics/stand.bmp");
                pic.Location = new System.Drawing.Point(6, 17);
                pic.Size = new System.Drawing.Size(90, 90);
                pic.TabIndex = 1;
                pic.TabStop = false;

                string action = "";
                double from = Math.Round(lastTime, 2);
                double to = Math.Round(x, 2);

                switch (lastLss)
                {
                    case 0:
                        pic.Image = Image.FromFile("../Pics/lay.jpg");
                        action = "lay";
                        break;
                    case 1:
                        pic.Image = Image.FromFile("../Pics/laysit.bmp");
                        action = "lay/sit";
                        break;
                    case 2:
                        pic.Image = Image.FromFile("../Pics/sit.bmp");
                        action = "sit";
                        break;
                    case 3:
                        pic.Image = Image.FromFile("../Pics/stand.bmp");
                        action = "stand";
                        break;
                }

                l2 = new System.Windows.Forms.Label();
                l1 = new System.Windows.Forms.Label();

                l1.Location = new System.Drawing.Point(102, 17);
                l1.TabIndex = 0;
                l1.Text = "from: "+ from + " sec";

                l2.Location = new System.Drawing.Point(102, 61);
                l2.TabIndex = 2;
                l2.Text = "to: " + to + " sec";

                UpdateFile(action, from, to);

                gbx.Controls.Add(l1);
                gbx.Controls.Add(l2);
                gbx.Controls.Add(pic);
                gbx.Size = new System.Drawing.Size(185, 111);
                gbx.TabIndex = 2;
                gbx.TabStop = false;
                gbx.Name = "groupBox" + (groupBoxCount + 1);
                gbx.Visible = true;

                this.Invoke((MethodInvoker)delegate ()
                {
                    flowLayoutPanel1.Controls.Add(gbx);
                    flowLayoutPanel1.Refresh();
                });

                lastTime = x;
                
            }

        }

        private void UpdateFile(string action, double from, double to)
        {
            StreamWriter writer = File.AppendText("../output.csv");
            writer.WriteLine(action + ";" + from + ";" + to);
            writer.Close();
        }

        private void UpgradeGraph(ZedGraphControl z, double x, double y, Color c)
        {
            GraphPane pane = z.GraphPane;

            if (pane.CurveList.Count == 0)
            {
                pane.AddCurve("", new double[] { x }, new double[] { y }, c, SymbolType.None);
            }
            else
            {
                CurveItem curve = pane.CurveList[pane.CurveList.Count - 1];

                if (curve.Color == c)
                {
                    curve.AddPoint(x, y);
                }
                else
                {
                    PointPairList ppl = new PointPairList();
                    ppl.Add(curve.Points[curve.NPts - 1].X, curve.Points[curve.NPts - 1].Y);
                    ppl.Add(x, y);
                    pane.AddCurve("", ppl, c, SymbolType.None);
                }
            }
            z.AxisChange();
            z.Invalidate();
        }

        private void removeAllGraphs()
        {

            for (int i = 0; i < zedList.Count(); i++)
            {
                ZedGraphControl z = (ZedGraphControl)zedList.ElementAt(i);
                z.GraphPane.CurveList.Clear();
                z.AxisChange();
                z.Invalidate();
            }

            groupBoxList = GetAll(this, typeof(GroupBox));
            groupBoxList = groupBoxList.OrderBy(GroupBox => GroupBox.Name);

            flowLayoutPanel1.Controls.Clear();

        }

        //EVENTI

        private void Form_Load(object sender, EventArgs e)
        {
            comboBox1.DisplayMember = "Name";

            for (int i = 0; i < zedList.Count(); i++)
            {
                comboBox1.Items.Add(zedList.ElementAt(i));
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;

            Program.StopThreads();

            removeAllGraphs();

            Program.StartThreads();

            button2.Enabled = true;
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Enabled = true;
            textBox2.Enabled = true;
            textBox3.Enabled = true;
            textBox4.Enabled = true;
        }

        private void zedGraphControl2_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && textBox2.Text != "" && textBox3.Text != "" && textBox4.Text != "")
            {
                double minX = Convert.ToDouble(textBox1.Text);
                double maxX = Convert.ToDouble(textBox2.Text);
                double minY = Convert.ToDouble(textBox3.Text);
                double maxY = Convert.ToDouble(textBox4.Text);

                if (minX < maxX || minY < maxY)
                {
                    for (int i = 0; i < zedList.Count(); i++)
                    {
                        ZedGraphControl zed = (ZedGraphControl)comboBox1.SelectedItem;

                        GraphPane myPane = zed.GraphPane;
                        myPane.XAxis.Scale.Min = minX;
                        myPane.XAxis.Scale.Max = maxX;
                        myPane.YAxis.Scale.Min = minY;
                        myPane.YAxis.Scale.Max = maxY;

                        myPane.AxisChange();
                        zed.Invalidate();

                    }
                }
                else
                {
                    MessageBox.Show("Invalid numbers");
                }
            }
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(textBox1.Text, "[^0-9]"))
            {
                ((TextBox)sender).Text = "";
                MessageBox.Show("Please enter only numbers.");
            }
        }

        public IEnumerable<Control> GetAll(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAll(ctrl, type))
                                      .Concat(controls)
                                      .Where(c => c.GetType() == type);
        }


        /*
        public void DrawModule()
        {
       
            
            // Conterra di volta in volta il pacchetto i-esimo invitato e parsato dal listener.
            Packet val;

            // Le liste che conterranno i punti delle tre curve da disegnare.
            var accelerometerPPL = new PointPairList();
            var gyroscopePPL = new PointPairList();
            var magnetometerPPL = new PointPairList();
            var magnetometerDiscPPL = new PointPairList();
            var smoothed = new PointPairList();

            // Le liste che conterranno il modulo applicato rispettivamente
            // ai deti dell'accelerometro e del giroscopio.
            var modAccelerometer = new List<double>();
            var modGyroscope = new List<double>();
            
            // Il valore dell'asse X dei grafici.
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
                    magnetometerDiscPPL.Add(x, girata);
                    girata = RemoveDiscontinuity(magnetometerPPL, girata, x);

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
                    magnetometerDiscGraph.CurveList.Clear();

                    accelerometerGraph.AddCurve("", accelerometerPPL, Color.Red, SymbolType.None);
                    gyroscopeGraph.AddCurve("", gyroscopePPL, Color.Blue, SymbolType.None);
                    magnetometerGraph.AddCurve("Theta", magnetometerPPL, Color.Green, SymbolType.None);
                    magnetometerDiscGraph.AddCurve("Theta", magnetometerDiscPPL, Color.DeepPink, SymbolType.None);

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

        */

        /*
        // Rimuove la discontinuita', se presente, nell'ultimo valore
        // del magnetometro.
        private double RemoveDiscontinuity(PointPairList magnetometerPPL, double y, double x)
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
        */

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
