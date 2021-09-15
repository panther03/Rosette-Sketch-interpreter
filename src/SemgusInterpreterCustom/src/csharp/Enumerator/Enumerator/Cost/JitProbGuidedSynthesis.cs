using Semgus.Interpreter;
using Semgus.Util;
using System;
using System.Diagnostics;

namespace Semgus.Enumerator {
    public class JitProbGuidedSynthesis {
        public ILogger Log { get; set; }
        private readonly InterpretationGrammar _grammar;
        private readonly int _maxCostStart;
        private readonly int _maxCostStep;
        private readonly TimeSpan _timeout;

        public JitProbGuidedSynthesis(InterpretationGrammar grammar, int maxCostStart, int maxCostStep, TimeSpan timeout) {
            _grammar = grammar;
            _maxCostStart = maxCostStart;
            _maxCostStep = maxCostStep;
            _timeout = timeout;
        }

        public SynthesisResult PerformSynthesis(InductiveConstraint spec, IProgramEquivalenceCheck check) {
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            var costGrammar = UniformGrammarPDF.AssignCosts(_grammar);

            SynthesisResult result;

            var wrap = new PartialSolutionCollector(spec) { Log = Log };

            int maxCost = _maxCostStart;

            int iter = 0;
            int k = 0;
            do {
                Log?.Log($"Grammar (iter {iter}):");
                Log?.Log(costGrammar.PrettyPrint());

                var csg = new CostGuidedSynthesis(costGrammar, maxCost) { Log = Log };
                check.Reset();
                result = csg.PerformSynthesis(wrap, check);

                k += result.NumChecked;

                if (result.Success) return result;

                if(wrap.TrySelectNext()) {
                    costGrammar = wrap.AssignCosts(_grammar);
                } else {
                    break;
                }
                maxCost += _maxCostStep;
                iter++;
            } while (stopwatch.Elapsed < _timeout);

            result.NumChecked = k;
            return result;
        }
    }
}