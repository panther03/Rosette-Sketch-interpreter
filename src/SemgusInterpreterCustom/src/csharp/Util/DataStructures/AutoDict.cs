using System;
using System.Collections.Generic;

namespace Semgus.Util {
    /// <summary>
    /// Dictionary equipped with a generator function for mapping new keys to new values.
    /// </summary>
    public class AutoDict<TKey, TValue> : Dictionary<TKey,TValue> {
        private readonly Func<TKey, TValue> _ctor;

        public AutoDict(Func<TKey, TValue> ctor) : base() {
            this._ctor = ctor;
        }

        public TValue SafeGet(TKey key) {
            TValue value;
            if (!TryGetValue(key, out value)) {
                value = _ctor(key);
                Add(key, value);
            }
            return value;
        }
    }
}
