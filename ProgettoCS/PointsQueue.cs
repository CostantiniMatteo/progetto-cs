using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgettoCS
{
    public class PointsQueue
    {
        ConcurrentQueue<double[]> queue;

        public PointsQueue()
        {
            queue = new ConcurrentQueue<double[]>();
        }

        public double[] GetNextElement()
        {
            double[] element = null;
            bool dequeued = queue.TryDequeue(out element);
            if (dequeued)
                return element;
            else
                return null;
        }


        public void EnqueueElement(double[] el)
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
            queue = new ConcurrentQueue<double[]>();
        }

    }
}
