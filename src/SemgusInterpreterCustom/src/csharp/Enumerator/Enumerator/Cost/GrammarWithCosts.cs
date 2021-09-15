using Semgus.Interpreter;
using Semgus.Util;
using System.Text;

namespace Semgus.Enumerator {
    public class GrammarWithCosts {
        public RuleGroupByCost LeafRules { get; }
        public RuleGroupByCost BranchRules { get; }

        public GrammarWithCosts(RuleGroupByCost leafRules, RuleGroupByCost branchRules) {
            LeafRules = leafRules;
            BranchRules = branchRules;
        }

        public static GrammarWithCosts Construct(InterpretationGrammar grammar, IRuleCostFunction costFunction) {
            var leafRules = new DictOfList<int, RuleInterpreter>();
            var branchRules = new DictOfList<int, RuleInterpreter>();

            foreach (var kvp in grammar.LeafTerms) {
                foreach (var rule in kvp.Value) {
                    leafRules.Add(costFunction.GetCost(rule), rule);
                }
            }
            foreach (var kvp in grammar.BranchTerms) {
                foreach (var rule in kvp.Value) {
                    branchRules.Add(costFunction.GetCost(rule), rule);
                }
            }

            return new GrammarWithCosts(new RuleGroupByCost(leafRules), new RuleGroupByCost(branchRules));
        }

        public string PrettyPrint() {
            var sb = new StringBuilder();
            sb.AppendLine("Leaf: {");
            foreach(var kvp in LeafRules) {
                foreach(var rule in kvp.Value) {
                    sb.AppendLine($"  [{kvp.Key}] {rule.Nonterminal}:{rule.Identifier}");
                }
            }
            sb.AppendLine("},");
            sb.AppendLine("Branch: {");
            foreach (var kvp in BranchRules) {
                foreach (var rule in kvp.Value) {
                    sb.AppendLine($"  [{kvp.Key}] {rule.Nonterminal}:{rule.Identifier}");
                }
            }
            sb.AppendLine("},");
            return sb.ToString();
        }
    }
}