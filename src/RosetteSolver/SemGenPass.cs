using System.Collections.Generic;
using Semgus.Interpreter;
using System.Linq;
using Semgus.Util;
using System;
using Semgus.Syntax;

namespace Semgus.Solver.Rosette {
    class SemGenPass{
        private static string stateTypeName;
        private static Production currProduction;

        private static InterpretationGrammar interpretationGrammar;
        private static (List<(string,Type)>,List<(string,Type)>) insOuts;
        public static string BuildSemGenFns(SemgusProblem node) {
            var visitor = new SemGenPassVisitor();
            interpretationGrammar = InterpretationGrammar.FromAst(node,Theory.BasicLibrary);
            return node.Accept(new SemGenPassVisitor()).ToString();
        }

        private static IReadOnlyList<ArgVariableInfo> ConvertArgVariables(IReadOnlyList<VariableDeclaration> relElements, Theory theory) {
            if (relElements[0].DeclarationContext != VariableDeclaration.Context.NT_Term) throw new ArgumentException();

            int argCount = relElements.Count - 1;
            var args = new ArgVariableInfo[argCount];
            for (int i = 1; i < relElements.Count; i++) {
                args[i - 1] = new ArgVariableInfo(
                    relElements[i].Name,
                    theory.GetType(relElements[i].Type),
                    PredicateAnalysisHelper.IsDeclaredAsInput(relElements[i].DeclarationContext)
                );
            }

            return args;
        }

        private static (List<(string,Type)> inputs, List<(string,Type)> outputs) GetInOutNames(IReadOnlyList<ArgVariableInfo> argumentSlots) {
            var inputNames = new List<(string,Type)>();
            var outputNames = new List<(string,Type)>();
            for (int i = 0; i < argumentSlots.Count; i++) {
                if (argumentSlots[i].IsInput) {
                    inputNames.Add((argumentSlots[i].Name,argumentSlots[i].Type));
                } else {
                    outputNames.Add((argumentSlots[i].Name,argumentSlots[i].Type));
                }
            }
            return (inputNames, outputNames);
        }

        private static void processInOutVars(Production p) {
            var argVars = p.RelationInstance.Elements;
            var argVarInfos = ConvertArgVariables(argVars,Theory.BasicLibrary);
            insOuts = GetInOutNames(argVarInfos);
        }

        private static void getSemFnReturnType(Production p, CodeTextBuilder b) {
            List<(string,Type)> outputs;
            processInOutVars(p);
            (_, outputs) = insOuts;
            if (outputs.Count > 1) {
                stateTypeName = "State" + p.Nonterminal.Name;
                b.Write($"struct {stateTypeName}");
                using (b.InBraces()) {
                    foreach ((string name,Type t) o in outputs) {
                        b.LineBreak();
                        b.Write($"{o.t.Name} {o.name};");
                    }
                }
            } else if (outputs.Count == 0) {
                stateTypeName = "void";
                Console.WriteLine("Can't be void");
            } else {
                stateTypeName = outputs[0].Item2.Name;   
            }
        }

        private class SemGenPassVisitor : IAstVisitor<CodeTextBuilder> {
            private static readonly CodeTextBuilder _builder = new CodeTextBuilder();

            private CodeTextBuilder DoVisit(ISyntaxNode node) => node.Accept(this);

            private CodeTextBuilder VisitEach(IEnumerable<ISyntaxNode> nodes) {
                foreach (var node in nodes) DoVisit(node);
                return _builder;
            }

            public CodeTextBuilder Visit(Constraint node) {
                _builder.LineBreak();
                using (_builder.InParens()) {
                    _builder.Write("constraint");
                    using (_builder.InLineBreaks()) {
                        return DoVisit(node.Formula);
                    }
                }
            }

            private CodeTextBuilder VisitEach(IEnumerable<ISyntaxNode> nodes, string sep = " ") {
                bool first = true;
                foreach (var node in nodes) {
                    if (first) {
                        first = false;
                    } else {
                        _builder.Write(sep);
                    }
                    DoVisit(node);
                }
                return _builder;
            }

            private static IEnumerable<T> Just<T>(T value) {
                yield return value;
            }

            public CodeTextBuilder Visit(AtomicRewriteExpression node) {
                    _builder.Write("Struct_");
                    DoVisit(node.Atom);
                return _builder;
            }

            public CodeTextBuilder Visit(LeafTerm node) => _builder.Write(node.Text);

            public CodeTextBuilder Visit(LibraryFunctionCall node) {
                using (_builder.InParens()) {
                    _builder.Write(node.LibraryFunction.Name);
                    _builder.Write(" ");
                    return VisitEach(node.Arguments, " ");
                }
            }

            public CodeTextBuilder Visit<TValue>(Literal<TValue> node) => _builder.Write(node.Value.ToString());

            public CodeTextBuilder Visit(NonterminalTermDeclaration node) => _builder.Write(node.Name);

            public CodeTextBuilder Visit(VariableEvaluation node) => _builder.Write(node.Variable.Name);

            public CodeTextBuilder Visit(Operator node) => _builder.Write(node.Text);

            public CodeTextBuilder Visit(SemgusProblem node) => VisitEach(Just<ISyntaxNode>(node.SynthFun));

            public CodeTextBuilder Visit(VariableDeclaration node) => _builder.Write($"{node.Type} {node.Name}");

            public CodeTextBuilder Visit(SynthFun node) {
                _builder.LineBreak();
                VisitEach(node.Productions);
                return _builder;
            }

            public CodeTextBuilder Visit(SemanticRelationDeclaration node) {
                using (_builder.InParens()) {
                    _builder.WriteEach(Just(node.Name).Concat(node.ElementTypes.Select(e => e.Name)), " ");
                }
                return _builder;
            }

            public CodeTextBuilder Visit(SemanticRelationInstance node) {
                /*using (_builder.InParens()) {
                    _builder.WriteEach(Just(node.Relation.Name).Concat(node.Assignments.Select(v => v.Name)), " ");
                }*/
                return _builder;
            }

            public CodeTextBuilder Visit(SemanticRelationQuery node) {
                return _builder;
            }

            public CodeTextBuilder Visit(OpRewriteExpression node) {
                _builder.Write("Struct_");
                DoVisit(node.Op);
                _builder.Write(" ");
                VisitEach(node.Operands," ");
                return _builder;
            }

            public CodeTextBuilder Visit(Production node) {
                currProduction = node;
                getSemFnReturnType(node,_builder);
                using (_builder.InLineBreaks()) {
                    using (_builder.InParens()) {
                        _builder.Write($"define ({node.Nonterminal.Name}.Sem p");
                        foreach ((String name,Type type) arg in insOuts.Item1) {
                            _builder.Write($" {arg.name}");
                        
                        }
                        _builder.Write(")");
                        _builder.LineBreak();
                        using (_builder.InParens()) {
                            _builder.Write($"destruct p");
                                VisitEach(node.ProductionRules);
                            _builder.LineBreak();
                        }
                    }
                }
                return _builder;
            }

            public CodeTextBuilder Visit(ProductionRule node) {
                using (_builder.InLineBreaks()) {
                    using (_builder.InBrackets()) {
                        using (_builder.InParens()) {
                            DoVisit(node.RewriteExpression);
                        }
                        _builder.Write(" ");
                        using (_builder.InParens()) {
                            List<RuleInterpreter> ris;
                            if (node.IsLeaf()) {
                                ris = interpretationGrammar.LeafTerms[currProduction.Nonterminal];
                            } else {
                                ris = interpretationGrammar.BranchTerms[currProduction.Nonterminal];
                            }
                            foreach (RuleInterpreter ri in ris) {
                                // what might I feed it here?
                                foreach (IAssignmentStatement s in ri._steps) {
                                    _builder.Write(s.PrintCode());
                                    _builder.Write("; ");
                                }
                            }
                        }
                    }
                }
                return _builder;
            }
        }
    }
}