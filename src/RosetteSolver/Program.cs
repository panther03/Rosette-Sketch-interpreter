using Semgus.Interpreter;
using Semgus.Parser;
using Semgus.Syntax;
using System;
using System.Linq;
using Semgus.Util;
using System.IO;

// normal to have to include this for InductiveConstraintAnalyzer?
using Semgus.Solvers.Enumerative;


namespace Semgus.Solver.Rosette {
    class Program {

        public static readonly string HEADER = @"#lang rosette

(require
  rosette/lib/match
  rosette/lib/destruct
  rosette/lib/angelic
  rosette/lib/synthax)

(current-bitwidth #f)

; hack because i can't get the interpreter to generate #t and #f
(define True #t)
(define False #f)";

        static void Main(string[] args) {
            if (args.Length != 2) {
                Console.Error.WriteLine("Expects two arguments: a Semgus file to parse, and an output racket file.");
            }
            
            var infile = args[0];
            var outfile = args[1];

            var theory = BasicLibrary.Instance; // default theory
            var constraintAnalyzer = new InductiveConstraintAnalyzer(theory);

            var ast = Parse(infile);
            var grammar = InterpretationGrammar.FromAst(ast, theory);
            var spec = constraintAnalyzer.Analyze(ast);

            var combinedTerms = combineTerms(grammar);

            var printer = HEADER + "\n" + AdtBuilder.BuildAdtRepr(combinedTerms) + "\n"
                                        + SyntaxBuilder.BuildSyntaxGenFns(combinedTerms) + "\n";
            //                            + SemGenPass.BuildSemGenFns(grammar) + "\n"
            //                            + ConstraintGenPass.BuildConstraints(spec);

            File.WriteAllText("outfile", printer);
        }

        static DictOfList<Nonterminal, ProductionRuleInterpreter> combineTerms(InterpretationGrammar grammar) {
            var AllTerms = new DictOfList<Nonterminal, ProductionRuleInterpreter>();
            // makes assumption that branchterms and leafterms have the same nonterminals
            var AllKeys = grammar.BranchTerms.Keys;
            foreach (var key in grammar.LeafTerms.Keys) {
                if (grammar.BranchTerms.ContainsKey(key)) {
                    AllTerms.AddCollection(key,(grammar.BranchTerms[key].Concat(grammar.LeafTerms[key])).ToList());
                } else {
                    AllTerms.AddCollection(key,grammar.LeafTerms[key]);
                }
            }
            return AllTerms;
        }

        public static SemgusProblem Parse(string filePath) {
            using var writer = new StringWriter();

            if (new SemgusParser(filePath).TryParse(out var ast, writer)) {
                return ast;
            } else {
                throw new Exception(writer.ToString());
            }
        }
    }
}
