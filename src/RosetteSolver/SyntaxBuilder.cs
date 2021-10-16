using Semgus.Util;
using Semgus.Interpreter;
using Semgus.Syntax;

namespace Semgus.Solver.Rosette {
    class SyntaxBuilder {
        private static readonly CodeTextBuilder _builder = new CodeTextBuilder();
        private readonly static int DEPTH = 4;
        public static string BuildSyntaxGenFns(DictOfList<Nonterminal, ProductionRuleInterpreter> terms) {
        
            _builder.Write("\n;;; SYNTAX SECTION\n");
            _builder.Write($"\n(current-grammar-depth {DEPTH})\n");

            using (_builder.InParens()) {
                _builder.Write("define-grammar (gram)");
                foreach (Nonterminal nt in terms.Keys) {
                    _builder.LineBreak();
                    using (_builder.InBrackets()) {
                        _builder.Write($"{nt.Name}");
                        _builder.LineBreak();
                        using (_builder.InParens()) {
                            _builder.Write("choose");
                            foreach (ProductionRuleInterpreter pi in terms[nt]) {
                                _builder.LineBreak();
                                using (_builder.InParens()) {
                                    _builder.Write($"Struct_{pi.Syntax.Constructor}");
                                    foreach (Nonterminal arg_nt in pi.ChildTermTypes) {
                                        _builder.Write($" ({arg_nt})");
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