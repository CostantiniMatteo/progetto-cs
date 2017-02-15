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

        public Form(PointsQueue pq)
        {
            zedList = GetAll(this, typeof(ZedGraphControl));
            zedList = zedList.OrderBy(ZedGraphControl => ZedGraphControl.Name);

            File.Delete(".../output.csv");

            InitializeComponent();
            Resize();
            groupBoxCount = 3;
            lastTime = 0;

            tabControl1.TabPages[0].Text = "Grafici di base";
            tabControl1.TabPages[1].Text = "Dead Reckoning";

            tabControl1.SizeMode = TabSizeMode.Fixed;
            tabControl1.ItemSize = new Size(tabControl1.Width / 4, 0);

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
            Color[] c = { Color.Red, Color.Blue, Color.Green, Color.OrangeRed, Color.Red, Color.Blue, Color.Green };
            double x = 0;
            lastTime = 0;

            while (!Program.stop)
            {

                double[] points = pointsQueue.GetNextElement();

                if (points != null && !Program.stop)
                {
                    for (int i = 0; i < points.Length - 3; i++)
                    {
                        UpgradeGraph((ZedGraphControl)zedList.ElementAt(i), x, points[i], c[i]);
                    }

                    generateGroupBox(points[7], x);

                    lastLss = (int)points[7];

                    UpgradeGraph((ZedGraphControl)zedList.ElementAt(7), points[8], points[9], Color.OrangeRed);


                    Thread.Sleep(15);
                    x += 0.02;
                }

                //setConnection();

            }
        }

        private void generateGroupBox(double p, double x)
        {
            if ((p != lastLss && lastTime != x) || (pointsQueue.LastWindow && pointsQueue.Count == 0))
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
                l1.Text = "from: " + from + " sec";

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

                this.BeginInvoke((MethodInvoker)delegate ()
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

        private void setConnection()
        {

                if (Program.connected && label2.Text == "DISCONNECTED")
                {
                    this.BeginInvoke((MethodInvoker)delegate ()
                    {
                        label2.Text = "CONNECTED";
                        label2.ForeColor = Color.Green;
                    });
                }
                else if (!Program.connected && label2.Text == "CONNECTED")
                {
                    this.BeginInvoke((MethodInvoker)delegate ()
                    {
                        label2.Text = "DISCONNECTED";
                        label2.ForeColor = Color.Red;
                    });
                }
            

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

    }
}
