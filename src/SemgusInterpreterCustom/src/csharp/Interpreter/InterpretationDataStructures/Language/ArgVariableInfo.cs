using System;

namespace Semgus.Interpreter {
    /// <summary>
    /// Info about a variable that is present in a node's associated semantic relation instance.
    /// </summary>
    public class ArgVariableInfo : VariableInfo {
        public bool IsInput { get; }

        public ArgVariableInfo(string name, Type type, bool isInput) : base(name, type) {
            this.IsInput = isInput;
        }
    }
}