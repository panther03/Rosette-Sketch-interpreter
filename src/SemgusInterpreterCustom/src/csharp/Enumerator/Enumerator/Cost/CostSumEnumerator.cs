using System.Collections.Generic;
using Semgus.Interpreter;
using Semgus.Util;

namespace Semgus.Enumerator {
    public class CostSumEnumerator {
        private readonly INodeFactory _nodeFactory;
        private readonly GrammarWithCosts _grammar;
        private readonly ExpressionBank _expressionBank;

        public CostSumEnumerator(INodeFactory nodeFactory, GrammarWithCosts grammar, ExpressionBank expressionBank) {
            _nodeFactory = nodeFactory;
            _grammar = grammar;
            _expressionBank = expressionBank;
        }

        public IEnumerable<IDSLSyntaxNode> EnumerateAtCost(int budget) {
            if(_grammar.LeafRules.TryGetValue(budget,out var leavesAtCost)) {
                foreach(var leaf in leavesAtCost) {
                    yield return _nodeFactory.Instantiate(leaf);
                }
            }

            foreach(var kvp in _grammar.BranchRules) {
                var ruleCost = kvp.Key;
                if (ruleCost > budget) continue;

                var childBudget = budget - ruleCost;
                foreach(var rule in kvp.Value) {
                    foreach(var expr in FillAtChildCost(rule,childBudget)) {
                        yield return expr;
                    }

                }
            }
        }

        public IEnumerable<IDSLSyntaxNode> FillAtChildCost(RuleInterpreter rule, int childBudget) {
            var candidatesSetsPerSlot = _expressionBank.GetCandidateSets(rule.SubTermSlots);
            var arity = rule.SubTermSlots.Count;

            var costsPerSlot = new IEnumerable<int>[arity];
            for(int i = 0; i <arity; i++) {
                costsPerSlot[i] = candidatesSetsPerSlot[i].Keys;
            }

            var selection = new List<IDSLSyntaxNode>[arity];
            
            foreach (var indexVector in IterationUtil.EnumerateChoicesWithSum(costsPerSlot, childBudget)) {

                for (int slotIndex = 0; slotIndex < arity; slotIndex++) {
                    selection[slotIndex] = candidatesSetsPerSlot[slotIndex][indexVector[slotIndex]];
                }

                foreach (var choice in IterationUtil.CartesianProduct(selection)) {
                    yield return _nodeFactory.Instantiate(rule, choice);
                }
            }
        }
    }
}