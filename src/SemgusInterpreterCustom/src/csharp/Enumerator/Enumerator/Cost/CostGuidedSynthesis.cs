using Semgus.Interpreter;
using Semgus.Util;
using System.Collections.Generic;
using System.Timers;

namespace Semgus.Enumerator {
    public class CostGuidedSynthesis {
        public ILogger Log { get; set; }
        private readonly GrammarWithCosts _grammar;
        private readonly int _maxCost;

        public CostGuidedSynthesis(GrammarWithCosts grammar, int maxCost) {
            _grammar = grammar;
            _maxCost = maxCost;
        }

        public SynthesisResult PerformSynthesis(ISynthesisSpecification spec, IProgramEquivalenceCheck check) {
            var bank = new ExpressionBank();

            var enumerator = new CostSumEnumerator(DSLSyntaxNode.Factory.Instance, _grammar, bank);

            int k = 0;

            for (int budget = 1; budget < _maxCost; budget++) {
                //Log?.Log($"Starting enumeration at cost {budget}");

                var distinctTerms = new List<IDSLSyntaxNode>();

                foreach (var expr in enumerator.EnumerateAtCost(budget)) {
                    k++;

                    // todo: combine evaluation steps for spec, check

                    if (spec.IsSatisfiedBy(expr)) {
                        Log?.Log($"Found a match after {k} terms: {expr}");
                        return new SynthesisResult(
                            success: true,
                            result: expr,
                            bankSize: bank.Size,
                            cost: budget,
                            numChecked: k
                        );
                    } else if (check.TryInclude(expr)) {
                        distinctTerms.Add(expr);
                    }
                    if (k % 1000 == 0) {
                        Log?.Log($"(c={budget}: after {k} terms, bank size is {bank.Size + distinctTerms.Count})");
                    }
                }

                foreach (var expr in distinctTerms) {
                    //Log?.Log($"Adding {expr} to bank ({bank.Size + 1})");
                    bank.Add(expr.Nonterminal, budget, expr);
                }
            }

            Log?.Log($"Unable to find synthesis match after {k} terms");

            return new SynthesisResult(
                success: false,
                result: null,
                bankSize: bank.Size,
                cost: _maxCost,
                numChecked: k
            );
        }
    }
}