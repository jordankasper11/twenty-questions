using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TwentyQuestions.Data.Caching
{
    public interface ICacheManager
    {
        T Get<T>(string key);

        T Get<T>(string key, Func<T> getValue, int minutesToCache);

        Task<T> GetAsync<T>(string key, Func<Task<T>> getValue, int minutesToCache);

        void Set<T>(string key, T value, int minutesToCache);
    }
}
