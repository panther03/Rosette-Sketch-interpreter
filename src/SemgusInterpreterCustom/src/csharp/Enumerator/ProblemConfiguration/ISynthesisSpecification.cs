using Semgus.Interpreter;

namespace Semgus.Enumerator {
    public interface ISynthesisSpecification {
        bool IsSatisfiedBy(IDSLSyntaxNode node);
    }
}
