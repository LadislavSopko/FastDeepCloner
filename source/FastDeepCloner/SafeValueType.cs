using System.Collections.Generic;

#pragma warning disable 1591

namespace FastDeepCloner
{
    /// <summary>
    /// CustomValueType Dictionary, safe for concurrent use: FastDeepCloner keeps its per-type caches in static instances,
    /// and the first concurrent clones of a type used to corrupt them (TASK163/TASK388). Every member the caches use
    /// takes one lock; Add is add-if-absent, since two threads computing the same type metadata produce equivalent values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="P"></typeparam>
    public class SafeValueType<T, P> : Dictionary<T, P>
    {
        private readonly object _sync = new object();

        public SafeValueType(Dictionary<T, P> dic = null)
        {
            if (dic != null)
                foreach (var item in dic)
                    TryAdd(item.Key, item.Value);
        }

        public new void Add(T key, P item)
        {
            lock (_sync)
            {
                if (!base.ContainsKey(key))
                    base.Add(key, item);
            }
        }

        public new bool ContainsKey(T key)
        {
            lock (_sync)
            {
                return base.ContainsKey(key);
            }
        }

        public new void Clear()
        {
            lock (_sync)
            {
                base.Clear();
            }
        }

        public new bool Remove(T key)
        {
            lock (_sync)
            {
                return base.Remove(key);
            }
        }

        public new P this[T key]
        {
            get
            {
                lock (_sync)
                {
                    return base[key];
                }
            }
            set
            {
                lock (_sync)
                {
                    base[key] = value;
                }
            }
        }

        public bool TryAdd(T key, P item, bool overwrite = false)
        {
            lock (_sync)
            {
                if (base.ContainsKey(key) && !overwrite)
                    return true;

                if (overwrite && base.ContainsKey(key))
                    base.Remove(key);
                base.Add(key, item);

                return true;
            }
        }

        public P GetOrAdd(T key, P item, bool overwrite = false)
        {
            lock (_sync)
            {
                if (base.ContainsKey(key) && !overwrite)
                    return base[key];

                if (overwrite && base.ContainsKey(key))
                    base.Remove(key);
                base.Add(key, item);
                return base[key];
            }
        }

        public P Get(T key)
        {
            lock (_sync)
            {
                if (base.TryGetValue(key, out P value))
                    return value;
                object o = null;
                return (P)o;
            }
        }
    }
}

#pragma warning restore 1591
