using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace SprayChronicle.QueryHandling
{
    public class BufferedStateRepository<T> : IStatefulRepository<T>
    {
        readonly ILogger<T> _logger;

        readonly IStatefulRepository<T> _repository;

        readonly int _limit;

        readonly ConcurrentDictionary<string,T> _saves;

        readonly ConcurrentDictionary<string,bool> _removes;

        Timer _timer;

        bool _flushing = false;

        public BufferedStateRepository(ILogger<T> logger, IStatefulRepository<T> repository) : this(logger, repository, 10000)
        {
        }

        public BufferedStateRepository(ILogger<T> logger, IStatefulRepository<T> repository, int limit)
        {
            _logger = logger;
            _repository = repository;
            _limit = limit;
            _saves = new ConcurrentDictionary<string,T>();
            _removes = new ConcurrentDictionary<string,bool>();
        }

        public string Identity(T obj)
        {
            return _repository.Identity(obj);
        }

        public T Load(string identity)
        {
            if (!_saves.ContainsKey(identity)) {
                T obj = _repository.Load(identity);
                if (null == obj) {
                    return default(T);
                }
                _saves.TryAdd(identity, _repository.Load(identity));
            }
            return _saves[identity];
        }

        public T Load(Func<IQueryable<T>,T> callback)
        {
            T obj = callback(_saves.Values.AsQueryable());
            if (null != obj) {
                return obj;
            }
            return _repository.Load(callback);
        }

        public IEnumerable<T> Load(Func<IQueryable<T>,IEnumerable<T>> callback)
        {
            return callback(_saves.Values.AsQueryable())
                .Concat(_repository.Load(callback));
        }

        public PagedResult<T> Load(Func<IQueryable<T>,IEnumerable<T>> callback, int page, int perPage)
        {
            var results = callback(_saves.Values.AsQueryable())
                .Concat(_repository.Load(callback));

            return new PagedResult<T>(
                results.Skip((page - 1) * perPage).Take(perPage),
                page,
                perPage,
                results.Count()
            );
        }

        
        public void Save(T obj)
        {
            string identity = Identity(obj);

            if (_saves.ContainsKey(identity)) {
                T removed;
                _saves.TryRemove(identity, out removed);
            }
            
            _saves.TryAdd(identity, obj);
            
            if (_removes.ContainsKey(identity)) {
                bool removed;
                _removes.TryRemove(identity, out removed);
            }

            Flush();
        }

        public void Save(T[] objs)
        {
            foreach (T obj in objs) {
                Save(obj);
            }
        }

        public void Remove(string identity)
        {
            _removes.TryAdd(identity, true);

            if (_saves.ContainsKey(identity)) {
                T removed;
                _saves.TryRemove(identity, out removed);
            }

            Flush();
        }

        public void Remove(string[] identities)
        {
            foreach (string identity in identities) {
                Remove(identity);
            }
        }

        public void Remove(T obj)
        {
            Remove(Identity(obj));
        }

        public void Remove(T[] objs)
        {
            foreach (T obj in objs) {
                Remove(obj);
            }
        }

        void Flush()
        {
            StopFlushTimer();

            if (_flushing) {
                return;
            }

            if (_saves.Count + _removes.Count >= _limit) {
                DoFlushAsync().Wait();
            } else {
                StartFlushTimer();
            }
        }

        void DoFlush()
        {
            _flushing = true;
            
            DoFlushSaves();
            DoFlushRemoves();

            _saves.Clear();
            _removes.Clear();

            _flushing = false;
        }

        async Task DoFlushAsync()
        {
            await Task.Run(() => DoFlush());
        }

        void DoFlushSaves()
        {
            if (_saves.Count == 0) {
                return;
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
        
            _repository.Save(_saves.Values.ToArray());

            stopwatch.Stop();
            _logger.LogInformation(
                "Saved {0} messages in {1}ms ({2}/second)",
                _saves.Count(),
                stopwatch.ElapsedMilliseconds,
                PerSecond(stopwatch.ElapsedMilliseconds, _saves.Count())
            );
        }

        void DoFlushRemoves()
        {
            if (_removes.Count == 0) {
                return;
            }
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
        
            _repository.Remove(_removes.Keys.ToArray());

            stopwatch.Stop();
            _logger.LogInformation(
                "Removed {0} {1}ms ({2}/second)",
                _removes.Count(),
                stopwatch.ElapsedMilliseconds,
                PerSecond(stopwatch.ElapsedMilliseconds, _removes.Count())
            );
        }

        public void Clear()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            _repository.Clear();
            _saves.Clear();
            _removes.Clear();

            stopwatch.Stop();
            _logger.LogInformation("[{0}::CLEAR] {1}ms", typeof(T).Name, stopwatch.ElapsedMilliseconds);
        }

        void StartFlushTimer()
        {
            StopFlushTimer();
            _timer = new Timer(_ => DoFlush(), null, 100, Timeout.Infinite);
        }

        void StopFlushTimer()
        {
            if (null != _timer) {
                _timer.Dispose();
                _timer = null;
            }
        }

        double PerSecond(long ms, long count)
        {
            if (ms == 0) {
                return double.PositiveInfinity;
            }

            return (1000 / ms) * count;
        }
    }
}
