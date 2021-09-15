using System;

namespace Semgus.Interpreter {
    /// <summary>
    /// Slot to receive the value assigned to a variable.
    /// May correspond to an input or output.
    /// </summary>
    public class VariableReference {
        public bool HasValue { get; private set; } = false;
        public object Value => HasValue ? _value : throw new InvalidOperationException();
        private object _value;

        /// <summary>
        /// Set this variable equal to some value.
        /// This can only be done if the variable currently has no value.
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(object value) {
            if (HasValue) throw new InvalidOperationException();
            _value = value;
            HasValue = true;
        }
    }
}