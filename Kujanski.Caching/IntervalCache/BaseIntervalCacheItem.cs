using System;
using System.Collections.Generic;
using System.Text;

namespace Kujanski.Caching.IntervalCache
{
    public abstract class BaseIntervalCacheItem : IIntervalCacheItem
    {
        public DateTime OriginalAttempt { get; set; }
        public DateTime LastAttempt { get; set; }
        public bool DeleteFlag { get; set; }
        public int Attempts { get; set; }

        public bool IsCloseTo(object obj)
        {
            IIntervalCacheItem item = obj as IIntervalCacheItem;
            if (item == null)
                return false;

            return false;
        }

        public override bool Equals(object obj)
        {
            IIntervalCacheItem item = obj as IIntervalCacheItem;
            if (item == null)
                return base.Equals(obj);

            return false;
        }

        public override int GetHashCode()
        {
            return OriginalAttempt.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
