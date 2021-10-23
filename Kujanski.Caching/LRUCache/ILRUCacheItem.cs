using System;
using System.Collections.Generic;
using System.Text;

namespace Kujanski.Caching.LRUCache
{
    public interface ILRUCacheItem<T>
    {
        ILRUCacheItem<T> PreviousItem { get; set; }
        ILRUCacheItem<T> NextItem { get; set; }
        int Key { get; set; }
        T Value { get; set; }
    }
}
