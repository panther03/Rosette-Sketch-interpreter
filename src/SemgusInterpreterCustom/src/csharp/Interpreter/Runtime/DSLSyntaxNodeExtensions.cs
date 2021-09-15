using System;
using System.Collections.Generic;
using System.Linq;

namespace Semgus.Interpreter {
    public static class DSLSyntaxNodeExtensions {
        public static IReadOnlyList<object> RunProgram(this IDSLSyntaxNode node, IReadOnlyDictionary<string,object> input) {
            var argList = new List<VariableReference>();
            var outIdx = new List<int>();

            var argumentSlots = node.ArgumentSlots;
            for (int i = 0; i < argumentSlots.Count; i++) {
                var varRef = new VariableReference();
                argList.Add(varRef);

                var arg = argumentSlots[i];
                if (arg.IsInput) {
                    varRef.SetValue(input[arg.Name]);
                } else {
                    outIdx.Add(i);
                }
            }

            node.Interpret(argList);

            var outList = new List<object>(outIdx.Count);

            for (int i = 0; i < outIdx.Count; i++) {
                outList.Add(argList[outIdx[i]].Value);
            }

            return outList;
        }

        public static IReadOnlyDictionary<string, object> RunProgramReturningDict(this IDSLSyntaxNode node, IReadOnlyDictionary<string, object> input) {

            var providedInputNames = new HashSet<string>(input.Keys);
            foreach (var arg in node.ArgumentSlots) {
                if(arg.IsInput && !providedInputNames.Remove(arg.Name)) {
                    throw new ArgumentException($"Missing required input argument \"{arg.Name}\"");
                }
            }

            if (providedInputNames.Count > 0) {
                throw new ArgumentException($"Unexpected input argument(s) {string.Join(",", providedInputNames)}");
            }

            var outList = RunProgram(node, input);

            var outDict = new Dictionary<string, object>();

            int k = 0;
            foreach (var arg in node.ArgumentSlots) {
                if (!arg.IsInput) {
                    outDict.Add(arg.Name, outList[k]);
                    k++;
                }
            }

            if (outDict.Count != outList.Count) throw new Exception();

            return outDict;
        }
    }
}