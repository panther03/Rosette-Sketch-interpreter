using Semgus.Util;
using Semgus.Interpreter;
using Semgus.Syntax;

namespace Semgus.Solver.Rosette {
    class SyntaxBuilder {
        private static readonly CodeTextBuilder _builder = new CodeTextBuilder();
        private readonly static int DEPTH = 4;
        public static string BuildSyntaxGenFns(InterpretationGrammar g) {
        
            _builder.Write("\n;;; SYNTAX SECTION\n");
            _builder.Write($"\n(current-grammar-depth {DEPTH})\n");

            using (_builder.InParens()) {
                _builder.Write("define-grammar (gram)");
                foreach (Nonterminal nt in g.Nonterminals) {
                    _builder.LineBreak();
                    using (_builder.InBrackets()) {
                        _builder.Write($"{nt.Name}");
                        _builder.LineBreak();
                        using (_builder.InParens()) {
                            _builder.Write("choose");
                            foreach (ProductionRuleInterpreter pi in g.Productions[nt]) {
                                _builder.LineBreak();
                                using (_builder.InParens()) {
                                    _builder.Write($"Struct_{pi.Syntax.Constructor}");
                                    foreach (TermVariableInfo arg in pi.Syntax.ChildTerms) {
                                        _builder.Write($" ({arg.Nonterminal.Name})");
                                    }
                                }
                            }
                            _builder.LineBreak();
                        }
                    }
                    _builder.LineBreak();
                }
            }
            return _builder.ToString();
        }
    }
}