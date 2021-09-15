using Semgus.Syntax;

namespace Semgus.Interpreter {
    /// <summary>
    /// Information about a child node that appears in a production rule's rewrite expression.
    /// </summary>
    public class TermInfo {
        public string Name { get; }
        public Nonterminal Nonterminal { get; }

        public TermInfo(string name, Nonterminal nonterminal) {
            Name = name;
            Nonterminal = nonterminal;
        }
    }
}