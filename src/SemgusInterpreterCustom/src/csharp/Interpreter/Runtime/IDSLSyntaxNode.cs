using Semgus.Syntax;
using System.Collections.Generic;

namespace Semgus.Interpreter {
    public interface IDSLSyntaxNode {
        Nonterminal Nonterminal { get; }
        IReadOnlyList<ArgVariableInfo> ArgumentSlots { get; }
        IReadOnlyList<string> OutputNames { get; }

        int Size { get; }
        int Height { get; }

        void Interpret(IReadOnlyList<VariableReference> argVars);

        // TODO replace this with visitor pattern
        HashSet<RuleInterpreter> GatherRulesInTree(HashSet<RuleInterpreter> inout);
    }
}
