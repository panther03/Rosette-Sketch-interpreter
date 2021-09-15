using System;
using Semgus.Syntax;

namespace Semgus.Interpreter {
    public interface ISmtLibExpression {
        IFormula Formula { get; }
        Type ResultType { get; }
        object Evaluate(EvaluationContext context);
    }
}