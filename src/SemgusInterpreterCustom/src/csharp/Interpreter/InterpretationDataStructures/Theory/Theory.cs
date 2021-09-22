using System;
using System.Collections.Generic;
using System.Linq;
using Semgus.Syntax;

namespace Semgus.Interpreter {
    /// <summary>
    /// Types and functions needed for interpretation.
    /// This class forms the connection between the interpreter and SMT-LIB2.
    /// </summary>
    public class Theory {
        private readonly IReadOnlyDictionary<string, SmtLibFunction> _functions;
        private readonly IReadOnlyDictionary<string, Type> _typeMap;

        public Theory(IReadOnlyDictionary<string, SmtLibFunction> functions, IReadOnlyDictionary<string, Type> typeMap) {
            _functions = functions;
            _typeMap = typeMap;
        }

        public static Theory BasicLibrary { get; } = MakeBasicLibrary();

        private static Theory MakeBasicLibrary() {
            var functions = new[] {
                new SmtLibFunction("&&",
                    new SmtLibFunction.TypeSignature(
                        typeof(bool),
                        new[]{typeof(bool)},
                        typeof(bool)
                    ),
                    args => {
                        for(int i = 0; i < args.Length; i++) {
                            if(!((bool) args[i])) return false;
                        }
                        return true;
                    }
                ),

                new SmtLibFunction("||  ",
                    new SmtLibFunction.TypeSignature(
                        typeof(bool),
                        new[]{typeof(bool)},
                        typeof(bool)
                    ),
                    args => {
                        for(int i = 0; i < args.Length; i++) {
                            if((bool) args[i]) return true;
                        }
                        return false;
                    }
                ),

                new SmtLibFunction("!",
                    new SmtLibFunction.TypeSignature(
                        typeof(bool),
                        new[] { typeof(bool) }
                    ),
                    args => {
                        return !((bool)args[0]);
                    }
                ),

                new SmtLibFunction("=",
                    // TODO: this should require all args to be of the same type (generic)
                    new SmtLibFunction.TypeSignature(
                        typeof(bool),
                        new[]{typeof(object)},
                        typeof(object)
                    ),
                    args => {
                        var t0 = args[0];
                        for (int i = 1; i < args.Length; i++) {
                            if (!t0.Equals(args[i])) return false;
                        }
                        return true;
                    }
                ),

                new SmtLibFunction("+",
                    new SmtLibFunction.TypeSignature(
                        typeof(int),
                        new[]{typeof(int)},
                        typeof(int)
                    ),
                    args => {
                        var a = (int)args[0];
                        for (int i = 1; i < args.Length; i++) {
                            a += (int) args[i];
                        }
                        return a;
                    }
                ),

                new SmtLibFunction("-",
                    new SmtLibFunction.TypeSignature(
                        typeof(int),
                        new[]{typeof(int), typeof(int)}
                    ),
                    args => {
                        return (int) args[0] - (int) args[1];
                    }
                ),

                new SmtLibFunction("if",
                    new SmtLibFunction.TypeSignature(
                        typeof(int),
                        new[]{typeof(bool), typeof(int), typeof(int)}
                    ),
                    args => {
                        return (bool) args[0] ? (int) args[1] : (int) args[2];
                    }
                ),
                new SmtLibFunction("<",
                    new SmtLibFunction.TypeSignature(
                        typeof(bool),
                        new[]{typeof(int), typeof(int)}
                    ),
                    args => {
                        return (int) args[0] < (int) args[1];
                    }
                ),
            }.ToDictionary(fn => fn.Name, fn => fn);

            functions.Add("not", functions["!"]);

            return new Theory(
                functions: functions,
                typeMap: new Dictionary<string, Type> {
                    {"Int", typeof(int)},
                    {"Bool", typeof(bool)}
                }
            );
        }

        public Type GetType(SemgusType type) => _typeMap[type.Name];
        internal SmtLibFunction GetFunction(LibraryFunction libraryFunction) => _functions[libraryFunction.Name];
    }
}