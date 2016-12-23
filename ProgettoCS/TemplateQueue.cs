using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ProgettoCS
{
    class TemplateQueue<T>
    {
        //Queue che funge da buffer per le operazioni da eseguire
        //Il main (Program.cs) preleva, ListenerServer accoda
        //Non ho capito come funzionano le ConcurrentQueue, spero siano
        //  davvero thread safe se no va implementata la mutua esclusione
        //  e sbatti vari che so bene come funzionano...già già
        private ConcurrentQueue<T> queue;

        public TemplateQueue()
        {
            queue = new ConcurrentQueue<T>();
        }

        //Sarà davvero thread safe?
        //Scopri un modo per testare se è vero
        public T GetNextElement()
        {
            T element = default(T);
            bool dequeued = queue.TryDequeue(out element);
            if (dequeued)
                return element;
            else
                return default(T);
        }

        //Sarà davvero thread safe?
        public void EnqueueElement(T el)
        {
            if (el != null)
            {
                queue.Enqueue(el);
            }
        }


    }
}
