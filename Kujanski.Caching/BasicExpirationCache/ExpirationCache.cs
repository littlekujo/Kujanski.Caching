using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Timers;

namespace Kujanski.Caching.BasicExpirationCache
{
    public class ExpirationCache<T> : IExpirationCache<T>
    {
        private List<T> _cache;
        private Timer _expirationTimer;
        private bool _isDisposing;
        
        public ExpirationCache(double intervalMs, Func<List<T>> populationFunction)
        {
            if(Cache == null)
            {
                Cache = new List<T>();
            }

            PopulateFunction = populationFunction;

            _expirationTimer = new Timer(intervalMs);
            _expirationTimer.AutoReset = true;
            _expirationTimer.Elapsed += _expirationTimer_Elapsed;
            _expirationTimer.Start();
        }

        public List<T> Cache
        {
            get
            {
                return _cache;
            }
            protected set
            {
                _cache = value;
            }
        }

        public long PopulationCount
        {
            get;
            protected set;
        }

        public bool IsPopulating
        {
            get;
            protected set;
        }
        public DateTime LastPopulated
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
                Cache = PopulateFunction.Invoke();
            }
            catch(Exception ex)
            {
                Trace.Write($"ExpirationCache.ExecutePopulateFunction: Error executing Populate Function: ex = {ex.ToString()}");
            }

            PopulationCount++;
            LastPopulated = DateTime.Now;

            if(Cache == null)
            {
                Cache = new List<T>();
                IsPopulating = false;
                return false;
            }

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
            if(Cache != null)
            {
                Cache.Clear();
                Cache = null;
            }
        }
        #endregion
    }
}
