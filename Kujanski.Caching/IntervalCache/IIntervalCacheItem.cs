using System;

namespace Kujanski.Caching.IntervalCache
{
    public interface IIntervalCacheItem
    {
        DateTime OriginalAttempt { get; set; }
        DateTime LastAttempt { get; set; }
        bool DeleteFlag { get; set; }
        int Attempts { get; set; }
        bool IsCloseTo(object obj);
        bool Equals(object obj);
        int GetHashCode();
        string ToString();
    }
}