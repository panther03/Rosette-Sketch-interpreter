using Semgus.Interpreter;
using Semgus.Parser;
using Semgus.Syntax;
using System;
using System.IO;
using System.Collections.Generic;

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

            var printer = HEADER + "\n" + AdtBuilder.BuildAdtRepr(grammar) + "\n";
            //                            + SyntaxGenPass.BuildSyntaxGenFns(grammar) + "\n"
            //                            + SemGenPass.BuildSemGenFns(grammar) + "\n"
            //                            + ConstraintGenPass.BuildConstraints(spec);
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
