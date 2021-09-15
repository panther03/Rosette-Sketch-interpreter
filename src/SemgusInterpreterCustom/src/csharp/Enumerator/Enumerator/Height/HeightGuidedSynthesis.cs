using Semgus.Interpreter;
using Semgus.Util;

namespace Semgus.Enumerator {
    public class HeightGuidedSynthesis {
        public ILogger Log { get; set; }
        private readonly InterpretationGrammar _grammar;

        public HeightGuidedSynthesis(InterpretationGrammar grammar) {
            _grammar = grammar;
        }

        public SynthesisResult PerformSynthesis(ISynthesisSpecification spec, IProgramEquivalenceCheck check) {
            var bank = new ExpressionBank();

            var enumerator = new HeightEnumerator(DSLSyntaxNode.Factory.Instance, _grammar, bank);

            int k = 0;

            const int MAX_HEIGHT = 10;

            for (int height = 0; height < MAX_HEIGHT; height++) {
                Log?.Log($"Starting enumeration at height {height}");

                foreach (var expr in enumerator.EnumerateAtHeight(height)) {
                    k++;
                    if (spec.IsSatisfiedBy(expr)) {
                        Log?.Log($"Found a match after {k} terms: {expr}");
                        return new SynthesisResult(
                            success: true,
                            result: expr,
                            bankSize: bank.Size,
                            cost: height,
                            numChecked: k
                        );
                    } else if (check.TryInclude(expr)) {
                        //Log?.Log($"Adding {expr} to bank ({bank.Size + 1})");
                        bank.Add(expr.Nonterminal, height, expr);
                    }
                    if (k % 1000 == 0) {
                        Log?.Log($"(h={height}: after {k} terms, bank size is {bank.Size})");
                    }
                }
            }

            Log?.Log($"Unable to find synthesis match after {k} terms");

            return new SynthesisResult(
                success: false,
                result: null,
                bankSize: bank.Size,
                cost: MAX_HEIGHT,
                numChecked: k
            );
        }
    }
}