using System;
using System.Collections.Generic;
using System.Text;
using Kujanski.Caching.LRUCache;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kujanski.Caching.Tests
{
    [TestClass]
    public class LRUCacheTests
    {
        LRUCache<int> _sut;
        Random _rand = new Random();

        [TestInitialize]
        public void Init()
        {
            _sut = new LRUCache<int>(3);
        }

        [TestMethod]
        public void LRUCache_PopulatesToCapacity()
        {
            _sut.Put(1, 1);
            _sut.Put(2, 2);
            _sut.Put(3, 3);
            _sut.Put(4, 4);

            Assert.AreEqual(3, _sut.Count());
        }

        [TestMethod]
        public void LRUCache_TopItem_IsMostRecentlyAdded()
        {
            _sut.Put(1, 1);
            _sut.Put(2, 2);
            _sut.Put(3, 3);

            var retrieved = _sut.Get(2);

            _sut.Put(4, 4);
            _sut.Put(5, 5);

            Assert.AreEqual(3, _sut.Count());
            Assert.AreEqual(5, _sut.PeekTopKey());
        }


        [TestMethod]
        public void LRUCache_TopItem_IsMostUsed()
        {
            _sut.Put(1, 1);
            _sut.Put(2, 2);
            _sut.Put(3, 3);

            var retrieved = _sut.Get(2);

            Assert.AreEqual(3, _sut.Count());
            Assert.AreEqual(2, _sut.PeekTopKey());
        }

        [TestMethod]
        public void LRUCache_AddSame_Replaces()
        {
            _sut.Put(1, 1);
            _sut.Put(2, 2);
            _sut.Put(2, 3);

            var retrieved = _sut.Get(2);

            Assert.AreEqual(2, _sut.Count());
            Assert.AreEqual(2, _sut.PeekTopKey());
            Assert.AreEqual(3, retrieved);
        }
    }
}
