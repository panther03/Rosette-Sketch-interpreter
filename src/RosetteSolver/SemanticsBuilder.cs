using Semgus.Util;
using Semgus.Interpreter;
using Semgus.Syntax;
using System.Linq;
using System.Collections.Generic;

namespace Semgus.Solver.Rosette {
    class SemanticsBuilder {
        private static readonly CodeTextBuilder _builder = new CodeTextBuilder();
        private readonly static int DEPTH = 4;
        private static string cond_var = "NO_COND";

        private static void GenerateRuleSteps(CodeTextBuilder builder, IInterpretationStep s, Dictionary<VariableInfo,int> outvars, string[] outvals) {
            switch(s) {
                case AssignmentFromLocalFormula a_s:
                    int index;
                    if ((index = (outvars.GetValueOrDefault(a_s.ResultVar, -1))) != -1) {
                        outvals[index] = a_s.Expression.Formula.PrintFormula();
                    } else {
                        builder.Write($" (define {a_s.ResultVar.Name} {a_s.Expression.Formula.PrintFormula()})");
                    } 
                    break;
                case TermEvaluation t_s:
                    _builder.Write(" ");
                    using (builder.InParens()) {
                        builder.Write("define ");
                        if (t_s.OutputVariables.Count() == 1) {
                            if (cond_var == "NO_COND") cond_var = t_s.OutputVariables[0].Name;
                            builder.Write($"{t_s.OutputVariables[0].Name} ");
                        } else {
                            using (builder.InParens()) {
                                foreach (VariableInfo out_var in t_s.OutputVariables) {
                                    if ((index = (outvars.GetValueOrDefault(out_var, -1))) != -1) {
                                        outvals[index] = out_var.Name;
                                    }
                                    builder.Write($" {out_var.Name}");
                                }
                            }
                            builder.Write(" ");
                        }
                        
                        using (builder.InParens()) {
                            builder.Write($"{t_s.Term.Nonterminal.Name}.Sem {t_s.Term.Name}");
                            foreach (VariableInfo in_var in t_s.InputVariables) {
                                builder.Write($" {in_var.Name}");
                            }
                        }
                    }
                    break;
                case ConditionalAssertion c_s:
                    /*foreach (VariableInfo v in c_s.DependencyVariables) {
                        builder.Write($"{v.Name} ");
                    }*/
                    //cond_var = c_s.
                    // unimplemented for now but will be added soon, fall thru for now
                    break;
                default:
                    builder.Write(" UNIMPLEMENTED");
                    break;
            }
        }

        private static void GenerateOutputStatement(CodeTextBuilder builder, string[] outvals) {
            builder.Write(" ");
            if (outvals.Count() == 1) {
                builder.Write(outvals[0]);
            } else {
                builder.Write(" ");
                using (builder.InParens()) {
                    builder.Write("values ");
                    builder.WriteEach(outvals);
                }
            }
        }
        
        private static void GenerateSemantics(CodeTextBuilder builder, SemanticRuleInterpreter s, Dictionary<VariableInfo,int> outvars, string[] outvals) {
            if (s.Steps.Count == 1) {
                GenerateRuleSteps(builder, s.Steps[0], outvars, outvals);
                GenerateOutputStatement(builder, outvals);
            } else {
                using (builder.InParens()) {
                    builder.Write("begin");
                    foreach (IInterpretationStep step in s.Steps) {   
                        GenerateRuleSteps(builder, step, outvars, outvals);
                    }
                    GenerateOutputStatement(builder, outvals);
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
                            cond_var = "NO_COND";

                            _builder.LineBreak();
                            using (_builder.InBrackets()) {
                                using (_builder.InParens()) {
                                    _builder.Write($"Struct_{pi.Syntax.Constructor}");
                                    foreach (TermVariableInfo v in pi.Syntax.ChildTerms) {
                                        _builder.Write($" {v.Name}");
                                    }
                                }
                                _builder.Write(" ");
                            
                                // make assumption that semantics is either one item or
                                // just two, corresponding to an if/else or loop
                                
                                switch (pi.Semantics.Count) {                    
                                    case 1:
                                        GenerateSemantics(_builder, pi.Semantics[0], outvars, outvals); break;
                                    case 2:               
                                        // Two separate builders for each code block of the if/else or while loop
                                        CodeTextBuilder if_builder = new CodeTextBuilder();
                                        CodeTextBuilder else_builder = new CodeTextBuilder();


                                        // DANGER!! we are assuming that the first step is the one that sets the cond variable.
                                        // otherwise you have to do multiple passes. this can be fixed in the future.
                                        GenerateRuleSteps(_builder, pi.Semantics[0].Steps[0], outvars, outvals);
                                        GenerateSemantics(if_builder, pi.Semantics[0], outvars, outvals); 
                                        GenerateSemantics(else_builder, pi.Semantics[1], outvars, outvals); 
                                        using (_builder.InParens()) {
                                            _builder.Write($"if ({cond_var})");
                                            _builder.LineBreak();
                                            using (_builder.InParens()) {
                                                _builder.Write(if_builder.ToString());
                                            }
                                            _builder.LineBreak();
                                            using (_builder.InParens()) {
                                                _builder.Write(else_builder.ToString());
                                            }
                                        }
                                        break;
                                    default:
                                        _builder.Write("UNIMPLEMENTED"); break;
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