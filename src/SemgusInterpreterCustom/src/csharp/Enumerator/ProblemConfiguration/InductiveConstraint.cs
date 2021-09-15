using Semgus.Interpreter;
using System;
using System.Collections.Generic;

namespace Semgus.Enumerator {
    public class InductiveConstraint : ISynthesisSpecification {
        public IReadOnlyList<BehaviorExample> Examples { get; }

        public InductiveConstraint(IReadOnlyList<BehaviorExample> examples) {
            Examples = examples;
        }

        public bool IsSatisfiedBy(IDSLSyntaxNode node) {
            foreach (var example in Examples) {
                if (!IsMatch(node.RunProgramReturningDict(example.Input), example.Output)) return false;
            }
            return true;
        }

        private static bool IsMatch(IReadOnlyDictionary<string, object> actual, IReadOnlyDictionary<string, object> expected) {
            foreach (var kvp in expected) {
                if (!actual.TryGetValue(kvp.Key, out var actualValue)) return false; // Missing expected variable
                if (!kvp.Value.Equals(actualValue)) return false; // Values don't match
            }
            return true;
        }

        public (bool all, bool[] each, int matchCount) EvaluateMatchInfo(IDSLSyntaxNode node) {
            int n = Examples.Count;
            bool all = true;
            bool[] each = new bool[n];
            int matchCount = 0;

            for (int i = 0; i < n; i++) {
                var example = Examples[i];
                if (IsMatch(node.RunProgramReturningDict(example.Input), example.Output)) {
                    each[i] = true;
                    matchCount++;
                } else {
                    each[i] = false;
                    all = false;
                }
            }

            return (all, each, matchCount);
        }
    }
}