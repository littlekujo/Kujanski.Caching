using System;
using System.Collections.Generic;
using System.Text;

namespace Kujanski.Caching.LRUCache
{
    public class LRUCacheItem<T> : ILRUCacheItem<T>
    {
        public ILRUCacheItem<T> PreviousItem { get; set; }
        public ILRUCacheItem<T> NextItem { get; set; }
        public int Key { get; set; }
        public T Value { get; set; }

        public LRUCacheItem(int key, T value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}
