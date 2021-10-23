using System;
using System.Collections.Generic;
using System.Text;

namespace Kujanski.Caching.BasicExpirationCache
{
    public interface IExpirationCache<T>: IDisposable
    {
        List<T> Cache
        {
            get;
        }

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

        void SetInterval(double intervalMs);

        void StartInterval();
        void StopInterval();

        bool ExecutePopulateFunction();
    }
}
