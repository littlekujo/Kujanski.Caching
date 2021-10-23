using System;

namespace Kujanski.Caching.IntervalCache
{
    public interface IIntervalCache : IDisposable
    {
        /// <summary>
        /// Define what happens when the flush interval expires/occurs. This function should return true for an item to be marked as handled and ready for delete.
        /// </summary>
        Func<IIntervalCacheItem, bool> FlushFunction { get; set; }
        /// <summary>
        /// The interval at which the flush function is called as well as when cleanup is checked etc.
        /// </summary>
        int FlushIntervalMs { get; set; }
        /// <summary>
        /// Items will be removed when their original attempt date passes the expiration age or their times attempted is exceeded
        /// </summary>
        int ItemExpirationSeconds { get; set; }
        /// <summary>
        /// Items will be removed when their original attempt date passes the expiration age or their times attempted is exceeded
        /// </summary>
        int ItemExpirationTimesAttempted { get; set; }
        /// <summary>
        /// Indicates the flush interval timer is currently running
        /// </summary>
        bool IsRunning { get; set; }
        /// <summary>
        /// Indicates currently in process of flushing function
        /// </summary>
        bool IsFlushing { get; set; }
        /// <summary>
        /// Will kick off the flush interval timer automatically if an item is added successfully
        /// </summary>
        bool AutoStart { get; set; }
        /// <summary>
        /// Determines is the HasCloseTo check is used to not allow similar/almost identical items to be added to the cache
        /// </summary>
        bool UseFuzzyRejection { get; set; }
        /// <summary>
        /// An arbitrary name of this cache to distinguish between multiple caches
        /// </summary>
        string Name { get; set; }

        int GetCount();
        bool Add(IIntervalCacheItem item);
        void StartFlushPolling();
        void StopFlushPolling();
        int FlushCache();
        int ClearDeletableItems();
        int ClearAndReset();
    }
}