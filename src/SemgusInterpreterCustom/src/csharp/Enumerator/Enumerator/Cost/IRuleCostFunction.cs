using Semgus.Interpreter;

namespace Semgus.Enumerator {
    public interface IRuleCostFunction {
        int GetCost(RuleInterpreter rule);
    }
}