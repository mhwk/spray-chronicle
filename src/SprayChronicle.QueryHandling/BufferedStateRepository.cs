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
    public sealed class BufferedStateRepository<T> : StatefulRepository<T> where T : class
    {
        private readonly ILogger<T> _logger;

        private readonly IStatefulRepository<T> _repository;

        private readonly int _limit;

        private readonly ConcurrentDictionary<string,T> _saves;

        private readonly ConcurrentDictionary<string,bool> _removes;

        private Timer _timer;

        private bool _flushing = false;

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

        public override string Identity(T obj)
        {
            return _repository.Identity(obj);
        }

        public override T Load(string identity)
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

        public override T Load(Func<IQueryable<T>,T> callback)
        {
            var obj = callback(_saves.Values.AsQueryable());
            if (null != obj) {
                return obj;
            }
            return _repository.Load(callback);
        }

        public override IEnumerable<T> Load(Func<IQueryable<T>,IEnumerable<T>> callback)
        {
            return callback(_saves.Values.AsQueryable())
                .Concat(_repository.Load(callback));
        }

        public override PagedResult<T> Load(Func<IQueryable<T>,IEnumerable<T>> callback, int page, int perPage)
        {
            var results = callback(_saves.Values.AsQueryable())
                .Concat(_repository.Load(callback));

            return new PagedResult<T>(
                results.Skip((page - 1) * perPage).Take(perPage).ToArray(),
                page,
                perPage,
                results.Count()
            );
        }

        
        public override void Save(T obj)
        {
            var identity = Identity(obj);

            if (_saves.ContainsKey(identity)) {
                _saves.TryRemove(identity, out _);
            }
            
            _saves.TryAdd(identity, obj);
            
            if (_removes.ContainsKey(identity)) {
                _removes.TryRemove(identity, out _);
            }

            Flush();
        }

        public override void Save(T[] objs)
        {
            foreach (var obj in objs) {
                Save(obj);
            }
        }

        public override void Remove(string identity)
        {
            _removes.TryAdd(identity, true);

            if (_saves.ContainsKey(identity)) {
                _saves.TryRemove(identity, out _);
            }

            Flush();
        }

        public override void Remove(string[] identities)
        {
            foreach (var identity in identities) {
                Remove(identity);
            }
        }

        public override void Remove(T obj)
        {
            Remove(Identity(obj));
        }

        public override void Remove(T[] objs)
        {
            foreach (var obj in objs) {
                Remove(obj);
            }
        }

        private void Flush()
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

        private void DoFlush()
        {
            _flushing = true;
            
            DoFlushSaves();
            DoFlushRemoves();

            _saves.Clear();
            _removes.Clear();

            _flushing = false;
        }

        private async Task DoFlushAsync()
        {
            await Task.Run(() => DoFlush());
        }

        private void DoFlushSaves()
        {
            if (_saves.Count == 0) {
                return;
            }

            var stopwatch = new Stopwatch();
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

        private void DoFlushRemoves()
        {
            if (_removes.Count == 0) {
                return;
            }
            
            var stopwatch = new Stopwatch();
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

        public override void Clear()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _repository.Clear();
            _saves.Clear();
            _removes.Clear();

            stopwatch.Stop();
            _logger.LogInformation("[{0}::CLEAR] {1}ms", typeof(T).Name, stopwatch.ElapsedMilliseconds);
        }

        private void StartFlushTimer()
        {
            StopFlushTimer();
            _timer = new Timer(_ => DoFlush(), null, 100, Timeout.Infinite);
        }

        private void StopFlushTimer()
        {
            if (null == _timer) return;
            
            _timer.Dispose();
            _timer = null;
        }

        private static double PerSecond(long ms, long count)
        {
            if (ms == 0) {
                return double.PositiveInfinity;
            }

            return (1000 / ms) * count;
        }
    }
}
