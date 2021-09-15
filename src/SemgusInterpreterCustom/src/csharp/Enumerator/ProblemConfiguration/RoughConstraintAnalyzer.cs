using Semgus.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Semgus.Enumerator {
    public class RoughConstraintAnalyzer {

        public (ISynthesisSpecification, IProgramEquivalenceCheck) Analyze(SemgusProblem ast) {
            var constraints = ast.Constraints;

            var relationInstances = ast.SynthFun.Productions.Select(p => p.RelationInstance).ToList();

            return Analyze(constraints, relationInstances);
        }


        public (ISynthesisSpecification, IProgramEquivalenceCheck) Analyze(IReadOnlyList<Constraint> constraints, IReadOnlyList<SemanticRelationInstance> relationInstances) {
            var examples = new List<BehaviorExample>();
            foreach (var constraint in constraints) {
                foreach (var example in AnalyzePredicate(constraint.Formula, relationInstances)) {
                    examples.Add(example);
                }
            }

            if (examples.Count == 0) {
                throw new ArgumentException(); // todo message
            }

            var outputKey = examples[0].Output.Keys.First();
            return (
                new InductiveConstraint(examples),
                new ObservationalEquivalenceCheck(examples.Select(e => e.Input).ToList())
            );
        }

        private IEnumerable<BehaviorExample> AnalyzePredicate(IFormula predicate, IReadOnlyList<SemanticRelationInstance> relationInstances) {
            if (!(predicate is LibraryFunctionCall call))
                throw new ArgumentException();

            const string NAME_AND = "and";

            if (call.LibraryFunction.Name != NAME_AND)
                throw new ArgumentException();

            foreach (var arg in call.Arguments) {
                yield return AnalyzeClause(arg, relationInstances);
            }
        }

        private BehaviorExample AnalyzeClause(IFormula clause, IReadOnlyList<SemanticRelationInstance> relationInstances) {
            // Assume query is (Term, Lit<T0>, Lit<T1>, ... Lit<TN>)
            // TODO: support aux variables in output slots

            if (!(clause is SemanticRelationQuery query)) throw new ArgumentException();

            if (!(
                query.Terms[0] is VariableEvaluation termVarEval &&
                termVarEval.Variable.DeclarationContext == VariableDeclaration.Context.CT_Term
            )) throw new ArgumentException();

            var relInstance = relationInstances.Where(ri => ri.Relation == query.Relation).First();

            if (!ReferenceEquals(relInstance.Relation, query.Relation)) throw new ArgumentException();

            var relElements = relInstance.Elements;

            var input = new Dictionary<string, object>();
            var output = new Dictionary<string, object>();

            int n = query.Terms.Count;
            for (int i = 1; i < n; i++) {
                if (!(query.Terms[i] is LiteralBase lit)) throw new ArgumentException();

                if (Interpreter.PredicateAnalysisHelper.IsDeclaredAsInput(relElements[i].DeclarationContext)) {
                    input.Add(relElements[i].Name, lit.BoxedValue);
                } else {
                    output.Add(relElements[i].Name, lit.BoxedValue);
                }

            }

            return new BehaviorExample(input, output);
        }
    }
}