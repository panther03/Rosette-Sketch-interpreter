using Semgus.Interpreter;

namespace Semgus.Enumerator {
    public class SynthesisResult {
        public bool Success { get; }
        public IDSLSyntaxNode Result { get; }
        public int BankSize { get; }
        public int Cost { get; }
        public int NumChecked { get; set; }

        public SynthesisResult(bool success, IDSLSyntaxNode result, int bankSize, int cost, int numChecked) {
            Success = success;
            Result = result;
            BankSize = bankSize;
            Cost = cost;
            NumChecked = numChecked;
        }

        public override string ToString() {
            if(Success) {
                return $"[b={BankSize},c={Cost},n={NumChecked}] {Result}]";
            } else {
                return $"[b={BankSize},c={Cost},n={NumChecked}] no match]";
            }
        }
    }
}