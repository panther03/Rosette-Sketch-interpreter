using Semgus.Syntax;

namespace Semgus.Interpreter {
    /// <summary>
    /// Evaluate some formula (not containing child terms) and assign the result to a specific variable.
    /// </summary>
    internal class AssignToLocalFormulaEvaluation : IAssignmentStatement {
        private readonly string _resultVarName;
        private readonly ISmtLibExpression _expression;

        public AssignToLocalFormulaEvaluation(string resultVarName, ISmtLibExpression expression) {
            this._resultVarName = resultVarName;
            this._expression = expression;
        }

        public void Execute(EvaluationContext context) {
            var result = context.GetVariable(_resultVarName);
            result.SetValue(_expression.Evaluate(context));
        }

        public string PrintCode() => _expression.Formula.PrintFormula();
    }
}