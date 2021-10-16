using Semgus.Util;
using Semgus.Interpreter;
using Semgus.Syntax;

namespace Semgus.Solver.Rosette {
    class AdtBuilder{
        private static readonly CodeTextBuilder _builder = new CodeTextBuilder();
        public static string BuildAdtRepr(DictOfList<Nonterminal, ProductionRuleInterpreter> terms) {
        
            _builder.Write("\n;;; STRUCT DECL SECTION\n");

            foreach (Nonterminal nt in terms.Keys) {
                _builder.LineBreak();
                _builder.Write($"; {nt.Name} nonterminal");
                foreach (ProductionRuleInterpreter pi in terms[nt]) {
                    _builder.LineBreak();
                    using (_builder.InParens()) {
                        _builder.Write($"struct Struct_{pi.Syntax.Constructor} ");
                        int cnt = 0;
                        bool first = true;
                        using (_builder.InParens()) {
                            foreach (Nonterminal arg_nt in pi.ChildTermTypes) {
                                if (first) {
                                    first = false;
                                } else {
                                    _builder.Write(" ");
                                }
                                _builder.Write($"t{arg_nt}{cnt}");
                                cnt++;
                            }
                        }
                    }
                }
            }
            return _builder.ToString();
        }
    }
}