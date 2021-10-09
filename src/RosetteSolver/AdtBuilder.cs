using Semgus.Util;
using Semgus.Interpreter;

namespace Semgus.Solver.Rosette {
    class AdtBuilder{
        private static readonly CodeTextBuilder _builder = new CodeTextBuilder();
        public static string BuildAdtRepr(InterpretationGrammar g) {
        
            // implement
            
            return _builder.ToString();
        }
    }
}