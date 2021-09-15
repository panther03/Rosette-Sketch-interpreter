using System;
using System.Collections.Generic;
using Semgus.Syntax;

namespace Semgus.Interpreter {
    public class RuleInterpreter {
        public Nonterminal Nonterminal { get; }

        public string Identifier { get; set; } = "???";

        // Nonterminals that appear in this production rule's RHS.
        // The order of this list is arbitrary (TermInfo.Name is used for identifying which term is which).
        // For leaf production rules, this list will be non-null and empty.
        public IReadOnlyList<TermInfo> SubTermSlots { get; }

        // Variables that must be declared in a higher scope during interpretation.
        // The order of this list is significant: its elements are indexed in correspondence with the variables'
        // appearance in the relevant semantic relation instance.
        // E.g., given Start.Sem(t, x, y, result), ArgumentSlots[0].Name will be "x", whereas ArgumentSlots[2].Name will be "result".
        public IReadOnlyList<ArgVariableInfo> ArgumentSlots { get; }

        // Names of the input variables, ordered by their apparance in the semantic relation instance.
        // E.g. given Example.Sem(t, in0, in1 ,out0, out1, in1, out2), InputNames will be [in0, in1, in2].
        public IReadOnlyList<string> InputNames { get; }

        // Names of the output variables, ordered by their apparance in the semantic relation instance.
        // E.g. given Example.Sem(t, in0, in1 ,out0, out1, in1, out2), OutputNames will be [out0, out1, out2].
        public IReadOnlyList<string> OutputNames { get; }

        // Variables that will be declared in the scope of this rule during interpretation.
        // The order of this list is arbitrary. If no aux variables are declared, this list will be non-null and empty.
        private readonly IReadOnlyList<VariableInfo> _auxVariables;

        // Imperative steps to perform when interpreting this rule.
        // For now, each of these is a variable assignment.
        public readonly IReadOnlyList<IAssignmentStatement> _steps;

        public RuleInterpreter(
            Nonterminal nonterminal,
            IReadOnlyList<TermInfo> subTermSlots,
            IReadOnlyList<ArgVariableInfo> argumentSlots,
            IReadOnlyList<VariableInfo> auxVariables,
            IReadOnlyList<IAssignmentStatement> steps
        ) {
            this.Nonterminal = nonterminal;
            this.SubTermSlots = subTermSlots;
            this.ArgumentSlots = argumentSlots;
            (this.InputNames, this.OutputNames) = GetInOutNames(argumentSlots);
            this._auxVariables = auxVariables;
            _steps = steps;
        }

        // Interpret a syntax node.
        // Results are stored by mutating the values of the argument variables.
        public void Interpret(IReadOnlyDictionary<string, IDSLSyntaxNode> subTerms, IReadOnlyList<VariableReference> args) {
            var evalContext = new EvaluationContext();

            // Map each provided input variable by reference
            for (int i = 0; i < args.Count; i++) {
                evalContext.MapVariable(ArgumentSlots[i].Name, args[i]);
                //evalContext.MapVariable(RelationInstance.Elements[i + 1].Name, args[i]);
            }

            // Generate a new slot for each aux variable not included in the args
            // TODO: should we throw an exception if one of these *is* defined in the args?
            for (int i = 0; i < _auxVariables.Count; i++) {
                evalContext.EnsureVariableExists(_auxVariables[i].Name);
            }

            evalContext.SetTermMap(subTerms);

            foreach (var step in _steps) {
                step.Execute(evalContext);
                Console.WriteLine("Test");
                Console.WriteLine(step.PrintCode());
            }
        }

        private static (List<string> inputNames, List<string> outputNames) GetInOutNames(IReadOnlyList<ArgVariableInfo> argumentSlots) {
            var inputNames = new List<string>();
            var outputNames = new List<string>();
            for (int i = 0; i < argumentSlots.Count; i++) {
                if (argumentSlots[i].IsInput) {
                    inputNames.Add(argumentSlots[i].Name);
                } else {
                    outputNames.Add(argumentSlots[i].Name);
                }
            }
            return (inputNames, outputNames);
        }
    }
}