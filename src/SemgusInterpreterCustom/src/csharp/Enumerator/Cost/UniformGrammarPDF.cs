using Semgus.Interpreter;
using Semgus.Syntax;
using Semgus.Util;
using System;

namespace Semgus.Enumerator {
    public static class UniformGrammarPDF {
        public static GrammarWithCosts AssignCosts(InterpretationGrammar grammar) {
            var ruleCounts = new AutoDict<Nonterminal, int>(_ => 0);


            foreach (var rule in grammar.LeafTerms.EnumerateAllValues()) {
                ruleCounts[rule.Nonterminal] = ruleCounts.SafeGet(rule.Nonterminal) + 1;
            }

            foreach (var rule in grammar.BranchTerms.EnumerateAllValues()) {
                ruleCounts[rule.Nonterminal] = ruleCounts.SafeGet(rule.Nonterminal) + 1;
            }
            
            var leaves = new DictOfList<int, RuleInterpreter>();
            var branches = new DictOfList<int, RuleInterpreter>();

            foreach (var rule in grammar.LeafTerms.EnumerateAllValues()) {
                double prob = 1.0 / ruleCounts[rule.Nonterminal];
                int cost = Math.Max(1, Convert.ToInt32(-Math.Log2(prob)));
                leaves.Add(cost, rule);
            }
            foreach (var rule in grammar.BranchTerms.EnumerateAllValues()) {
                double prob = 1.0 / ruleCounts[rule.Nonterminal];
                int cost = Math.Max(1, Convert.ToInt32(-Math.Log2(prob)));
                branches.Add(cost, rule);
            }

            return new GrammarWithCosts(new RuleGroupByCost(leaves), new RuleGroupByCost(branches));
        }
    }
}