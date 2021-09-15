namespace Semgus.Interpreter {
    public interface INodeFactory {
        IDSLSyntaxNode Instantiate(RuleInterpreter rule);
        IDSLSyntaxNode Instantiate(RuleInterpreter rule, IDSLSyntaxNode[] subTerms);
    }
}