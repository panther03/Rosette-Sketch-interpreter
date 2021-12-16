using Semgus.Util;
using Semgus.Interpreter;
using Semgus.Syntax;
using System.Linq;
using System.Collections.Generic;

namespace Semgus.Solver.Rosette {
    class SemanticsBuilder {
        private static readonly CodeTextBuilder _builder = new CodeTextBuilder();

        private static readonly string DEPTH_VAR_NAME = "__depth";
        private static string DEPTH_VAR_TEMP = DEPTH_VAR_NAME;
        private static string DEPTH_VAR_LAST = DEPTH_VAR_TEMP;
        private static int DEPTH_VAR_TEMP_LVL = 0;
        private readonly static int DEPTH = 4;
        private static string cond_var = "NO_COND";
        private static bool is_recursive = false;
        
        private static Dictionary<Nonterminal,bool> scan_results = new Dictionary<Nonterminal, bool>();


        // For each step in a semantic rule, we generate the corresponding statement in a Racket (begin ...) block
        // based on the type of step it is (Assignment, Evaluation of different term, etc.)
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
                    builder.Write(" ");
                    using (builder.InParens()) {
                        bool is_term_recursive = scan_results[t_s.Term.Nonterminal];
                        int real_out_count = t_s.OutputVariables.Count() + (is_term_recursive ? 1 : 0);
                        if (real_out_count == 1) {
                            builder.Write("define ");
                            if (cond_var == "NO_COND") cond_var = t_s.OutputVariables[0].Name;
                            builder.Write($"{t_s.OutputVariables[0].Name} ");
                        } else {
                            builder.Write("set-vals-list ");
                            DEPTH_VAR_TEMP = $"{DEPTH_VAR_NAME}_o{DEPTH_VAR_TEMP_LVL}";
                            if (is_term_recursive) builder.Write($"{DEPTH_VAR_TEMP} ");
                            foreach (VariableInfo out_var in t_s.OutputVariables) {
                                if ((index = (outvars.GetValueOrDefault(out_var, -1))) != -1) {
                                    outvals[index] = out_var.Name;
                                }
                                builder.Write($" {out_var.Name}");
                            }
                            builder.Write(" ");
                            DEPTH_VAR_TEMP_LVL++;
                        }
                        
                        using (builder.InParens()) {
                            builder.Write($"{t_s.Term.Nonterminal.Name}.Sem {t_s.Term.Name}");
                            if (is_term_recursive) builder.Write($" {DEPTH_VAR_LAST}");
                            foreach (VariableInfo in_var in t_s.InputVariables) {
                                builder.Write($" {in_var.Name}");
                            }
                            DEPTH_VAR_LAST = DEPTH_VAR_TEMP;
                        }
                    }
                    break;
                case ConditionalAssertion c_s:
                    // Not actually generating anything as we just set global variables for the actual productions
                    break;
                default:
                    builder.Write("UNINMPLEMENTED");
                    break;
            }
        }

        // helper function to generate the output of a rule (last statement in begin block)
        // depends on whether we are currently generating a recursive rule
        // in that case, we need to include the depth in the output vars
        private static void GenerateOutputStatement(CodeTextBuilder builder, string[] outvals) {
            int real_inp_count = outvals.Count() + (is_recursive ? 1 : 0);
            if (real_inp_count == 1) {
                builder.Write(" ");
                builder.Write(outvals[0]);
            } else {
                builder.Write(" ");
                using (builder.InParens()) {
                    builder.Write("list ");
                    if (is_recursive) builder.Write($"(- {DEPTH_VAR_TEMP} 1) ");
                    builder.WriteEach(outvals);
                }
            }
        }
        
        // simple helper function to generate differently if there is only one step in a rule
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
            DEPTH_VAR_TEMP = DEPTH_VAR_NAME;
            DEPTH_VAR_LAST = DEPTH_VAR_TEMP;
            DEPTH_VAR_TEMP_LVL = 0;
        }


        // scans and marks rules if they are found to be recursive,
        // the way this is checked is if there is any TermEvaluation with an index of 0
        // which refers to the parent term. That means it is recursive.
        private static bool ScanProdRulesForRecursion(InterpretationGrammar g, Nonterminal nt) {
            foreach (ProductionRuleInterpreter pi in g.Productions[nt]) {
                foreach (SemanticRuleInterpreter si in pi.Semantics) {
                    foreach (IInterpretationStep s in si.Steps) {
                        switch (s) {
                            case TermEvaluation t_s: 
                                if (t_s.Term.Index == 0) return true; break;
                            default: break;
                        }
                    }
                }        
            }
            return false;
        }

        public static string BuildSemGenFns(InterpretationGrammar g) {
        
            _builder.Write("\n;;; SEMANTICS SECTION\n");

            // Have to do a pass through all nonterminals first to determine
            // which ones invoke recursion
            // For these, we need to pass a __depth parameter to limit number of iterations
            foreach (Nonterminal nt in g.Nonterminals) {
                is_recursive = ScanProdRulesForRecursion(g,nt); 
                scan_results.Add(nt, is_recursive);       
            }
            
            foreach (Nonterminal nt in g.Nonterminals) {
                Dictionary<VariableInfo, int> outvars = new Dictionary<VariableInfo, int>();

                is_recursive = scan_results[nt];
                
                // probably better way to do this lol
                int i = 0;
                foreach (VariableInfo o in g.Productions[nt][0].OutputVariables) {
                    outvars.Add(o, i);
                    i++;
                }

                _builder.LineBreak();
                using (_builder.InParens()) {
                    _builder.Write($"define ({nt.Name}.Sem  {g.Productions[nt][0].Syntax.TermVariable.Name}");

                    if (is_recursive) _builder.Write($" {DEPTH_VAR_TEMP}");

                    // TODO: is this safe? all of the production rule interpreters
                    // for a nonterminal have the same input names, but it's not tied to
                    // the nonterminal directly

                    foreach (VariableInfo in_var in g.Productions[nt][0].InputVariables) {
                        _builder.Write($" {in_var.Name}");
                    }
                    _builder.Write(")");

                    if (is_recursive) {
                        _builder.LineBreak();
                        _builder.Write($"(assert (>= {DEPTH_VAR_TEMP} 0))");
                    }
                    _builder.LineBreak();
                    using (_builder.InParens()) {
                        _builder.Write($"destruct {g.Productions[nt][0].Syntax.TermVariable.Name}");

                        // Main loop for each Production in a nonterminal.
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
                                        using (_builder.InParens()) {
                                            _builder.Write("begin");
                                            GenerateRuleSteps(_builder, pi.Semantics[0].Steps[0], outvars, outvals);
                                            GenerateSemantics(if_builder, pi.Semantics[0], outvars, outvals); 
                                            GenerateSemantics(else_builder, pi.Semantics[1], outvars, outvals); 
                                            _builder.Write(" ");
                                            using (_builder.InParens()) {
                                                _builder.Write($"if ({cond_var})");
                                                _builder.LineBreak();
                                                _builder.Write(if_builder.ToString());
                                                _builder.LineBreak();
                                                _builder.Write(else_builder.ToString());
                                            }
                                        }
                                        
                                        break;
                                    default:
                                        // regex tests enter this case due to having more than 2 conditionals
                                        _builder.Write("UNIMPLEMENTED"); break;
                                }
                            }
                        }
                        _builder.LineBreak();
                    }
                    _builder.LineBreak();
                }
                _builder.LineBreak();
            }
            return _builder.ToString();
        }
    }
}