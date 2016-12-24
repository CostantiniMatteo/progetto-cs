using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProgettoCS;

namespace TestProgettoCS
{
    [TestClass]
    public class SlidingWindowTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var s = new SlidingWindow();

            for(var i = 0; i < 500; i++)
                s.Add(i);

            Assert.AreEqual(500, s.Count, "" + s.Count);

            s.UpdateWindow();

            Assert.AreEqual(250, s.Count);

            for(var i = 500; i < 700; i++)
                s.Add(i);

            Assert.AreEqual(450, s.Count);

            for(var i = 700; i < 750; i++)
                s.Add(i);

            Assert.AreEqual(500, s.Count);
        }
    }
}
