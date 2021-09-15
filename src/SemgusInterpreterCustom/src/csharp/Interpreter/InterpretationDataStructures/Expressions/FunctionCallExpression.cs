using System;
using System.Collections.Generic;
using Semgus.Syntax;

namespace Semgus.Interpreter {
    public class FunctionCallExpression : ISmtLibExpression {
        public IFormula Formula { get; }
        private readonly SmtLibFunction _function;
        private readonly IReadOnlyList<ISmtLibExpression> _args;

        public Type ResultType => _function.Signature.ResultType;

        public FunctionCallExpression(IFormula formula, SmtLibFunction function, IReadOnlyList<ISmtLibExpression> args) {
            this.Formula = formula;

            function.AssertTypeCheck(args);

            this._function = function;
            this._args = args;
        }

        public object Evaluate(EvaluationContext context) {
            var concreteArgs = new object[_args.Count];
            for (int i = 0; i < _args.Count; i++) {
                concreteArgs[i] = _args[i].Evaluate(context);
            }
            return _function.Evaluate(concreteArgs);
        }
    }
}