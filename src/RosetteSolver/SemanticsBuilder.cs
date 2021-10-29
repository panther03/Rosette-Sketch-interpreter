using Semgus.Util;
using Semgus.Interpreter;
using Semgus.Syntax;
using System.Linq;
using System.Collections.Generic;

namespace Semgus.Solver.Rosette {
    class SemanticsBuilder {
        private static readonly CodeTextBuilder _builder = new CodeTextBuilder();
        private readonly static int DEPTH = 4;

        private static void GenerateRuleSteps(IInterpretationStep s, Dictionary<VariableInfo,int> outvars, string[] outvals) {
            switch(s) {
                case AssignmentFromLocalFormula a_s:
                    int index;
                    if ((index = (outvars.GetValueOrDefault(a_s.ResultVar, -1))) != -1) {
                        outvals[index] = a_s.Expression.Formula.PrintFormula();
                    } else {
                        _builder.Write($"(define {a_s.ResultVar.Name} {a_s.Expression.Formula.PrintFormula()})");
                    } 
                    break;
                case TermEvaluation c_s:
                    _builder.Write(" ");
                    using (_builder.InParens()) {
                        _builder.Write("define ");
                        _builder.Write($"{c_s.OutputVariables[0].Name} ");
                        using (_builder.InParens()) {
                            _builder.Write($"{c_s.Term.Nonterminal.Name}.Sem {c_s.Term.Name}");
                            foreach (VariableInfo in_var in c_s.InputVariables) {
                                _builder.Write($" {in_var.Name}");
                            }
                        }
                    }
                    break;
                case ConditionalAssertion:
                    // unimplemented for now but will be added soon, fall thru for now
                default:
                    _builder.Write(" UNIMPLEMENTED");
                    break;
            }
        }

        private static void GenerateOutputStatement(string[] outvals) {
            if (outvals.Count() == 1) {
                _builder.Write(outvals[0]);
            } else {
                _builder.Write(" ");
                using (_builder.InParens()) {
                    _builder.Write("list ");
                    _builder.WriteEach(outvals);
                }
            }
        }
        public static string BuildSemGenFns(InterpretationGrammar g) {
        
            _builder.Write("\n;;; SEMANTICS SECTION\n");
            
            foreach (Nonterminal nt in g.Nonterminals) {
                Dictionary<VariableInfo, int> outvars = new Dictionary<VariableInfo, int>();
                
                // this is plain stupid
                int i = 0;
                foreach (VariableInfo o in g.Productions[nt][0].OutputVariables) {
                    outvars.Add(o, i);
                    i++;
                }

                _builder.LineBreak();
                using (_builder.InParens()) {
                    _builder.Write($"define ({nt.Name}.Sem p");
                    // TODO: is this safe? all of the production rule interpreters
                    // for a nonterminal have the same input names, but it's not tied to
                    // the nonterminal directly
                    foreach (VariableInfo in_var in g.Productions[nt][0].InputVariables) {
                        _builder.Write($" {in_var.Name}");
                    }
                    _builder.Write(")");

                    _builder.LineBreak();
                    using (_builder.InParens()) {
                        _builder.Write("destruct p");
                        foreach (ProductionRuleInterpreter pi in g.Productions[nt]) {
                            string[] outvals = new string[pi.OutputVariables.Count];

                            _builder.LineBreak();
                            using (_builder.InBrackets()) {
                                using (_builder.InParens()) {
                                    _builder.Write($"Struct_{pi.Syntax.Constructor}");
                                    foreach (TermVariableInfo v in pi.Syntax.ChildTerms) {
                                        _builder.Write($" {v.Name}");
                                    }
                                }
                                _builder.Write(" ");
                                // hack for atomic productions
                                if (pi.Semantics[0].Steps.Count == 1) {
                                    GenerateRuleSteps(pi.Semantics[0].Steps[0], outvars, outvals);
                                    GenerateOutputStatement(outvals);
                                } else {
                                    using (_builder.InParens()) {
                                        _builder.Write("begin");
                                        foreach (IInterpretationStep step in pi.Semantics[0].Steps) {   
                                            GenerateRuleSteps(step, outvars, outvals);
                                        }
                                        GenerateOutputStatement(outvals);
                                    }
                                }
                                
                            }
                        }
                    }
                }
                _builder.LineBreak();
            }
            return _builder.ToString();
        }
    }
}