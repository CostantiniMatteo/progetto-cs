using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgettoCS
{
    class Analyzer
    {
        private PacketQueue packetQueue;
        private SlidingWindow window;
        private Form f;

        protected Analyzer(Form f, SlidingWindow window, PacketQueue packetQueue)
        {
            this.f = f;
            this.window = window;
            this.packetQueue = packetQueue;
        }

    }
}
