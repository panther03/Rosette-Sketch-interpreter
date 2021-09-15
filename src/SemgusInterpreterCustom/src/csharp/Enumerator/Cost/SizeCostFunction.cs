using Semgus.Interpreter;

namespace Semgus.Enumerator {
    public class SizeCostFunction : IRuleCostFunction {
        public static SizeCostFunction Instance { get; } = new SizeCostFunction();

        public int GetCost(RuleInterpreter rule) => 1;
    }
}