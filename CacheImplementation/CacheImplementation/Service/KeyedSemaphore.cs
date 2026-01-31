using System;
using System.Collections.Concurrent;

namespace CacheImplementation.Service;

public sealed class KeyedSemaphore
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = [];
    public async Task<IDisposable> LockAsync(string key, CancellationToken ct)
    {
        var semLock = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1,1));
        await semLock.WaitAsync(ct);
        return new Releaser(key, semLock, _locks);
    }
    
    public sealed class Releaser(string key, SemaphoreSlim semLock, ConcurrentDictionary<string, SemaphoreSlim> locks) : IDisposable
    {
        private readonly string _key = key;
        private readonly SemaphoreSlim _semLock = semLock;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = locks;
        public void Dispose()
        {
            _semLock.Release();
            if(_semLock.CurrentCount==1)
                _locks.TryRemove(_key, out _);
        }
    }
}
