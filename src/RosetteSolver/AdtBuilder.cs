using Semgus.Util;
using Semgus.Interpreter;
using Semgus.Syntax;

namespace Semgus.Solver.Rosette {
    class AdtBuilder{
        private static readonly CodeTextBuilder _builder = new CodeTextBuilder();
        public static string BuildAdtRepr(InterpretationGrammar g) {
        
            _builder.Write("\n;;; STRUCT DECL SECTION\n");

            foreach (Nonterminal nt in g.Nonterminals) {
                _builder.LineBreak();
                _builder.Write($"; {nt.Name} nonterminal");
                foreach (ProductionRuleInterpreter pi in g.Productions[nt]) {
                    _builder.LineBreak();
                    using (_builder.InParens()) {
                        _builder.Write($"struct Struct_{pi.Syntax.Constructor} ");
                        int cnt = 0;
                        bool first = true;
                        using (_builder.InParens()) {
                            foreach (TermVariableInfo v in pi.Syntax.ChildTerms) {
                                if (first) {
                                    first = false;
                                } else {
                                    _builder.Write(" ");
                                }
                                _builder.Write(v.Name);
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