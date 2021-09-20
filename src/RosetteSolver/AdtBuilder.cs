using System.Collections.Generic;
using System.Linq;
using Semgus.Util;
using Semgus.Syntax;

namespace Semgus.Solver.Rosette {
    class AdtBuilder{
        public static  string BuildAdtRepresentation(SemgusProblem node) {
            var visitor = new AdtBuildVisitor();
            return node.Accept(new AdtBuildVisitor()).ToString();
        }

        private class AdtBuildVisitor : IAstVisitor<CodeTextBuilder> {
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
                _builder.Write(" ");
                using (_builder.InParens()) {}
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

            public CodeTextBuilder Visit(SemgusProblem node) {
                _builder.Write("\n;;; STRUCT DECL SECTION\n");
                return VisitEach(Just<ISyntaxNode>(node.SynthFun));
            }

            public CodeTextBuilder Visit(VariableDeclaration node) => _builder.Write($"{node.Name}:{node.Type}");

            public CodeTextBuilder Visit(SynthFun node) {
                VisitEach(node.Productions);
                _builder.LineBreak();
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
                using (_builder.InParens()) {
                    _builder.Write(node.Relation.Name);
                    _builder.Write(" ");
                    VisitEach(node.Terms, " ");
                }
                return _builder;
            }

            public CodeTextBuilder Visit(OpRewriteExpression node) {
                _builder.Write("Struct_");
                DoVisit(node.Op);
                _builder.Write(" ");
                using (_builder.InParens()) {
                    VisitEach(node.Operands," ");
                }
                return _builder;
            }

            public CodeTextBuilder Visit(Production node) {
                
                _builder.LineBreak();
                _builder.Write($"; {node.Nonterminal.Name} nonterminal");
                VisitEach(node.ProductionRules);
                _builder.LineBreak();
                return _builder;
            }

            public CodeTextBuilder Visit(ProductionRule node) {
                using (_builder.InLineBreaks()) {
                    using (_builder.InParens()) {
                        _builder.Write("struct ");
                        DoVisit(node.RewriteExpression);
                        _builder.Write(" #:transparent");
                        return _builder;
                    }
                }
            }
        }
    }
}