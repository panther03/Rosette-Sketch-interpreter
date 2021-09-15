//using System;
//using System.Collections.Generic;
//using System.Linq;
//using InterpreterExample;
//using Semgus.Enumerator;
//using Semgus.Interpreter;
//using Semgus.Syntax;
//using Semgus.Util;

//namespace EnumeratorExample {
//    public static class Test0 {


//        public static void Run() {
//            var sem = new ExampleSemgus();

//            var theory = Theory.BasicLibrary;
//            var analyzer = new RoughFunctionalAnalyzer(theory,new[] { sem.ri_start_sem });

//            var result_add = analyzer.AnalyzePredicate(sem.predicate_add);
//            var result_leaf_x = analyzer.AnalyzePredicate(sem.predicate_leaf_x);
//            var result_leaf_y = analyzer.AnalyzePredicate(sem.predicate_leaf_y);

//            Log("--- input: ---");
//            Log(sem.predicate_add.PrintFormula());
//            Log("--- result: ---");

//            foreach (var step in result_add.GetStepsInOrder()) {
//                Log(step.PrintCode());
//            }
//            Log("--- free variables: ---");
//            Log(string.Join(", ", result_add.GetFreeVariables().Select(v => v.Name)));
//            Log("--- bound variables: ---");
//            Log(string.Join(", ", result_add.GetBoundVariables().Select(v => v.Name)));
//            Log("--- done ---");

//            var interpreter_add = result_add.ToInterpreter(sem.nt_start, sem.ri_start_sem, sem.rew_add, theory);
//            interpreter_add.Identifier = FormatIdentifier(sem.nt_start, sem.rew_add);

//            var interpreter_leaf_x = result_leaf_x.ToInterpreter(sem.nt_start, sem.ri_start_sem, sem.rew_leaf_x, theory);
//            interpreter_leaf_x.Identifier = FormatIdentifier(sem.nt_start, sem.rew_leaf_x);

//            var interpreter_leaf_y = result_leaf_y.ToInterpreter(sem.nt_start, sem.ri_start_sem, sem.rew_leaf_y, theory);
//            interpreter_leaf_y.Identifier = FormatIdentifier(sem.nt_start, sem.rew_leaf_y);

//            var additionExample = new DSLSyntaxNode(interpreter_add, new Dictionary<string, IDSLSyntaxNode> {
//                {"t1", new DSLSyntaxNode(interpreter_leaf_x)},
//                {"t2", new DSLSyntaxNode(interpreter_leaf_y)}
//            });

//            Log("--- testing program ---");

//            var result = additionExample.RunProgramReturningDict(new Dictionary<string, object> { { "x", 5 }, { "y", 7 } });

//            foreach (var kvp in result) {
//                Log($"{kvp.Key}: {kvp.Value}");
//            }

//            Log(additionExample.ToString());

//            var ld_leaf = new DictOfList<Nonterminal, RuleInterpreter>();
//            var ld_branch = new DictOfList<Nonterminal, RuleInterpreter>();

//            ld_leaf.Add(sem.nt_start, interpreter_leaf_x);
//            ld_leaf.Add(sem.nt_start, interpreter_leaf_y);
//            ld_branch.Add(sem.nt_start, interpreter_add);
//            var grammar = new InterpretationGrammar(ld_leaf, ld_branch);

//            Log("--- enumerate ---");


//            var enumerator = new BottomUpEnumerator() { Log = ConsoleLogger.Instance };
//            int k = 0;
//            var bank = new ExpressionBank__OLD(grammar);
//            foreach (var term in enumerator.EnumerateAll(grammar, bank, EmptyEquivalenceCheck.Instance, 3)) {
//                Log($"{k,-4} {term}");
//                k++;
//            }
//            Log($"\tBANK SIZE {bank.Size}");

//            Log("--- enumerate (sel) ---");

//            // Goal: x + x + y
//            Func<object, object, object> synth_goal = (object x, object y) => (int)x + (int)x + (int)y;

//            var n_examples = 10;
//            var rand = new Random();
//            var synth_input_x = PopulateArray<object>(() => rand.Next(), n_examples);
//            var synth_input_y = PopulateArray<object>(() => rand.Next(), n_examples);
//            var synth_output_r = new object[n_examples];
//            for (int i = 0; i < n_examples; i++) synth_output_r[i] = synth_goal(synth_input_x[i], synth_input_y[i]);

//            var synth_out_name = "result";
//            var synth_spec = new InductiveConstraint(BehaviorExample.Zip(
//                new Dictionary<string, object[]> { { "x", synth_input_x }, { "y", synth_input_y } },
//                new Dictionary<string, object[]> { { synth_out_name, synth_output_r } }
//            ));

//            var equiv_x = new ObservationalEquivalenceCheck(synth_spec.Examples.Select(e => e.Input).ToList());

//            k = 0;
//            bank = new ExpressionBank__OLD(grammar);
//            foreach (var term in enumerator.EnumerateAll(grammar, bank, equiv_x, 3)) {
//                Log($"{k,-4} {term}");
//                k++;
//            }
//            Log($"\tBANK SIZE {bank.Size}");
//            foreach (var expr in bank.EnumerateContents()) {
//                Log($"{expr,-12}");
//            }
//            Log("--- synthesis ---");

//            equiv_x.Reset();
//            var synthesisResult = enumerator.Synthesize(grammar, synth_spec, equiv_x);
//            Log(synthesisResult.ToString());

//            System.Console.ReadKey();
//        }

//        static T[] PopulateArray<T>(Func<T> generator, int length) {
//            var array = new T[length];
//            for (int i = 0; i < length; i++) {
//                array[i] = generator();
//            }
//            return array;
//        }

//        static string FormatIdentifier(Nonterminal nt, IProductionRewriteExpression rew) =>
//            $"{rew.GetNamingSymbol()}";

//        static void Log(string s) => System.Console.WriteLine(s);
//    }
//}
