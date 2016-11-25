using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SprayChronicle.EventHandling.Projecting
{
    public class BufferedRepository<T> : IProjectionRepository<T>
    {
        readonly IProjectionRepository<T> _repository;

        readonly int _limit;

        readonly ConcurrentDictionary<string,T> _saves;

        readonly ConcurrentDictionary<string,bool> _removes;

        Timer _timer;

        bool _flushing = false;

        public BufferedRepository(IProjectionRepository<T> repository) : this(repository, 10000)
        {
        }

        public BufferedRepository(IProjectionRepository<T> repository, int limit)
        {
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
        
        public IEnumerable<T> FindBy(Func<IQueryable<T>,IEnumerable<T>> callback)
        {
            foreach (T obj in callback(_saves.Values.AsQueryable())) {
                yield return obj;
            }
            foreach (T obj in _repository.FindBy(callback)) {
                _saves.TryAdd(Identity(obj), obj);
                yield return obj;
            }
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

            if (_saves.Count + _removes.Count >= _limit) {
                DoFlush();
            } else {
                StartFlushTimer();
            }
        }

        async void DoFlush()
        {
            if (_flushing) {
                Console.WriteLine("[{0}] Already flushing...", typeof(T).Name);
                return;
            }

            await Task.Run(() => {
                _flushing = true;
                
                DoFlushSaves();
                DoFlushRemoves();

                _saves.Clear();
                _removes.Clear();

                _flushing = false;
            });
        }

        void DoFlushSaves()
        {
            if (_saves.Count == 0) {
                return;
            }

            var count = _saves.Count();
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
        
            _repository.Save(_saves.Values.ToArray());

            stopwatch.Stop();
            Console.WriteLine(
                "[{0}::SAVE] {1}ms ({2}/second)",
                typeof(T).Name,
                stopwatch.ElapsedMilliseconds,
                PerSecond(stopwatch.ElapsedMilliseconds, count)
            );
        }

        void DoFlushRemoves()
        {
            if (_removes.Count == 0) {
                return;
            }
            
            var count = _removes.Count();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
        
            _repository.Remove(_removes.Keys.ToArray());

            stopwatch.Stop();
            Console.WriteLine(
                "[{0}::REMOVE] {1}ms ({2}/second)",
                typeof(T).Name,
                stopwatch.ElapsedMilliseconds,
                PerSecond(stopwatch.ElapsedMilliseconds, count)
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
            Console.WriteLine("[{0}::CLEAR] {1}ms", typeof(T).Name, stopwatch.ElapsedMilliseconds);
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
