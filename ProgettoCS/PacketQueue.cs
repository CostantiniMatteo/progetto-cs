using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ProgettoCS
{
    public class PacketQueue
    {
        //Queue che funge da buffer per le operazioni da eseguire
        private ConcurrentQueue<Packet> queue;

        public PacketQueue()
        {
            queue = new ConcurrentQueue<Packet>();
        }

        public Packet GetNextElement()
        {
            Packet element = null;
            bool dequeued = queue.TryDequeue(out element);
            if (dequeued)
                return element;
            else
                return null;
        }

        public void EnqueueElement(Packet el)
        {
            if (el != null)
            {
                queue.Enqueue(el);
            }
        }

        public int Count
        {
            get { return queue.Count; }
        }

        public bool IsEmpty()
        {
            return queue.IsEmpty;
        }

        public void RemoveAllElements()
        {
            queue = new ConcurrentQueue<Packet>();
        }

    }
}
