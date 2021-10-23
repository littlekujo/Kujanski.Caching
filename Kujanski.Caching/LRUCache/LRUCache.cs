using System;
using System.Collections.Generic;
using System.Text;

namespace Kujanski.Caching.LRUCache
{
    public class LRUCache<T> : ILRUCache<T>
    {
        private ILRUCacheItem<T> _head = new LRUCacheItem<T>(0, default);
        private ILRUCacheItem<T> _tail = new LRUCacheItem<T>(0, default);

        private Dictionary<int, ILRUCacheItem<T>> _collection;
        private int capacity;

        public LRUCache(int capacity)
        {
            _collection = new Dictionary<int, ILRUCacheItem<T>>();
            this.capacity = capacity;

            _head.NextItem = _tail;
            _tail.PreviousItem = _head;
        }

        public T Get(int key)
        {
            if (_collection.ContainsKey(key))
            {
                ILRUCacheItem<T> node = _collection[key];
                DeleteNode(node);
                AddNodeFirst(node);

                return node.Value;
            }
            return default;
        }

        public int? PeekTopKey()
        {
            if (_head == null || _head.NextItem == null)
                return null;

            return _head.NextItem.Key;
        }

        public int Count()
        {
            return _collection.Count;
        }

        public void Put(int key, T value)
        {
            var node = new LRUCacheItem<T>(key, value);

            if (_collection.ContainsKey(key))
            {
                DeleteNode(_collection[key]);
            }

            if (_collection.Count == capacity)
            {
                DeleteNode(_tail.PreviousItem);
            }

            AddNodeFirst(node);
        }

        private void AddNodeFirst(ILRUCacheItem<T> node)
        {
            _collection.Add(node.Key, node);

            if (_head.NextItem == null)
            {
                _head.NextItem = node;
                node.PreviousItem = _head;
                _tail.PreviousItem = node;

            }
            else
            {
                node.NextItem = _head.NextItem;
                node.NextItem.PreviousItem = node;
                node.PreviousItem = _head;
                _head.NextItem = node;
            }
        }

        private void DeleteNode(ILRUCacheItem<T> node)
        {
            _collection.Remove(node.Key);
            node.PreviousItem.NextItem = node.NextItem;
            node.NextItem.PreviousItem = node.PreviousItem;
        }
    }

}
