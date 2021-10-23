using System;
using System.Collections.Generic;
using System.Text;

namespace Kujanski.Caching.LRUCache
{
    public interface ILRUCache<T>
    {
        T Get(int key);

        void Put(int key, T value);

        int Count();

        int? PeekTopKey();
    }
}
