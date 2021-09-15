using System;
using System.Collections.Generic;
using System.Linq;

namespace Semgus.Enumerator {
    /// <summary>
    /// Encodes a single input/output example.
    /// </summary>
    public class BehaviorExample {
        public IReadOnlyDictionary<string, object> Input { get; }
        public IReadOnlyDictionary<string, object> Output { get; }

        public BehaviorExample(IReadOnlyDictionary<string, object> input, IReadOnlyDictionary<string, object> output) {
            Input = input;
            Output = output;
        }

        public static IReadOnlyList<BehaviorExample> Zip(IReadOnlyDictionary<string, object[]> input, IReadOnlyDictionary<string, object[]> output) {
            if (!HaveSameLengths(input.Values.Concat(output.Values).ToList(), out var n)) throw new ArgumentException();

            var examples = new List<BehaviorExample>();

            for (int i = 0; i < n; i++) {
                var input_i = new Dictionary<string, object>();
                foreach (var kvp in input) {
                    input_i.Add(kvp.Key, kvp.Value[i]);
                }
                var output_i = new Dictionary<string, object>();
                foreach (var kvp in output) {
                    output_i.Add(kvp.Key, kvp.Value[i]);
                }
                examples.Add(new BehaviorExample(input_i, output_i));
            }

            return examples;
        }

        private static bool HaveSameLengths(IReadOnlyList<IReadOnlyCollection<object>> collections, out int n) {
            n = 0;
            if (collections.Count > 0) {
                n = collections[0].Count;
                for (int i = 1; i < collections.Count; i++) {
                    if (collections[i].Count != n) return false;
                }
            }
            return true;
        }
    }
}