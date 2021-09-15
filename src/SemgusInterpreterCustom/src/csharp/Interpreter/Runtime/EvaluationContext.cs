using System;
using System.Collections.Generic;

namespace Semgus.Interpreter {
    /// <summary>
    /// Collection of child terms and variable bindings necessary to evaluate a single syntax node.
    /// Interpretation will take effect by setting the values of some variables.
    /// </summary>
    public class EvaluationContext {
        private readonly Dictionary<string,VariableReference> _variableMap = new Dictionary<string, VariableReference>();
        private IReadOnlyDictionary<string, IDSLSyntaxNode> _termMap;
        
        public VariableReference GetVariable(string name) => _variableMap[name];
        public IDSLSyntaxNode GetChildNode(string name) => _termMap[name];

        internal void MapVariable(string name, VariableReference variableReference) {
            _variableMap.Add(name,variableReference);
        }

        internal void EnsureVariableExists(string name) {
            if(!_variableMap.ContainsKey(name)) _variableMap.Add(name,new VariableReference());
        }

        internal void SetTermMap(IReadOnlyDictionary<string, IDSLSyntaxNode> termMap) {
            if(!(_termMap is null)) throw new InvalidOperationException();
            _termMap = termMap;
        }
    }
}