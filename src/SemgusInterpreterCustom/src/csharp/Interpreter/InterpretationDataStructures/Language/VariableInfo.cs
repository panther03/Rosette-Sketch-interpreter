using System;

namespace Semgus.Interpreter {
    /// <summary>
    /// Indicates the name and type of a variable that is referenced in a production rule's semantics.
    /// (not including child terms)
    /// </summary>
    public class VariableInfo {
        public string Name { get; }
        public Type Type { get; }

        public VariableInfo(string name, Type type) {
            Name = name;
            Type = type;
        }
    }
}