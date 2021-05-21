using System.Collections.Generic;
using System.Linq;
using Semgus.Util;
using Semgus.Syntax;

namespace Semgus.Solver.Sketch {
    class SyntaxGenPass{
        private readonly static string GENERATOR_SKELETON = "assert bnd >= 0; int t = ??;";
        private static int rhs_id = 0;
        private static string lhs_name;
        public static  string BuildSyntaxGenFns(ISyntaxNode node) {
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
                _builder.Write($"if (t=={rhs_id})");
                using (_builder.InBraces()) {
                    _builder.Write("return new Struct_");
                    DoVisit(node.Atom);
                    using (_builder.InParens()) {}
                    _builder.Write(";");
                }
                rhs_id++;
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

            public CodeTextBuilder Visit(NonterminalTermDeclaration node) => _builder.Write($"{node.Name}={lhs_name}_gen(bnd-1), ");

            public CodeTextBuilder Visit(VariableEvaluation node) => _builder.Write(node.Variable.Name);

            public CodeTextBuilder Visit(Operator node) => _builder.Write(node.Text);

            public CodeTextBuilder Visit(SemgusProblem node) => VisitEach(Just<ISyntaxNode>(node.SynthFun));

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
                _builder.Write($"if (t=={rhs_id})");
                using (_builder.InBraces()) {
                    _builder.Write("return new Struct_");
                    DoVisit(node.Op);
                    using (_builder.InParens()) {
                        VisitEach(node.Operands," ");
                    }
                    _builder.Write(";");
                }
                rhs_id++;
                return _builder;
            }

            public CodeTextBuilder Visit(Production node) {
                lhs_name = node.Nonterminal.Name;
                using (_builder.InLineBreaks()) {
                    _builder.LineBreak();
                    _builder.Write("generator ");
                    _builder.Write(lhs_name);
                    _builder.Write($" {node.Nonterminal.Name}_gen(int bnd) ");
                    using (_builder.InBraces()) {
                            _builder.LineBreak();
                            _builder.Write(GENERATOR_SKELETON);
                            rhs_id = 0;
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