using System;
using Semgus.Syntax;

namespace Semgus.Interpreter {
    public class LiteralExpression : ISmtLibExpression {
        public IFormula Formula { get; }
        public Type ResultType { get; }
        
        private readonly object _boxedValue;

        public LiteralExpression(IFormula formula, object boxedValue, Type valueType) {
            this.Formula = formula;
            this.ResultType = valueType;
            this._boxedValue = boxedValue;
        }

        public object Evaluate(EvaluationContext context) => _boxedValue;
    }
}