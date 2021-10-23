using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Timers;

namespace Kujanski.Caching.SwapCache
{
    public class SwapCache<T> : ISwapCache<T>
    {
        private List<T> _cache1;
        private List<T> _cache2;
        private Timer _expirationTimer;
        private bool _isDisposing;
        private int _activeCache = 1;

        public SwapCache(double intervalMs, Func<List<T>> populationFunction)
        {
            if (_cache1 == null)
            {
                _cache1 = new List<T>();
            }
            if (_cache2 == null)
            {
                _cache2 = new List<T>();
            }

            PopulateFunction = populationFunction;

            _expirationTimer = new Timer(intervalMs);
            _expirationTimer.AutoReset = true;
            _expirationTimer.Elapsed += _expirationTimer_Elapsed;
            _expirationTimer.Start();
        }

        public string Name { get; set; }

        public List<T> ActiveCache
        {
            get
            {
                if(_activeCache == 2)
                    return _cache2;
                else
                    return _cache1;
            }
            protected set
            {
                if (_activeCache == 2)
                    _cache2 = value;
                else
                    _cache1 = value;
            }
        }

        protected List<T> InactiveCache
        {
            get
            {
                if (_activeCache == 2)
                    return _cache1;
                else
                    return _cache2;
            }
            set
            {
                if (_activeCache == 2)
                    _cache1 = value;
                else
                    _cache2 = value;
            }
        }

        public long PopulationCount
        {
            get;
            protected set;
        }

        public DateTime LastPopulated
        {
            get;
            protected set;
        }

        public bool IsPopulating
        {
            get;
            protected set;
        }

        public Func<List<T>> PopulateFunction
        {
            get;
            set;
        }

        public void SetInterval(double intervalMs)
        {
            if (_isDisposing)
                return;

            _expirationTimer.Stop();
            _expirationTimer.Interval = intervalMs;
            _expirationTimer.Start();
        }

        public void StartInterval()
        {
            if (_isDisposing)
                return;
            _expirationTimer.Start();
        }
        public void StopInterval()
        {
            _expirationTimer.Stop();
        }

        public void Swap()
        {
            if (_activeCache == 1)
                _activeCache = 2;
            else
                _activeCache = 1;
        }

        private void _expirationTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_isDisposing)
                return;

            ExecutePopulateFunction();
            
        }

        public bool ExecutePopulateFunction()
        {
            if (_isDisposing || IsPopulating || PopulateFunction == null)
                return false;

            IsPopulating = true;

            try
            {
                InactiveCache = PopulateFunction.Invoke();
            }
            catch (Exception ex)
            {
                Trace.Write($"SwapCache.ExecutePopulateFunction: Error executing Populate Function: ex = {ex.ToString()}");
            }

            PopulationCount++;
            LastPopulated = DateTime.Now;

            if (InactiveCache == null)
            {
                InactiveCache = new List<T>();
                Swap();
                IsPopulating = false;
                return false;
            }

            Swap();
            IsPopulating = false;
            return true;
        }

        #region Destructors
        public void Dispose()
        {
            _isDisposing = true;
            if (_expirationTimer != null)
            {
                _expirationTimer.Stop();
                _expirationTimer.Elapsed -= _expirationTimer_Elapsed;
                _expirationTimer.Dispose();
                _expirationTimer = null;
            }
            if (ActiveCache != null)
            {
                ActiveCache.Clear();
                ActiveCache = null;
            }
            if (InactiveCache != null)
            {
                InactiveCache.Clear();
                InactiveCache = null;
            }
        }
        #endregion
    }
}
