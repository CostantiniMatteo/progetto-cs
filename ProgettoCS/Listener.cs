using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProgettoCS
{
    class Listener
    {
        private const int port = 45555;

        // logQueue contiene i file di log che verranno stampati.
        // valQueue contiene i pacchetti ricevuti e parsati dal listener.
        // Entrambe le code sono condivise con Form.cs (per ora).
        private ConcurrentQueue<string> logQueue;
        private PacketQueue valQueue;

        public Listener(PacketQueue valQueue,
            ConcurrentQueue<string> logQueue)
        {
            this.logQueue = logQueue;
            this.valQueue = valQueue;
        }

        public BinaryReader ConnectAndGetReader()
        {
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            TcpListener server = new TcpListener(localAddr, port);

            server.Start();
            logQueue.Enqueue("Waiting for a connection... \n");

            TcpClient client = server.AcceptTcpClient();
            logQueue.Enqueue("Connected!\n");

            NetworkStream stream = client.GetStream();
            BinaryReader bin = new BinaryReader(stream);

            return bin;
        }

        public void Parse()
        {
            List<List<List<double>>> window = new List<List<List<double>>>();
            BinaryReader bin = ConnectAndGetReader();

            try
            {
                while(true)
                {

                    //Lettura e memorizzazione dati
                    int byteToRead;
                    byte[] pacchetto;
                    int numSensori;
                    int maxSensori = 10;
                    float valore;

                    byte[] len = new byte[2];
                    byte[] tem = new byte[3];

                    while(!(tem[0] == 0xFF && tem[1] == 0x32)) // cerca la sequenza FF-32
                    {
                        tem[0] = tem[1];
                        tem[1] = tem[2];
                        byte[] read = bin.ReadBytes(1);
                        tem[2] = read[0];
                    }
                    if(tem[2] != 0xFF) // modalità normale
                    {
                        byteToRead = tem[2]; // byte da leggere
                    }
                    else  // modalità extended-length
                    {
                        len = new byte[2];
                        len = bin.ReadBytes(2);
                        byteToRead = (len[0] * 256) + len[1]; // byte da leggere
                    }

                    byte[] data = new byte[byteToRead + 1];
                    data = bin.ReadBytes(byteToRead + 1); // lettura dei dati

                    if(tem[2] != 0xFF)
                    {
                        pacchetto = new byte[byteToRead + 4]; // creazione pacchetto
                    }
                    else
                    {
                        pacchetto = new byte[byteToRead + 6];
                    }

                    numSensori = (byteToRead - 2) / 52; // calcolo del numero di sensori
                    pacchetto[0] = 0xFF; // copia dei primi elementi
                    pacchetto[1] = 0x32;
                    pacchetto[2] = tem[2];

                    if(tem[2] != 0xFF)
                    {
                        data.CopyTo(pacchetto, 3); // copia dei dati
                    }
                    else
                    {
                        pacchetto[3] = len[0];
                        pacchetto[4] = len[1];
                        data.CopyTo(pacchetto, 5); // copia dei dati
                    }


                    List<List<double>> array = new List<List<double>>(); // salvataggio dati

                    int[] t = new int[maxSensori];

                    for(int x = 0; x < numSensori; x++)
                    {
                        array.Add(new List<double>()); // una lista per ogni sensore
                        t[x] = 5 + (52 * x);
                    }

                    while(true)
                    {
                        for(int i = 0; i < numSensori; i++)
                        {
                            byte[] temp = new byte[4];
                            for(int tr = 0; tr < 13; tr++)// 13 campi, 3 * 3 + 4
                            {
                                if(numSensori < 5)
                                {
                                    temp[0] = pacchetto[t[i] + 3]; // lettura inversa
                                    temp[1] = pacchetto[t[i] + 2];
                                    temp[2] = pacchetto[t[i] + 1];
                                    temp[3] = pacchetto[t[i]];
                                }
                                else
                                {
                                    temp[0] = pacchetto[t[i] + 5];
                                    temp[1] = pacchetto[t[i] + 4];
                                    temp[2] = pacchetto[t[i] + 3];
                                    temp[3] = pacchetto[t[i] + 2];
                                }
                                valore = BitConverter.ToSingle(temp, 0); // conversione
                                array[i].Add(valore); // memorizzazione
                                t[i] += 4;
                            }
                        }


                        valQueue.EnqueueElement(new Packet(array));

                        for(int x = 0; x < numSensori; x++)
                        {
                            t[x] = 5 + (52 * x);
                        }

                        for(int j = 0; j < numSensori; j++)
                        {
                            for(int tr = 0; tr < 13; tr++)
                            {
                                // esempio output su console
                                logQueue.Enqueue(array[j][tr] + "; " + "\n");
                            }
                            logQueue.Enqueue("\n");
                            array[j].RemoveRange(0, 13); // cancellazione dati
                        }
                        
                        logQueue.Enqueue("\n");

                        if(numSensori < 5) // lettura pacchetto seguente
                        {
                            pacchetto = bin.ReadBytes(byteToRead + 4);
                        }
                        else
                        {
                            pacchetto = bin.ReadBytes(byteToRead + 6);
                        }
                    }

                }
            }
            catch(IndexOutOfRangeException)
            {
                logQueue.Enqueue("Finito");
            }
        }
    }
}
