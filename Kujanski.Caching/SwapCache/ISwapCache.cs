using System;
using System.Collections.Generic;
using System.Text;

namespace Kujanski.Caching.SwapCache
{
    public interface ISwapCache<T> : IDisposable
    {
        List<T> ActiveCache
        {
            get;
        }

        string Name { get; set; }

        long PopulationCount
        {
            get;
        }

        DateTime LastPopulated
        {
            get;
        }

        bool IsPopulating
        {
            get;
        }

        Func<List<T>> PopulateFunction
        {
            get;
            set;
        }

        void Swap();

        void SetInterval(double intervalMs);

        void StartInterval();
        void StopInterval();

        bool ExecutePopulateFunction();
    }
}
