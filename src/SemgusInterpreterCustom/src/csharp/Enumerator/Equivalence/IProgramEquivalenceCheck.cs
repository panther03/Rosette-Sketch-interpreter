using Semgus.Interpreter;

namespace Semgus.Enumerator {
    public interface IProgramEquivalenceCheck {
        bool TryInclude(IDSLSyntaxNode node);
        void Reset();
    }
}