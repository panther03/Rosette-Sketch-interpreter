using Semgus.Interpreter;

namespace Semgus.Enumerator {
    public class EmptyEquivalenceCheck : IProgramEquivalenceCheck {
        public static EmptyEquivalenceCheck Instance { get; } = new EmptyEquivalenceCheck();
        public bool TryInclude(IDSLSyntaxNode node) => true;
        public void Reset() { }
    }
}