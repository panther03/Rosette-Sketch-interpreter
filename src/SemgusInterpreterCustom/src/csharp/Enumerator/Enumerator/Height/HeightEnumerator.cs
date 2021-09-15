using System;
using System.Collections.Generic;
using System.Linq;
using Semgus.Interpreter;
using Semgus.Util;

namespace Semgus.Enumerator {
    public class HeightEnumerator {
        private readonly INodeFactory _nodeFactory;
        private readonly InterpretationGrammar _grammar;
        private readonly ExpressionBank _expressionBank;

        public HeightEnumerator(INodeFactory nodeFactory, InterpretationGrammar grammar, ExpressionBank expressionBank) {
            _nodeFactory = nodeFactory;
            _grammar = grammar;
            _expressionBank = expressionBank;
        }
        
        public IEnumerable<IDSLSyntaxNode> EnumerateAtHeight(int height) {
            if (height < 0) throw new ArgumentOutOfRangeException();

            if (height == 0) {
                foreach (var (_, rule) in _grammar.LeafTerms.EnumerateKeyElementTuples()) {
                    yield return _nodeFactory.Instantiate(rule);
                }
            } else {
                foreach (var (nt, rule) in _grammar.BranchTerms.EnumerateKeyElementTuples()) {
                    foreach (var expr in FillAtChildHeight(rule, height - 1)) {
                        yield return expr;
                    }
                }
            }
        }

        /// <summary>
        /// Enumerate all syntax trees of height <paramref name="maxChildHeight"/> + 1 that can be constructed by instantiating <paramref name="rule"/>
        /// with terms from the current expression bank.
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="maxChildHeight"></param>
        /// <returns></returns>
        public IEnumerable<IDSLSyntaxNode> FillAtChildHeight(RuleInterpreter rule, int maxChildHeight) {
            var candidatesSetsPerSlot = _expressionBank.GetCandidateSets(rule.SubTermSlots);
            var arity = rule.SubTermSlots.Count;

            var selection = new IReadOnlyCollection<IDSLSyntaxNode>[arity];

            foreach (var indexVector in IterationUtil.EnumerateChoicesWithMax(arity, maxChildHeight)) {

                bool missing = false;
                for (int slotIndex = 0; slotIndex < arity; slotIndex++) {
                    if (candidatesSetsPerSlot[slotIndex].TryGetValue(indexVector[slotIndex], out var candidateSet)) {
                        selection[slotIndex] = candidateSet;
                    } else {
                        missing = true;
                        break;
                    }
                }
                if (missing) continue;

                foreach (var choice in IterationUtil.CartesianProduct(selection)) {
                    yield return _nodeFactory.Instantiate(rule,choice);
                }
            }
        }

    }
}