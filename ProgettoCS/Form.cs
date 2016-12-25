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

        private Listener listener;
        GraphPane accelerometerGraph, gyroscopeGraph, magnetometerGraph, magnetometerDiscGraph;
        private ConcurrentQueue<string> logQueue;
        private PacketQueue valQueue;
        private IEnumerable<Control> c;

        public Form()
        {
            InitializeComponent();
            Resize();
            c = GetAll(this, typeof(ZedGraphControl));

            accelerometerGraph = zedGraphControl1.GraphPane;
            gyroscopeGraph = zedGraphControl2.GraphPane;
            magnetometerGraph = zedGraphControl3.GraphPane;
            magnetometerDiscGraph = zedGraphControl4.GraphPane;

            accelerometerGraph.YAxis.Title.Text = "m/s²";
            accelerometerGraph.XAxis.Title.Text = "second";

            gyroscopeGraph.YAxis.Title.Text = "Grade";
            gyroscopeGraph.XAxis.Title.Text = "second";

            magnetometerGraph.YAxis.Title.Text = "π";
            magnetometerGraph.XAxis.Title.Text = "second";

            magnetometerDiscGraph.YAxis.Title.Text = "π";
            magnetometerDiscGraph.XAxis.Title.Text = "second";

            accelerometerGraph.IsFontsScaled = false;
            gyroscopeGraph.IsFontsScaled = false;
            magnetometerGraph.IsFontsScaled = false;
            magnetometerDiscGraph.IsFontsScaled = false;

            accelerometerGraph.Title.Text = "Accelerometro";
            gyroscopeGraph.Title.Text = "Giroscopio";
            magnetometerGraph.Title.Text = "Magnetometro senza discontinuità";
            magnetometerDiscGraph.Title.Text = "Magnetometro con discontinuità";

            accelerometerGraph.XAxis.MajorGrid.IsVisible = true;
            accelerometerGraph.YAxis.MajorGrid.IsVisible = true;

            gyroscopeGraph.XAxis.MajorGrid.IsVisible = true;
            gyroscopeGraph.YAxis.MajorGrid.IsVisible = true;

            magnetometerGraph.XAxis.MajorGrid.IsVisible = true;
            magnetometerGraph.YAxis.MajorGrid.IsVisible = true;

            magnetometerDiscGraph.XAxis.MajorGrid.IsVisible = true;
            magnetometerDiscGraph.YAxis.MajorGrid.IsVisible = true;

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

#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        private void Resize()
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
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


        private double Modulus(double v1, double v2, double v3)
        {
            return Math.Sqrt(v1*v1 + v2*v2 + v3*v3);
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

        //EVENTI

        private void Form_Load(object sender, EventArgs e)
        {
            c = c.OrderBy(ZedGraphControl => ZedGraphControl.Name);
            comboBox1.DisplayMember = "Name";

            for (int i = 0; i < c.Count(); i++)
            {
                comboBox1.Items.Add(c.ElementAt(i));
            }

        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Enabled = true;
            textBox2.Enabled = true;
            textBox3.Enabled = true;
            textBox4.Enabled = true;
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
                    for (int i = 0; i < c.Count(); i++)
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
    }
}
