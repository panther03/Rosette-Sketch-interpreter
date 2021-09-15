using Semgus.Syntax;

namespace Semgus.Interpreter {
    public interface IAssignmentStatement {
        /// <summary>
        /// Print some imperative pseudocode to represent this statement.
        /// </summary>
        /// <returns></returns>
        string PrintCode();

        /// <summary>
        /// Execute the statement in the given evaluation context.
        /// </summary>
        /// <param name="context"></param>
        void Execute(EvaluationContext context);
    }
}