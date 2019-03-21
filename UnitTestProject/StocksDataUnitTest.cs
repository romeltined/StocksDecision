using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StocksDecision.Models;

namespace UnitTestProject
{
    [TestClass]
    public class StocksDataUnitTest
    {
        [TestMethod]
        public void TestGetCorrelation()
        {
            StocksData sd = new StocksData();
            double result = sd.GetCorrelation("ACN", "MSFT");
        }
    }
}
