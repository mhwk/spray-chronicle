using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace SprayChronicle.Testing
{
    public class EpochGenerator : IList<DateTime>
    {
        public int Count => _epochs.Count;
        
        public bool IsReadOnly => false;
        
        private readonly List<DateTime> _epochs = new List<DateTime>();
        
        public void CopyTo(DateTime[] array, int index)
        {
            _epochs.CopyTo(array, index);
        }

        public void Add(DateTime item)
        {
            _epochs.Add(item);
        }

        public void Add(string iso8601)
        {
            if (!DateTime.TryParseExact(iso8601, "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)) {
                throw new FormatException(string.Format(
                    "Could not convert {0} into a valid yyyy-MM-ddTHH:mm:sszzz date",
                    iso8601
                ));
            }
            _epochs.Add(result.ToUniversalTime());
        }

        public void Clear()
        {
            _epochs.Clear();
        }

        public bool Contains(DateTime value)
        {
            return _epochs.Contains(value);
        }

        public int IndexOf(DateTime value)
        {
            return _epochs.IndexOf(value);
        }

        public void Insert(int index, DateTime value)
        {
            _epochs.Insert(index, value);
        }

        public bool Remove(DateTime value)
        {
            return _epochs.Remove(value);
        }

        public void RemoveAt(int index)
        {
            _epochs.RemoveAt(index);
        }

        public DateTime this[int index]
        {
            get {
                while (index >= _epochs.Count) {
                    _epochs.Add(DateTime.Now);
                }
                return _epochs[index];
            }
            set => _epochs.Add(value);
        }

        public IEnumerator<DateTime> GetEnumerator()
        {
            return _epochs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _epochs.GetEnumerator();
        }
    }
}