using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProgettoCS {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            PacketQueue packetQueue = new PacketQueue();
            PointsQueue pointsQueue = new PointsQueue();

            Listener l = new Listener(packetQueue);
            Form f = new Form(pointsQueue);
            Analyzer a = new Analyzer(f, packetQueue, pointsQueue);

            Thread listenerThread = new Thread(l.Parse);
            Thread analyzerThread = new Thread(a.Analize);
            Thread drawThread = new Thread(f.Draw);
            listenerThread.Start();
            analyzerThread.Start();
            drawThread.Start();
            Application.Run(f);
        }
    }
}
