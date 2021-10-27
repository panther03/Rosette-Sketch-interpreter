using Semgus.Util;
using Semgus.Interpreter;
using Semgus.Syntax;
using Semgus.Solvers.Enumerative;

namespace Semgus.Solver.Rosette {
    class ConstraintsBuilder {
        private static readonly CodeTextBuilder _builder = new CodeTextBuilder();
        public static string BuildConstraintFn(InductiveConstraint spec) {
        
            _builder.Write("\n;;; CONSTRAINTS SECTION\n");
            _builder.Write($"\n(define (sol) (gram))\n");

            using (_builder.InParens()) {
                _builder.Write($"define sol_{spec.StartSymbol.Name}\n");
                using (_builder.InParens()) {
                    _builder.Write("synthesize\n");
                    _builder.Write("#:forall (list)\n");
                    _builder.Write("#:guarantee ");
                    using (_builder.InParens()) {
                        _builder.Write("assert ");
                        if (spec.ExampleCount == 1) {
                            PrintExample(spec, spec.Examples[0]);
                        } else {
                            using (_builder.InParens()) {
                                _builder.Write("and");
                                foreach (BehaviorExample ex in spec.Examples) {
                                    _builder.Write(" ");
                                    PrintExample(spec, ex);
                                }
                            }
                        }
                    }
                }
            }

            _builder.Write($"\n(print-forms sol_{spec.StartSymbol.Name})\n");
            return _builder.ToString();
        }

        private static void PrintExample(InductiveConstraint spec, BehaviorExample e) {
            using (_builder.InParens()) {
                _builder.Write("equal? ");
                using (_builder.InParens()) {
                    _builder.Write($"{spec.StartSymbol.Name}.Sem (sol)");
                    foreach (VariableInfo v in e.InputVariables) {
                        _builder.Write($" {e.Values[v.Index]}");
                    }
                }
                // assume one output for now
                _builder.Write($" {e.Values[e.OutputVariables[0].Index]}");
            }
        }
    }
}