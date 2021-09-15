using System.Collections.Generic;
using System.Linq;
using Semgus.Syntax;

namespace Semgus.Interpreter {
    /// <summary>
    /// Evaluate some child term on a collection of variables.
    /// </summary>
    internal class AssignToInnerTermEvaluation : IAssignmentStatement {
        private readonly NonterminalTermDeclaration _termVar;
        private readonly IReadOnlyList<string> _argVarNames;

        public AssignToInnerTermEvaluation(NonterminalTermDeclaration termVar, IReadOnlyList<string> argVarNames) {
            this._termVar = termVar;
            this._argVarNames = argVarNames;
        }

        public void Execute(EvaluationContext context) {
            var tNode = context.GetChildNode(_termVar.Name);

            // Note: in general, the elements of this semantic relation instance could be arbitrary SMT-LIB2 formulas.
            // For now, we constrain them to be plain variables references.

            var argVars = new VariableReference[_argVarNames.Count];
            for(int i = 0; i < _argVarNames.Count; i++) {
                argVars[i] = context.GetVariable(_argVarNames[i]);
            }

            tNode.Interpret(argVars);
        }

        // assume lest element is output for now
        public string PrintCode() => $"(define {_argVarNames[^1]} ({_termVar.Nonterminal}.Sem {_termVar.Name} {string.Join(" ", _argVarNames.Take(_argVarNames.Count-2))}))";
    }
}