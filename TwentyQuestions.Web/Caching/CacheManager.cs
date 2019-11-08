using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwentyQuestions.Data.Caching;

namespace TwentyQuestions.Web.Caching
{
    public class CacheManager : ICacheManager
    {
        private IMemoryCache _cache;

        public CacheManager(IMemoryCache cache)
        {
            _cache = cache;
        }

        public T Get<T>(string key)
        {
            var value = _cache.Get<T>(key);

            return value;
        }

        public T Get<T>(string key, Func<T> getValue, int minutesToCache)
        {
            var value = Get<T>(key);

            if (value == null)
            {
                value = getValue();

                if (value != null)
                {
                    _cache.Set<T>(key, value, new TimeSpan(0, minutesToCache, 0));

                    return value;
                }
            }

            return default(T);
        }

        public async Task<T> GetAsync<T>(string key, Func<Task<T>> getValue, int minutesToCache)
        {
            var value = Get<T>(key);

            if (value == null)
            {
                value = await getValue();

                if (value != null)
                {
                    _cache.Set<T>(key, value, new TimeSpan(0, minutesToCache, 0));

                    return value;
                }
            }

            return default(T);
        }

        public void Set<T>(string key, T value, int minutesToCache)
        {
            if (value != null)
                _cache.Set<T>(key, value, new TimeSpan(0, minutesToCache, 0));
        }
    }
}
