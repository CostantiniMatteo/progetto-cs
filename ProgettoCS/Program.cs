using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProgettoCS {
    static class Program {

        public static volatile bool stop = false;
        private static Thread listenerThread;
        private static Thread analyzerThread;
        private static Thread drawThread;
        private static Listener l;
        private static Form f;
        private static Analyzer a;
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            PacketQueue packetQueue = new PacketQueue();
            PointsQueue pointsQueue = new PointsQueue();

            l = new Listener(packetQueue);
            f = new Form(pointsQueue);
            a = new Analyzer(f, packetQueue, pointsQueue);

            listenerThread = new Thread(l.Parse);
            analyzerThread = new Thread(a.Read);
            drawThread = new Thread(f.Draw);

            StartThreads();

            Application.Run(f);
        }

        public static void StartThreads()
        {
            while (listenerThread.IsAlive || analyzerThread.IsAlive || drawThread.IsAlive) ;

            listenerThread = new Thread(l.Parse);
            analyzerThread = new Thread(a.Read);
            drawThread = new Thread(f.Draw);

            stop = false;

            analyzerThread.Start();
            drawThread.Start();
            listenerThread.Start();
        }

        public static void StopThreads()
        {
            stop = true;
        }
    }
}
