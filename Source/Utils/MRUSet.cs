using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.BalintHelper.Utils
{
    public class MRUSet<T> : IEnumerable<T>
    {
        private readonly List<T> _order = [];
        private readonly HashSet<T> _set = [];

        public int Count => _order.Count;

        public T this[int index] => _order[index];

        public void Add(T item)
        {
            if (_set.Contains(item))
            {
                _order.Remove(item);
            }
            else
            {
                _set.Add(item);
            }
            _order.Add(item);
        }

        public bool Remove(T item)
        {
            if (!_set.Remove(item))
            {
                return false;
            }

            _order.Remove(item);
            return true;
        }

        public bool Contains(T item) => _set.Contains(item);

        public IEnumerator<T> GetEnumerator() => _order.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}