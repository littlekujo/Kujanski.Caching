using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Timers;


namespace Kujanski.Caching.IntervalCache
{
    public class IntervalCache : IIntervalCache
    {
        #region Member Variables
        //private static readonly ILogger _log = LogManager.GetLogger(typeof(IntervalCache).ToString());
        private ConcurrentDictionary<int, IIntervalCacheItem> _dictionary = new ConcurrentDictionary<int, IIntervalCacheItem>();
        #endregion

        #region Constructors
        public IntervalCache()
        {
            FlushTimer = new Timer(FlushIntervalMs);
            FlushTimer.AutoReset = true;
            FlushTimer.Elapsed += new ElapsedEventHandler(FlushTimerElapsed);
        }

        public IntervalCache(string inName, Func<IIntervalCacheItem, bool> inFlushFunc, 
            bool inAutoStart = true, bool inUseFuzzyRejection = true, 
            int inFlushIntervalMs = 20011, int inItemExpirationSeconds = 53) : this()
        {
            Name = inName;
            FlushFunction = inFlushFunc;
            AutoStart = inAutoStart;
            UseFuzzyRejection = inUseFuzzyRejection;
            FlushIntervalMs = inFlushIntervalMs;
            ItemExpirationSeconds = inItemExpirationSeconds;
        }
        #endregion

        #region Protected Properties
        /// <summary>
        /// The internal collection of items being cached
        /// </summary>
        protected ConcurrentDictionary<int, IIntervalCacheItem> Dictionary
        {
            get
            {
                return _dictionary;
            }
            set
            {
                _dictionary = value;
            }
        }
        /// <summary>
        /// The actual internal timer object
        /// </summary>
        protected Timer FlushTimer { get; set; }
        #endregion

        #region Public Properties
        /// <summary>
        /// An arbitrary name of this cache to distinguish between multiple caches
        /// </summary>
        public string Name { get; set; } = "IntervalCache";
        /// <summary>
        /// Will kick off the flush interval timer automatically if an item is added successfully
        /// </summary>
        public bool AutoStart { get; set; } = true;
        /// <summary>
        /// Define what happens when the flush interval expires/occurs. This function should return true for an item to be marked as handled and ready for delete.
        /// </summary>
        public Func<IIntervalCacheItem, bool> FlushFunction { get; set; }
        /// <summary>
        /// The interval at which the flush function is called as well as when cleanup is checked etc.
        /// </summary>
        public int FlushIntervalMs { get; set; } = 20011;
        /// <summary>
        /// Items will be removed when their original attempt date passes the expiration age or their times attempted is exceeded
        /// </summary>
        public int ItemExpirationSeconds { get; set; } = 53;
        /// <summary>
        /// Items will be removed when their original attempt date passes the expiration age or their times attempted is exceeded
        /// </summary>
        public int ItemExpirationTimesAttempted { get; set; } = 5;

        /// <summary>
        /// Indicates the flush interval timer is currently running
        /// </summary>
        public bool IsRunning { get; set; }
        /// <summary>
        /// Indicates currently in process of flushing function
        /// </summary>
        public bool IsFlushing { get; set; }
        /// <summary>
        /// Determines is the HasCloseTo check is used to not allow similar/almost identical items to be added to the cache
        /// </summary>
        public bool UseFuzzyRejection { get; set; } = true;
        #endregion

        #region Protected Methods
        protected bool HasAnyCloseTo(IIntervalCacheItem item)
        {
            if (item is null)
            {
                return false;
            }

            if (Dictionary.Any(n => n.Value != null && n.Value.IsCloseTo(item)))
            {
                return true;
            }

            return default;
        }

        protected bool HasAnyDeletedCloseTo(IIntervalCacheItem item)
        {
            if (item is null)
            {
                return false;
            }

            if (Dictionary.Any(n => n.Value != null && n.Value.DeleteFlag && n.Value.IsCloseTo(item)))
            {
                return true;
            }

            return default;
        }

        protected bool SetItemDeletable(IIntervalCacheItem item)
        {
            if (item is null)
            {
                return false;
            }

            var origHashCode = item.GetHashCode();
            item.DeleteFlag = true;
            var success = Dictionary.TryUpdate(origHashCode, item, item);
            if (success)
            {
                return true;
            }

            return false;
        }

        protected void FlushTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (IsRunning == false)
            {
                return;
            }

            if (!IsFlushing)
            {
                FlushCache();
            }

            ClearDeletableItems();
            if (Dictionary.Count == 0)
            {
                StopFlushPolling();
            }
        }
        #endregion

        #region Public Methods
        public bool Add(IIntervalCacheItem item)
        {
            if (!Dictionary.ContainsKey(item.GetHashCode()))
            {
                if (UseFuzzyRejection && HasAnyCloseTo(item))
                {
                    return false;
                }

                //_log.Debug($"IntervalCache {Name} Add: Adding item {item.ToString()}.");
                bool result = Dictionary.TryAdd(item.GetHashCode(), item);
                if (result && AutoStart)
                {
                    StartFlushPolling();
                }

                return result;
            }

            return false;
        }

        public int GetCount()
        {
            if (Dictionary is null)
            {
                return 0;
            }

            return Dictionary.Count;
        }

        public int ClearDeletableItems()
        {
            if (Dictionary.Count == 0)
            {
                return 0;
            }

            var expirationDate = DateTime.Now.Subtract(new TimeSpan(0, 0, ItemExpirationSeconds));

            var items = Dictionary.Where(v =>
                v.Value.DeleteFlag || v.Value.OriginalAttempt < expirationDate ||
                v.Value.Attempts >= ItemExpirationTimesAttempted);

            IIntervalCacheItem removed;
            var count = 0;
            foreach (var itemKvp in items)
            {
                var item = itemKvp.Value;
                bool success = Dictionary.TryRemove(item.GetHashCode(), out removed);
                if (success)
                {
                    count += 1;
                }
            }

            //_log.Debug($"IntervalCache {Name} ClearDeletableItems: Cleared {count} items.");
            return count;
        }

        public void StartFlushPolling()
        {
            if (IsRunning)
            {
                return;
            }

            FlushTimer.Start();
            IsRunning = true;
            //_log.Debug($"IntervalCache {Name} StartFlushPolling: Polling Started.");
        }

        public void StopFlushPolling()
        {
            if (IsRunning == false)
            {
                return;
            }

            FlushTimer.Stop();
            IsRunning = false;
            //_log.Debug($"IntervalCache {Name} StopFlushPolling: Polling Stopped.");
        }

        public int FlushCache()
        {
            IsFlushing = true;
            var count = 0;
            var items = Dictionary
                .Where(v => v.Value?.DeleteFlag == false)
                .OrderBy(v => v.Value?.LastAttempt);

            if (items == null || !items.Any())
            {
                //_log.Debug($"IntervalCache {Name} FlushCache: No items to flush.");
                IsFlushing = false;
                return 0;
            }

            //_log.Debug($"IntervalCache {Name} FlushCache: Starting flush of {items.Count()} possible items...");
            foreach (var itemKvp in items)
            {
                var item = itemKvp.Value;
                var result = false;
                try
                {
                    if (FlushFunction != null)
                    {
                        if (UseFuzzyRejection && HasAnyDeletedCloseTo(item))
                        {
                            SetItemDeletable(item);
                            //_log.Debug($"IntervalCache {Name} FlushCache: Ignoring item {item.ToString()} because there was another similar item recently flushed!");
                            continue;
                        }

                        result = FlushFunction(item);
                        item.Attempts = item.Attempts + 1;
                    }
                }
                catch (Exception ex)
                {
                    //_log.Debug($"IntervalCache {Name} FlushCache: Flushing item {item.ToString()}. Failed with exception! {ex.ToString()}");
                    IsFlushing = false;
                    item.Attempts += 1;
                }

                if (result)
                {
                    //_log.Debug($"IntervalCache {Name} FlushCache: Flushing item {item.ToString()}. Success!");
                    SetItemDeletable(item);
                    count += 1;
                }
                else
                {
                    //_log.Debug($"IntervalCache {Name} FlushCache: Flushing item {item.ToString()}. Failed!");
                }
            }

            //_log.Debug($"IntervalCache {Name} FlushCache: Done flushing {count} items.");
            IsFlushing = false;
            return count;
        }

        public int ClearAndReset()
        {
            StopFlushPolling();
            if (Dictionary.Count == 0)
            {
                return 0;
            }

            IIntervalCacheItem removed;
            var count = 0;
            foreach (var itemKvp in Dictionary)
            {
                var item = itemKvp.Value;
                bool success = Dictionary.TryRemove(item.GetHashCode(), out removed);
                if (success)
                {
                    count += 1;
                }
            }

            //_log.Debug($"IntervalCache {Name} ClearAndReset: Cleared {count} items.");
            return count;
        }
        #endregion

        #region Disposal
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    StopFlushPolling();
                    FlushTimer.Elapsed -= new ElapsedEventHandler(FlushTimerElapsed);
                }
            }

            disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

    }
}