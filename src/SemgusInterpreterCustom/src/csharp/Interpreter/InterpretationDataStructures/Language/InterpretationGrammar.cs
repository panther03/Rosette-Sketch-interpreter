using System.Collections.Generic;
using System.Linq;
using Semgus.Syntax;
using Semgus.Util;

namespace Semgus.Interpreter {
    public class InterpretationGrammar {
        public int RuleCount => LeafTerms.ValueCount + BranchTerms.ValueCount;
        public DictOfList<Nonterminal, RuleInterpreter> LeafTerms { get; }
        public DictOfList<Nonterminal, RuleInterpreter> BranchTerms { get; }

        public InterpretationGrammar(DictOfList<Nonterminal, RuleInterpreter> leafTerms, DictOfList<Nonterminal, RuleInterpreter> branchTerms) {
            LeafTerms = leafTerms;
            BranchTerms = branchTerms;
        }

        public static InterpretationGrammar FromProductions(IEnumerable<Production> productions, Theory theory) {
            var analyzer = new RoughFunctionalAnalyzer(theory, productions.Select(pr=>pr.RelationInstance));

            var leaf = new DictOfList<Nonterminal, RuleInterpreter>();
            var branch = new DictOfList<Nonterminal, RuleInterpreter>();

            foreach (var production in productions) {
                var relationInstance = production.RelationInstance;

                foreach (var rule in production.ProductionRules) {
                    var analysisResult = analyzer.AnalyzePredicate(rule.Predicate);

                    var interpreter = analysisResult.ToInterpreter(production.Nonterminal, relationInstance, rule.RewriteExpression, theory);

                    interpreter.Identifier = rule.RewriteExpression.GetNamingSymbol();

                    if (rule.IsLeaf()) {
                        leaf.Add(production.Nonterminal, interpreter);
                    } else {
                        branch.Add(production.Nonterminal, interpreter);
                    }
                }
            }

            return new InterpretationGrammar(leafTerms: leaf, branchTerms: branch);

        }

        public static InterpretationGrammar FromAst(SemgusProblem ast, Theory theory) => FromProductions(ast.SynthFun.Productions, theory);
    }
}