using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Semgus.Interpreter;
using Semgus.Util;

namespace Semgus.Enumerator {
    public class RuleGroupByCost : IReadOnlyDictionary<int, IReadOnlyList<RuleInterpreter>> {
        private readonly DictOfList<int, RuleInterpreter> _dict;

        public RuleGroupByCost(DictOfList<int, RuleInterpreter> dict) {
            _dict = dict;
        }

        public IReadOnlyList<RuleInterpreter> this[int key] => _dict[key];
        public IEnumerable<int> Keys => _dict.Keys;
        public IEnumerable<IReadOnlyList<RuleInterpreter>> Values => _dict.Values;
        public int Count => _dict.Count;
        public bool ContainsKey(int key) => _dict.ContainsKey(key);
        public IEnumerator<KeyValuePair<int, IReadOnlyList<RuleInterpreter>>> GetEnumerator() => _dict.Select(
            kvp => new KeyValuePair<int, IReadOnlyList<RuleInterpreter>>(kvp.Key, kvp.Value)
        ).GetEnumerator();

        public bool TryGetValue(int key, [MaybeNullWhen(false)] out IReadOnlyList<RuleInterpreter> value) {
            var b = _dict.TryGetValue(key, out var list);
            value = b ? list : default;
            return b;
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dict).GetEnumerator();
    }
}