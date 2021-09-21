using System.Collections.Generic;
using System.Linq;
using Semgus.Util;
using Semgus.Syntax;

namespace Semgus.Solver.Rosette {
    class SyntaxGenPass{
        private readonly static int DEPTH = 4;
        private static string lhs_name;
        public static  string BuildSyntaxGenFns(SemgusProblem node) {
            var visitor = new SyntaxGenPassVisitor();
            return node.Accept(new SyntaxGenPassVisitor()).ToString();
        }

        private class SyntaxGenPassVisitor : IAstVisitor<CodeTextBuilder> {
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
                using (_builder.InParens()) {
                    _builder.Write("Struct_");
                    DoVisit(node.Atom);
                }
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

            public CodeTextBuilder Visit(NonterminalTermDeclaration node) => _builder.Write($"({lhs_name})");

            public CodeTextBuilder Visit(VariableEvaluation node) => _builder.Write(node.Variable.Name);

            public CodeTextBuilder Visit(Operator node) => _builder.Write(node.Text);

            public CodeTextBuilder Visit(SemgusProblem node) {
                _builder.Write("\n;;; SYNTAX SECTION\n");
                _builder.Write($"\n(current-grammar-depth {DEPTH})\n");
                using (_builder.InParens()) {
                    _builder.Write("define-grammar (gram)");
                    VisitEach(Just<ISyntaxNode>(node.SynthFun));
                    _builder.LineBreak();
                }
                return _builder;
                
            } 

            public CodeTextBuilder Visit(VariableDeclaration node) => _builder.Write($"{node.Name}:{node.Type}");

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
                using (_builder.InParens()) {
                    _builder.Write("Struct_");
                    DoVisit(node.Op);
                    _builder.Write(" ");
                    VisitEach(node.Operands," ");
                }
                return _builder;
            }

            public CodeTextBuilder Visit(Production node) {
                lhs_name = node.Nonterminal.Name;
                _builder.LineBreak();
                using (_builder.InBrackets()) {
                    _builder.Write(lhs_name);
                    _builder.LineBreak();
                    using (_builder.InParens()) {
                        _builder.Write("choose");
                        VisitEach(node.ProductionRules);
                    }
                }
                return _builder;
            }

            public CodeTextBuilder Visit(ProductionRule node) {
                using (_builder.InLineBreaks()) {
                    DoVisit(node.RewriteExpression);
                    return _builder;
                }
            }
        }
    }
}