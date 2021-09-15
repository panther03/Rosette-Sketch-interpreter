using System;
using Semgus.Syntax;

namespace Semgus.Interpreter {
    public class VariableEvalExpression : ISmtLibExpression {
        public IFormula Formula { get; }
        public Type ResultType { get; }

        public string VariableName {get;}

        public VariableEvalExpression(IFormula formula, string varName, Type varType) {
            this.Formula = formula;
            this.ResultType = varType;
            this.VariableName = varName;
        }

        public object Evaluate(EvaluationContext context) => context.GetVariable(VariableName).Value;
    }
}