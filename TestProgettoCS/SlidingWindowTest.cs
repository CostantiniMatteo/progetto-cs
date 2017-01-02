using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProgettoCS;

namespace TestProgettoCS
{
    [TestClass]
    public class SlidingWindowTest
    {
        [TestMethod]
        public void SWTest()
        {
            var s = new SlidingWindow<int>();

            for(var i = 0; i < 500; i++)
                s.Add(i);

            Assert.AreEqual(500, s.Count, s.Count.ToString());

            s.UpdateWindow();

            Assert.AreEqual(250, s.Count);

            for(var i = 500; i < 700; i++)
                s.Add(i);

            Assert.AreEqual(450, s.Count);

            for(var i = 700; i < 750; i++)
                s.Add(i);

            Assert.AreEqual(500, s.Count);

            s[499] = 1000;
            int x = s[499];

            s.UpdateWindow();

            Assert.AreEqual(x, s[249]);

        }

        [TestMethod]
        public void PacketTest()
        {
            double[,] d = new double[3,3];
            for(var i = 0; i < 3; i++)
                for(var j = 0; j < 3; j++)
                    d[i,j] = i + j;

            var tmp = new List<List<double>>();

            for(int i = 0; i < 3; i ++)
            {
                tmp.Add(new List<double>());
                for(int j = 0; j < 3; j++)
                    tmp[i].Add(d[i,j]);
            }

            Packet p = new Packet(tmp);


            Console.WriteLine("cacca");
        }

    }
}
