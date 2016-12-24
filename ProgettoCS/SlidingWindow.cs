using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
 * L'analizzatore, "semplificando", eseguira' in un while(true)
 * le operazioni. Aggiunge un nuovo elemento recuperandolo dalla
 * coda che ha in comune con il listener. Se la finestra da analizzare
 * e' stata riempita (ovvvero se count == size) allora analizza la
 * finestra e poi la aggiorna, ovvero "scarta" i primi 250 (in questo caso)
 * elementi. Con la struttura dati che abbiamo pensato, per scartare i primi
 * N elementi basta incrementare l'indice di start di N.
 * 
 * C'e' da controllare bene quando la finestra e' effettivamente piena.
 * Non mi ricordo bene quello che disse Zandron e a quest'ora non ho la
 * forza di pensare. L'idea generale pero' e' questa.
 * 
 *  while(true)
 *    add(v)
 *    if(count == size)
 *      analize()
 *      updatewin()
 *      
 *      
 * [ | | | | | | | | .. | ]  <-- Finestra vuota
 * ^s=e
 * s = 0, e = 0
 * 
 * [5| | | | | | | | .. | ]  <-- Finestra dopo aver aggiunto un elemento
 * ^s^e
 * s = 0, e = 1
 * 
 * [5|2|4|1|0|1|2|7| .. |9]  <-- Finestra piena
 * ^s                   ^e
 * s = 0, e = 500
 * 
 * [5|2|4|1|0|1|2|7| .. |9]  <-- Finestra dopo l'update
 *           ^s         ^e
 * s = 250, e = 500
 * 
 * I vecchi elementi rimangono nella finestra ma non li considero.
 * Quando verra' richiamata la Add(double) si aggiornera' l'indice
 * end e sovrascrivo i vecchi dati.
 *  
 */

namespace ProgettoCS
{
    public class SlidingWindow
    {
        public const int size = 500;

        // L'array e' di dimensione size+1 perche' altrimenti
        // non si riuscirebbe a distinguere il caso in cui
        // la finestra e' piena dal caso in cui la finestra e'
        // vuota. In questo mondo invece la lista e' vuota quando
        // end == start, mentre e' piena quando end == start - 1.
        private double[] window = new double[size+1];
        private int start;
        private int end;

        public SlidingWindow()
        {
            start = 0;
            end = 0;
        }

        // L'operazione modulo puo' restituire valori negativi.
        // Prima di restituire il valore controllo quindi che sia
        // positivo. In caso contrario sommo 501, per portarmi
        // al valore equivalente positivo.
        public int Count
        {
            get
            {
                int r = (end - start) % (size + 1);
                // Controlol che il risultato ottentuto prima sia positivo.
                // Se cosi' non fosse, per trovare il rappresentante positivo,
                // aggiungo 501.
                r = r < 0 ? r + (size + 1) : r;
                return r;
            }
        }

        public void Add(double v)
        {
            window[end] = v;
            end = (end + 1) % (size+1);
        }

        public double Get(int index)
        {
            return window[(start + index) % (size+1)];
        }

        public void UpdateWindow()
        {
            start = (start + size / 2) % (size+1);
        }


    }
}
