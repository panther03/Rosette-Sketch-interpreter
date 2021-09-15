using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Semgus.Interpreter {
    /// <summary>
    /// Interpreted function sourced from a theory.
    /// </summary>
    public class SmtLibFunction {
        public class TypeSignature {
            public Type ResultType {get;}
            private readonly Type[] _requiredArgTypes;
            private readonly Type _starArgType;

            public TypeSignature(Type resultType, Type[] requiredArgTypes, Type starArgType = null) {
                this.ResultType = resultType;
                this._requiredArgTypes = requiredArgTypes;
                this._starArgType = starArgType;
            }
            
            public bool TypeCheck(IReadOnlyList<ISmtLibExpression> args) {
                if(_starArgType is null) {
                    if(args.Count != _requiredArgTypes.Length) return false;
                } else {
                    if(args.Count < _requiredArgTypes.Length) return false;
                }
                
                int k = 0;
                
                for(; k < _requiredArgTypes.Length; k++) {
                    if(!args[k].ResultType.IsAssignableTo(_requiredArgTypes[k])) return false;
                }
                for(; k < args.Count; k++) {
                    if(!args[k].ResultType.IsAssignableTo(_starArgType)) return false;
                }
                
                return true;
            }

            public string PrettyPrintInputs() {
                var sb = new StringBuilder();
                sb.Append("[");
                sb.Append(string.Join(", ", _requiredArgTypes.Select(t => t.Name)));

                if (_starArgType != null) {
                    if (_requiredArgTypes.Length > 0) {
                        sb.Append(", ");
                    }
                    sb.Append(_starArgType.Name);
                    sb.Append("*");
                }
                sb.Append("]");
                return sb.ToString();
            }
        }

        public delegate object Evaluator(object[] args);

        public string Name { get; }
        public TypeSignature Signature { get; }
        public Evaluator Evaluate { get; }

        public SmtLibFunction(string name, TypeSignature signature, Evaluator evaluate) {
            Name = name;
            Signature = signature;
            Evaluate = evaluate;
        }

        public void AssertTypeCheck(IReadOnlyList<ISmtLibExpression> args) {
            if (!Signature.TypeCheck(args)) throw new Exception($"Function {Name} expected types {Signature.PrettyPrintInputs()}, got [{string.Join(", ", args.Select(a => a.ResultType.Name))}]");
        }

    }
}