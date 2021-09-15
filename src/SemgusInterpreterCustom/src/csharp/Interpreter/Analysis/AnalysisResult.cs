using Semgus.Syntax;
using Semgus.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Semgus.Interpreter {
    /// <summary>
    /// Collects and processes information obtained during predicate analysis.
    /// 
    /// Note in particular the use of a variable dependency graph to determine the order
    /// in which to execute assignment statements.
    /// </summary>
    public class FunctionalAnalysisResult {
        public IFormula SyntaxConstraint { get; set; } = null;
        private Dictionary<VariableDeclaration, IAssignmentStatement> _setterDict = new Dictionary<VariableDeclaration, IAssignmentStatement>();
        private DependencyGraph<VariableDeclaration> _dependencyGraph = new DependencyGraph<VariableDeclaration>();
        private HashSet<VariableDeclaration> _knownVariables = new HashSet<VariableDeclaration>();

        public void AddResult(VariableDeclaration dependentVariable, IAssignmentStatement setter, IReadOnlyCollection<VariableDeclaration> dependencies) {
            _setterDict.Add(dependentVariable, setter);
            _dependencyGraph.Add(dependentVariable, dependencies);
            _knownVariables.Add(dependentVariable);

            foreach (var dep in dependencies) _knownVariables.Add(dep);
        }

        public void AddResult(IEnumerable<VariableDeclaration> dependentVariables, IAssignmentStatement setter, IReadOnlyCollection<VariableDeclaration> dependencies) {
            foreach (var dependentVariable in dependentVariables) {
                _setterDict.Add(dependentVariable, setter);
                _dependencyGraph.Add(dependentVariable, dependencies);
                _knownVariables.Add(dependentVariable);
            }

            foreach (var dep in dependencies) _knownVariables.Add(dep);
        }


        public void SetSyntaxConstraint(IFormula constraint) {
            if (SyntaxConstraint != null) throw new Exception();
            SyntaxConstraint = constraint;
        }

        public IReadOnlyList<IAssignmentStatement> GetStepsInOrder() {
            var stepList = new List<IAssignmentStatement>();
            var visited = new HashSet<IAssignmentStatement>();

            foreach (var variable in _dependencyGraph.Sort()) {
                // Check that we have a setter for this variable (if not, it's a free variable)
                if (_setterDict.TryGetValue(variable, out var setter)) {
                    // Check whether we've already visited this setter (i.e., it sets multiple variables)
                    if (visited.Add(setter)) {
                        stepList.Add(setter);
                    }
                }
            }

            return stepList;
        }

        public IEnumerable<VariableDeclaration> GetFreeVariables() => _knownVariables.Where(v => !_setterDict.ContainsKey(v));
        public IEnumerable<VariableDeclaration> GetBoundVariables() => _knownVariables.Where(_setterDict.ContainsKey);

        public IEnumerable<VariableDeclaration> AllVariables => _knownVariables;

        public RuleInterpreter ToInterpreter(Nonterminal nonterminal, SemanticRelationInstance relationInstance, IProductionRewriteExpression rewriteExpression, Theory theory) {
            var terms = rewriteExpression.GetChildTerms().Select(term => new TermInfo(term.Name, term.Nonterminal)).ToList();
            
            return new RuleInterpreter(
                nonterminal,
                terms,
                ConvertArgVariables(relationInstance.Elements,theory),
                ConvertAuxVariables(_knownVariables,theory),
                GetStepsInOrder()
            );
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

        private static IReadOnlyList<VariableInfo> ConvertAuxVariables(IEnumerable<VariableDeclaration> variables, Theory theory) {
            var aux = new List<VariableInfo>();
            foreach (var varDec in variables) {
                if (PredicateAnalysisHelper.IsDeclaredAsAux(varDec.DeclarationContext)) {
                    aux.Add(new VariableInfo(varDec.Name, theory.GetType(varDec.Type)));
                }
            }

            return aux;
        }
    }
}