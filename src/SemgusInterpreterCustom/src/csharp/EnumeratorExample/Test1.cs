using Semgus.Enumerator;
using Semgus.Interpreter;
using Semgus.Syntax;
using Semgus.Util;
using System;
using System.Diagnostics;

namespace EnumeratorExample {
    public static class Test1 {
        public static void Run(string fpath) {
            SemgusProblem ast;
            try {
                (ast, _) = Parser.ParseFileToAst(fpath);
            } catch(FileContextSemgusSyntaxException e) {
                Log(e.Message);
                Log(e.FileContext);
                System.Console.ReadKey();
                return;
            }

            var grammar = InterpretationGrammar.FromAst(ast, Theory.BasicLibrary);



            Log("----- synthesis (height) -----");

            {
                var (spec, check) = new RoughConstraintAnalyzer().Analyze(ast);
                var synthesizer = new HeightGuidedSynthesis(grammar) { Log = ConsoleLogger.Instance };

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var synthesisResult = synthesizer.PerformSynthesis(spec, check);
                stopwatch.Stop();

                Log(synthesisResult.ToString());
                Log($"(took {stopwatch.ElapsedMilliseconds * 0.001} sec)");
            }

            Log("----- synthesis (size) -----");
            {
                var (spec, check) = new RoughConstraintAnalyzer().Analyze(ast);
                var costGrammar = GrammarWithCosts.Construct(grammar, SizeCostFunction.Instance);
                var synthesizer = new CostGuidedSynthesis(costGrammar, 100) { Log = ConsoleLogger.Instance };

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var synthesisResult = synthesizer.PerformSynthesis(spec, check);
                stopwatch.Stop();

                Log(synthesisResult.ToString());
                Log($"(took {stopwatch.ElapsedMilliseconds * 0.001} sec)");
            }

            Log("----- synthesis (prob) -----");
            {
                var (spec, check) = new RoughConstraintAnalyzer().Analyze(ast);

                var synthesizer = new JitProbGuidedSynthesis(grammar, 24, 1, TimeSpan.FromMinutes(5));// { Log = ConsoleLogger.Instance };

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var synthesisResult = synthesizer.PerformSynthesis((InductiveConstraint) spec, check);
                stopwatch.Stop();
                
                Log(synthesisResult.ToString());
                Log($"(took {stopwatch.ElapsedMilliseconds * 0.001} sec)");
            }
            //Log("----- terms in cache: -----");

            //Log(((ObservationalEquivalenceCheck)check).PrettyPrint());
            System.Console.ReadKey();
        }

        static void Log(string s) => System.Console.WriteLine(s);
    }
}
