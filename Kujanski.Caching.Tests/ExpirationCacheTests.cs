using Kujanski.Caching.BasicExpirationCache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Kujanski.Caching.Tests
{
    [TestClass]
    public class ExpirationCacheTests
    {
        IExpirationCache<int> _sut;
        Random _rand = new Random();

        [TestInitialize]
        public void Init()
        {
            _sut = new ExpirationCache<int>(100, () =>
            {
                List<int> values = new List<int>();
                for (int i = 0; i < 500; i++)
                {
                    values.Add(_rand.Next(0, 100000));
                }
                return values;
            });
        }

        [TestMethod]
        public void IsInitiallyPopulated()
        {
            _sut.ExecutePopulateFunction();
            Assert.IsNotNull(_sut.Cache);
            Assert.IsTrue(_sut.Cache.Count > 0);
            Assert.IsTrue(_sut.PopulationCount > 0);
        }

        [TestMethod]
        public void PopulatesSeveralTimesOverTime()
        {
            _sut.SetInterval(10);
            Thread.Sleep(500);
            Assert.IsNotNull(_sut.Cache);
            Assert.IsTrue(_sut.Cache.Count > 0);
            Assert.IsTrue(_sut.PopulationCount > 10);
        }

        [TestMethod]
        public void RepopulationChangesItems()
        {
            _sut.ExecutePopulateFunction();
            var first1 = _sut.Cache[0];
            var second1 = _sut.Cache[1];
            Assert.AreEqual(_sut.PopulationCount, 1);

            _sut.ExecutePopulateFunction();
            var first2 = _sut.Cache[0];
            var second2 = _sut.Cache[1];
            Assert.AreEqual(_sut.PopulationCount, 2);

            Assert.AreNotEqual(first1, first2);
            Assert.AreNotEqual(second1, second2);
        }

    }
}
