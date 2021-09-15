using Semgus.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Semgus.Interpreter {
    public class DSLSyntaxNode : IDSLSyntaxNode {
        public class Factory : INodeFactory {
            public static Factory Instance { get; } = new Factory();

            public IDSLSyntaxNode Instantiate(RuleInterpreter rule) => new DSLSyntaxNode(rule);

            public IDSLSyntaxNode Instantiate(RuleInterpreter rule, IDSLSyntaxNode[] subTerms) { 
                var n = subTerms?.Length ?? 0;
                var slots = rule.SubTermSlots;
                if (n != slots.Count) throw new IndexOutOfRangeException();

                var childNodes = new Dictionary<string, IDSLSyntaxNode>();

                for (int i = 0; i < n; i++) {
                    // TODO: check that nonterminals match
                    childNodes.Add(slots[i].Name, subTerms[i]);
                }

                return new DSLSyntaxNode(
                    interpreter: rule,
                    childNodes: childNodes
                );
            }
        }

        private readonly RuleInterpreter _interpreter;
        private readonly IReadOnlyDictionary<string, IDSLSyntaxNode> _childNodes;

        public Nonterminal Nonterminal => _interpreter.Nonterminal;
        public IReadOnlyList<ArgVariableInfo> ArgumentSlots => _interpreter.ArgumentSlots;
        public IReadOnlyList<string> OutputNames => _interpreter.OutputNames;

        public int Size { get; }
        public int Height { get; }

        public DSLSyntaxNode(RuleInterpreter interpreter, IReadOnlyDictionary<string, IDSLSyntaxNode> childNodes = null) {
            _interpreter = interpreter;
            _childNodes = childNodes ?? new Dictionary<string, IDSLSyntaxNode>();

            if(_childNodes.Count==0) {
                Size = 1;
                Height = 1;
            } else {
                int s = 0;
                int h = 0;

                foreach(var node in _childNodes.Values) {
                    s += node.Size;
                    h = Math.Max(h, node.Height);
                }

                Size = 1 + s;
                Height = 1 + h;
            }
        }

        public override string ToString() {
            if (_childNodes.Count == 0) {
                return _interpreter.Identifier;
            } else {
                return $"({_interpreter.Identifier}{string.Join("", _childNodes.Select(kvp => $" {kvp.Value}"))})";
            }
        }

        public void Interpret(IReadOnlyList<VariableReference> argVars) => _interpreter.Interpret(_childNodes, argVars);

        public HashSet<RuleInterpreter> GatherRulesInTree(HashSet<RuleInterpreter> inout) {
            inout.Add(_interpreter);
            foreach (var c in _childNodes.Values) c.GatherRulesInTree(inout);
            return inout;
        }
    }
}