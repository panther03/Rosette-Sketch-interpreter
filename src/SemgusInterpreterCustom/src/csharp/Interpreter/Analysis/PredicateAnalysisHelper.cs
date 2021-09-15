using Semgus.Syntax;
using System;

namespace Semgus.Interpreter {
    public static class PredicateAnalysisHelper {
        public static bool IsDeclaredAsAux(VariableDeclaration.Context ctx) {
            switch (ctx) {
                case VariableDeclaration.Context.NT_Auxiliary:
                case VariableDeclaration.Context.PR_Auxiliary:
                    return true;

                case VariableDeclaration.Context.SF_Input:
                case VariableDeclaration.Context.SF_Output:
                    return false;

                case VariableDeclaration.Context.NT_Term:
                case VariableDeclaration.Context.PR_Subterm:
                case VariableDeclaration.Context.CT_Term:
                case VariableDeclaration.Context.CT_Auxiliary:
                    // these variable types should never reach this point
                    throw new ArgumentException();
                default:
                    throw new NotImplementedException();
            }
        }

        public static bool IsDeclaredAsInput(VariableDeclaration.Context ctx) {
            switch (ctx) {
                case VariableDeclaration.Context.SF_Input:
                    return true;
                case VariableDeclaration.Context.SF_Output:
                case VariableDeclaration.Context.NT_Auxiliary:
                case VariableDeclaration.Context.PR_Auxiliary:
                    return false;
                case VariableDeclaration.Context.NT_Term:
                case VariableDeclaration.Context.PR_Subterm:
                case VariableDeclaration.Context.CT_Term:
                case VariableDeclaration.Context.CT_Auxiliary:
                    // these variable types should never reach this point
                    throw new ArgumentException();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}