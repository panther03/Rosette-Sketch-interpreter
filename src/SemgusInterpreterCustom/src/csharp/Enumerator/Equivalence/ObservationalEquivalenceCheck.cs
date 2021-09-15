using Semgus.Interpreter;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Semgus.Enumerator {
    public class ObservationalEquivalenceCheck : IProgramEquivalenceCheck {
        public IReadOnlyList<IReadOnlyDictionary<string, object>> InputExamples { get; }
        private readonly ObservationalEquivalenceCache _outputCache = new ObservationalEquivalenceCache();

        public ObservationalEquivalenceCheck(IReadOnlyList<IReadOnlyDictionary<string, object>> input) {
            InputExamples = input;
        }


        public IEnumerable<ObservationalEquivalenceCache.CacheEntry> GetCacheEntries() => _outputCache.GetEntries();

        public void Reset() => _outputCache.Clear();

        public bool TryInclude(IDSLSyntaxNode node) {
            var ntCache = _outputCache.SafeGet(node.Nonterminal);
            return ntCache.TryAdd(EnumerateAllOutputs(node), node);
        }

        private IEnumerable<object> EnumerateAllOutputs(IDSLSyntaxNode node) {
            var n_example = InputExamples.Count;
            var n_outvars = node.OutputNames.Count;

            // Compute all outputs
            for (int i = 0; i < n_example; i++) {
                var output_i = node.RunProgram(InputExamples[i]);

                for (int j = 0; j < n_outvars; j++) {
                    yield return output_i[j];
                }
            }
        }

        public string PrettyPrint(int lineSize = 80) {
            var inputStrings = InputExamples.Select(
                dict => $"[{string.Join(", ", dict.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}]"
            ).ToList();

            int n_examples = inputStrings.Count;

            var sb1 = new StringBuilder();
            var sb2 = new StringBuilder("    ");

            foreach (var entry in GetCacheEntries()) {
                sb1.AppendLine($"({entry.Nonterminal}) {entry.Expression}");

                int n_outvars = entry.Outputs.GetLength(1);
                bool extra = false;

                for (int i = 0; i < n_examples; i++) {
                    sb2.Append(inputStrings[i]);
                    sb2.Append(" -> [");
                    for (int j = 0; j < n_outvars; j++) {
                        sb2.Append(entry.Outputs[i, j]);

                        if (j < n_outvars - 1) {
                            sb2.Append(", ");
                        }
                    }
                    sb2.Append("];  ");

                    if (sb2.Length > lineSize) {
                        sb1.AppendLine(sb2.ToString());
                        sb2.Clear();
                        sb2.Append("    ");
                        extra = false;
                    } else {
                        extra = true;
                    }
                }
                if (extra) {
                    sb1.AppendLine(sb2.ToString());
                    sb2.Clear();
                    sb2.Append("    ");
                }
            }

            return sb1.ToString();
        }
    }
}
