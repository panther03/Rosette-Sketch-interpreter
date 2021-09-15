using Semgus.Interpreter;
using Semgus.Syntax;
using Semgus.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Semgus.Enumerator {
    public class PartialSolutionCollector : ISynthesisSpecification {
        private readonly InductiveConstraint _innerConstraint;

        private readonly Dictionary<bool[], (IDSLSyntaxNode node, int matchCount)> cache
            = new Dictionary<bool[], (IDSLSyntaxNode node, int matchCount)>(BoolArrayComparer.Instance);

        private readonly Dictionary<IDSLSyntaxNode, (bool[] matches, double coverage)> selected
            = new Dictionary<IDSLSyntaxNode, (bool[] matches, double coverage)>();

        public ILogger Log { get; set; }

        public PartialSolutionCollector(InductiveConstraint innerConstraint) {
            this._innerConstraint = innerConstraint;
        }

        public bool IsSatisfiedBy(IDSLSyntaxNode node) {
            var (all, each, matchCount) = _innerConstraint.EvaluateMatchInfo(node);

            if (all) return true;

            if (matchCount > 0) TryInsert(node, each, matchCount);

            return false;
        }

        public bool TryInsert(IDSLSyntaxNode node, bool[] matches, int matchCount) {
            if (cache.TryGetValue(matches, out var tu)) {
                if (node.Size < tu.node.Size) {
                    // Replace
                    if (selected.Remove(tu.node)) {
                        selected.Add(node, (matches, (double)matchCount / matches.Length));
                    }
                    cache[matches] = (node, matchCount);
                    return true;
                } else {
                    return false;
                }
            } else {
                cache.Add(matches, (node, matchCount));
                return true;
            }
        }

        public bool TrySelectNext() {
            int exampleCount = _innerConstraint.Examples.Count;

            bool[] bestKey = null;
            int bestSize = int.MaxValue;
            IDSLSyntaxNode bestNode = null;
            int bestMatchCount = 0;

            // First Cheapest: select a single cheapest program (first enumerated)
            // that satisfies a unique subsetof examples
            foreach (var kvp in cache) {
                var tu = kvp.Value;
                if (
                    tu.node.Size < bestSize &&
                    !selected.ContainsKey(tu.node)
                ) {
                    bestKey = kvp.Key;
                    bestSize = tu.node.Size;
                    (bestNode, bestMatchCount) = tu;
                }
            }

            if (bestNode == null) return false;

            Log?.Log($"Selected new promising partial solution (s={bestSize}, c={bestMatchCount}/{exampleCount}) {bestNode}");
            selected.Add(bestNode, (bestKey, (double)bestMatchCount / exampleCount));

            return true;
        }

        public GrammarWithCosts AssignCosts(InterpretationGrammar grammar) {
            int n_rules = grammar.RuleCount;
            double r_base = 1.0 / n_rules; // todo preclude divide by zero

            var leavesTemp = new List<(RuleInterpreter rule, double rawScore)>();
            var branchesTemp = new List<(RuleInterpreter rule, double rawScore)>();

            var partialsInfo = selected.Select(kvp => (rules: kvp.Key.GatherRulesInTree(new HashSet<RuleInterpreter>()), coverage: kvp.Value.coverage)).ToList();

            var normalizers = new AutoDict<Nonterminal, double>(_=>0);

            foreach (var rule in grammar.LeafTerms.EnumerateAllValues()) {
                var fit = ComputeFitValue(rule, partialsInfo);
                var rawScore = Math.Pow(r_base, 1.0 - fit);
                normalizers[rule.Nonterminal] = normalizers.SafeGet(rule.Nonterminal) + rawScore;

                leavesTemp.Add((rule, rawScore));
            }
            foreach (var rule in grammar.BranchTerms.EnumerateAllValues()) {
                var fit = ComputeFitValue(rule, partialsInfo);
                var rawScore = Math.Pow(r_base, 1.0 - fit);
                normalizers[rule.Nonterminal] = normalizers.SafeGet(rule.Nonterminal) + rawScore;

                branchesTemp.Add((rule, rawScore));
            }

            var leaves = new DictOfList<int, RuleInterpreter>();
            var branches = new DictOfList<int, RuleInterpreter>();

            foreach (var (rule, rawScore) in leavesTemp) {
                var prob = rawScore / normalizers[rule.Nonterminal];
                int cost = Math.Max(1, Convert.ToInt32(-Math.Log2(prob)));
                leaves.Add(cost, rule);
            }
            foreach (var (rule, rawScore) in branchesTemp) {
                var prob = rawScore / normalizers[rule.Nonterminal];
                int cost = Math.Max(1, Convert.ToInt32(-Math.Log2(prob)));
                branches.Add(cost, rule);
            }

            return new GrammarWithCosts(new RuleGroupByCost(leaves), new RuleGroupByCost(branches));
        }

        static double ComputeFitValue(RuleInterpreter rule, List<(HashSet<RuleInterpreter> rules, double coverage)> partialsInfo) {
            double bestCoverage = 0;
            foreach (var (rules, coverage) in partialsInfo) {
                if (rules.Contains(rule) && bestCoverage < coverage) bestCoverage = coverage;
            }
            return bestCoverage;
        }

    }
}