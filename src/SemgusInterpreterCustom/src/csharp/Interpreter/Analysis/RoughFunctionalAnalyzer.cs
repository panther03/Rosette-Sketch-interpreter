using Semgus.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Semgus.Interpreter {
    public class RoughFunctionalAnalyzer {
        private readonly Theory _theory;
        private readonly IReadOnlyDictionary<SemanticRelationDeclaration, SemanticRelationInstance> _relationInstanceMap;

        public RoughFunctionalAnalyzer(Theory theory, IEnumerable<SemanticRelationInstance> relationInstances) {
            this._theory = theory;
            this._relationInstanceMap = relationInstances.ToDictionary(r => r.Relation, r => r);
        }

        /// <summary>
        /// Attempts to convert <paramref name="predicate"/> into an evaluation semantics
        /// (i.e., a sequence of interpretable statements that assign values to variables).
        /// 
        /// This makes strict assumptions about the shape of the predicate.
        /// * At the top level, it must be an `and` of one or more logical clauses, each of which
        ///   uniquely determines the value of one or more variables.
        /// * The clauses themselves must match other expectations described in the comments below.
        /// 
        /// If the predicate does not match the expected shape, an exception will be thrown.
        /// </summary>
        public FunctionalAnalysisResult AnalyzePredicate(IFormula predicate) {
            if (!(predicate is LibraryFunctionCall call))
                throw new ArgumentException();

            const string NAME_AND = "and";

            if (call.LibraryFunction.Name != NAME_AND)
                throw new ArgumentException();

            var results = new FunctionalAnalysisResult();

            foreach (var arg in call.Arguments) {
                AnalyzeClause(arg, results);
            }

            return results;
        }

        /// <summary>
        /// Each clause must either be an `=` or an instance of a semantic relation.
        /// </summary>
        /// <param name="clause"></param>
        /// <param name="results"></param>
        private void AnalyzeClause(IFormula clause, FunctionalAnalysisResult results) {
            switch (clause) {
                case SemanticRelationQuery query:
                    AnalyzeQuery(query, results);
                    return;
                case LibraryFunctionCall call:
                    AnalyzeEquality(call, results);
                    return;
                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// If a clause is an `=`, assume that the *first* argument is a variable that we are
        /// setting equal to the *second* argument, which is a locally interpretable formula
        /// (i.e., one containing no references to child terms).
        /// 
        /// The special case of the syntax constraint (e.g. `(= t (Plus t1 t2))`) is handled separately.
        /// </summary>
        /// <param name="call"></param>
        /// <param name="results"></param>
        private void AnalyzeEquality(LibraryFunctionCall call, FunctionalAnalysisResult results) {
            const string NAME_EQ = "=";

            if (call.LibraryFunction.Name != NAME_EQ || call.Arguments.Count != 2) throw new ArgumentException();

            // Assume first argument is the result
            if (!(call.Arguments[0] is VariableEvaluation resultVarEval)) throw new ArgumentException();
            var resultVar = resultVarEval.Variable;

            if (resultVar.DeclarationContext == VariableDeclaration.Context.NT_Term) {
                results.SetSyntaxConstraint(call);
                return;
            }

            var rhsFormula = call.Arguments[1];
            var dependencies = CollectVariableTerms(rhsFormula, new HashSet<VariableDeclaration>());

            results.AddResult(resultVar, new AssignToLocalFormulaEvaluation(resultVar.Name, ToInterpretableExpression(rhsFormula)), dependencies);
        }


        /// <summary>
        /// If a clause is a semantic relation instance, assume that it is setting some output variable(s)
        /// equal to the result of evaluating the child term on the provided input formula(s).
        /// </summary>
        /// <param name="query"></param>
        /// <param name="results"></param>
        private void AnalyzeQuery(SemanticRelationQuery query, FunctionalAnalysisResult results) {
            var n = query.Terms.Count;

            // Assert query is (Term, ...)
            if (!(
                query.Terms[0] is VariableEvaluation termVarEval &&
                termVarEval.Variable is NonterminalTermDeclaration termVar &&
                termVar.DeclarationContext == VariableDeclaration.Context.PR_Subterm // TODO: handle recursive case
            )) throw new ArgumentException();

            var relElements = _relationInstanceMap[query.Relation].Elements;

            var argVarNames = new List<string>();
            var inputFormulas = new List<IFormula>();
            var outputVariables = new List<VariableDeclaration>();

            // Break into input, output
            for (int i = 1; i < n; i++) {
                var term = query.Terms[i];

                // For now, require that each argument is a plain variable reference
                if (!(term is VariableEvaluation varEval)) throw new ArgumentException();
                var varDec = varEval.Variable;

                argVarNames.Add(varDec.Name);

                if (PredicateAnalysisHelper.IsDeclaredAsInput(relElements[i].DeclarationContext)) {
                    inputFormulas.Add(term);
                } else {
                    outputVariables.Add(varDec);
                }
            }

            var dependencies = new HashSet<VariableDeclaration>();
            foreach (var formula in inputFormulas) {
                CollectVariableTerms(formula, dependencies);
            }

            results.AddResult(outputVariables, new AssignToInnerTermEvaluation(termVar, argVarNames), dependencies);
        }

        /// <summary>
        /// Collect information about all variables that are referenced in the given formula.
        /// </summary>
        /// <param name="formula"></param>
        /// <param name="result">The collection into which to insert discovered variables</param>
        /// <returns><paramref name="result"/></returns>
        private static HashSet<VariableDeclaration> CollectVariableTerms(IFormula formula, HashSet<VariableDeclaration> result) {
            switch (formula) {

                case LibraryFunctionCall lfCall:
                    foreach (var arg in lfCall.Arguments) CollectVariableTerms(arg, result);
                    return result;

                case VariableEvaluation vareval:
                    var vardec = vareval.Variable;
                    result.Add(vardec);
                    return result;

                case LiteralBase:
                    return result;

                default:
                    throw new ArgumentException();
            }
        }

        private ISmtLibExpression ToInterpretableExpression(IFormula formula) {
            switch (formula) {
                case LibraryFunctionCall call:
                    return new FunctionCallExpression(formula, _theory.GetFunction(call.LibraryFunction), call.Arguments.Select(ToInterpretableExpression).ToList());
                case VariableEvaluation varEval:
                    return new VariableEvalExpression(formula, varEval.Variable.Name, _theory.GetType(varEval.Variable.Type));
                case LiteralBase lit:
                    return new LiteralExpression(formula, lit.BoxedValue, lit.ValueType);
                default:
                    throw new ArgumentException();
            }
        }
    }
}